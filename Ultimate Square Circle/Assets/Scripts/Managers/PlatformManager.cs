using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance;

    private List<ScriptableItem> _items;
    
    [SerializeField] private GameObject cam;

    public int itemsNbrToShow;

    [SerializeField] private List<BaseObject> objectsToPlace;
    private int selectedObjectIndex;
    [SerializeField] private bool inObjectSelection;
    
    public PlatformState platformState;
    public static event Action<PlatformState> OnPlatformStateChanged;
    
    [SerializeField] private GameObject objectBox;
    [SerializeField] private GameObject parentObject;
    
    private void Awake()
    {
        instance = this;
        
        OnPlatformStateChanged += state => Debug.Log($"Platform state changed to {state}"); 
        
        _items = Resources.LoadAll<ScriptableItem>("Objects").ToList();
    }

    private void Start()
    {
        objectsToPlace = new List<BaseObject>();
    }

    private void Update()
    {
        if (platformState == PlatformState.SelectObject) // Select object
        {
            BlinkObject();
            if (Input.GetMouseButtonDown(0))
            {
                SelectRay();
            }
        }
        
        if (platformState == PlatformState.PlaceObject) // Place object
        {
            // Object follows the cursor
            Vector2 posOnScreen = cam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            objectsToPlace[selectedObjectIndex].transform.position = new Vector3(posOnScreen.x + objectsToPlace[selectedObjectIndex].transform.localScale.x, posOnScreen.y + objectsToPlace[selectedObjectIndex].transform.localScale.y, 0);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject();
            }
        }
    }
    
    // private T GetRandomItem<T>(Item item) where T : BaseObject
    // {
    //     return (T)_items.Where(i => i.item == item).OrderBy(o => Random.value).First().objectPrefab;
    // }
    
    public T GetRandomPlatform<T>(params Item[] items) where T : BasePlatform
    {
        return (T)_items.Where(i => items.Contains(i.item)).OrderBy(o => Random.value).First().objectPrefab;
    }
    
    public T GetRandomItem<T>(params Item[] items) where T : BaseObject
    {
        return (T)_items.Where(i => items.Contains(i.item)).OrderBy(o => Random.value).First().objectPrefab;
    }

    public T GetRandomTrap<T>(Item item) where T : BaseTrap
    {
        return (T)_items.Where(i => i.item == item).OrderBy(o => Random.value).First().objectPrefab;
    }
    
    // public T GetRandomTrap<T>(params Item[] items) where T : BaseTrap
    // {
    //     return (T)_items.Where(i => items.Contains(i.item)).OrderBy(o => Random.value).First().objectPrefab;
    // }
    
    // Spawn the objects to place
    private void SpawnObjects()
    {
        List<Vector3> usedPositions = new List<Vector3>();
        Bounds boxBounds = objectBox.GetComponent<Collider2D>().bounds;

        for (int i = 0; i < itemsNbrToShow; i++)
        {
            var randomItem = GetRandomPlatform<BasePlatform>(Item.Platform);
            var spawnedObject = Instantiate(randomItem, parentObject.transform);
            Vector3 newPosition;

            // Check if the position is already used
            do
            {
                newPosition = new Vector3(
                    Random.Range(boxBounds.min.x, boxBounds.max.x),
                    Random.Range(boxBounds.min.y, boxBounds.max.y),
                    -1
                );
            } while (usedPositions.Any(p => Vector3.Distance(p, newPosition) < 2.0f));

            spawnedObject.transform.position = newPosition;
            usedPositions.Add(newPosition);
            objectsToPlace.Add(spawnedObject);
        }

        ChangeState(PlatformState.SelectObject);
    }
    
    // Define the parameters to make the object selection possible
    public void SelectObject()
    {
        selectedObjectIndex = -1;
        Cursor.visible = true;
    }

    // Select the object to place
    private void SelectRay()
    {
        RaycastHit2D hit = Physics2D.Raycast(cam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition), Vector2.zero); // Raycast from the mouse position

        if(hit.collider != null)
        {
            for (int i = 0; i < objectsToPlace.Count(); i++)
            {
                if (hit.collider.GetComponent<BaseObject>() == objectsToPlace[i])
                {
                    Debug.Log($"Object {objectsToPlace[i].gameObject.name} is selected");
                    objectsToPlace[i].isSelectedToPlace = true;
                    selectedObjectIndex = i;
                    
                    for (int j = 0; j < objectsToPlace.Count; j++) // Destroy all other objects
                    {
                        if (j != selectedObjectIndex)
                        {
                            Destroy(objectsToPlace[j].gameObject);
                        }
                    }
                    objectsToPlace[selectedObjectIndex].GetComponent<SpriteRenderer>().material.color = Color.white;
                    ChangeState(PlatformState.PlaceObject);
                    break;
                }
            }
        }
    }
    
    // Place the object on the grid
    public void PlaceObject()
    {
        if (selectedObjectIndex < 0 || selectedObjectIndex >= objectsToPlace.Count)
        {
            Debug.LogError("selectedObjectIndex is out of range.");
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(cam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition), Vector3.zero);

        if (hit.collider != null)
        {
            if (hit.collider.GetComponent<Tile>() == null) return;
            var tile = hit.collider.GetComponent<Tile>();
            var obj = objectsToPlace[selectedObjectIndex].GetComponent<BaseObject>();

            if (tile != null && obj != null && tile.Placeable && obj.isSelectedToPlace)
            {
                obj.transform.position = tile.transform.position;
                obj.occupiedTile = tile;
                obj.isPlaced = true;
                obj.isSelectedToPlace = false;
                tile.occupiedObject = obj;
                objectsToPlace.Clear();
                selectedObjectIndex = -1;
                ChangeState(PlatformState.End);
            }
        }
    }
    
    private void BlinkObject()
    {
        foreach (var platform in objectsToPlace)
        {
            Color color = platform.GetComponent<SpriteRenderer>().material.color;
            color.a = Mathf.PingPong(Time.time, 1);
            platform.GetComponent<SpriteRenderer>().material.color = color;
        }
    }
    
    // Change the state of the platform phase
    public void ChangeState(PlatformState newState)
    {
        platformState = newState;
        switch (newState)
        {
            case PlatformState.None: // Not doing anything related to the platforms
                GridManager.instance.HideGrid();
                Cursor.visible = false;
                break;
            case PlatformState.ShowObject: // Show the object to place
                objectBox.SetActive(true);
                GridManager.instance.ShowGrid();
                SpawnObjects();
                break;
            case PlatformState.SelectObject: // Select the object to place
                SelectObject();
                break;
            case PlatformState.PlaceObject: // Place the object on the grid
                objectBox.SetActive(false);
                PlaceObject();
                break;
            case PlatformState.End: // End the platform phase
                GameManager.instance.ChangeState(GameState.AiObjectSystem);
                ChangeState(PlatformState.None);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        
        OnPlatformStateChanged?.Invoke(newState);
    }
}

public enum PlatformState
{
    None,
    ShowObject,
    SelectObject,
    PlaceObject,
    End
}
