using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance;

    private List<ScriptableItem> _items;
    
    [SerializeField] private Transform cam;

    public int itemsNbrToShow;

    [SerializeField] private List<BaseObject> objectsToPlace;
    private int selectedObjectIndex;
    [SerializeField] private bool inObjectSelection;
    
    public PlatformState platformState;
    public static event Action<PlatformState> OnPlatformStateChanged;
    
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
        if (platformState == PlatformState.SelectObject && Input.GetMouseButtonDown(0))
        {
            SelectRay();
        }
        
        if (platformState == PlatformState.PlaceObject && Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }
    
    private T GetRandomItem<T>(Item item) where T : BaseObject
    {
        return (T)_items.Where(i => i.item == item).OrderBy(o => Random.value).First().objectPrefab;
    }
    
    private T GetRandomItem2<T>(params Item[] items) where T : BaseObject
    {
        return (T)_items.Where(i => items.Contains(i.item)).OrderBy(o => Random.value).First().objectPrefab;
    }
    
    public void SpawnObjects()
    {
        List<Vector3> usedPositions = new List<Vector3>();
        
        for (int i = 0; i < itemsNbrToShow; i++)
        {
            var randomItem = GetRandomItem2<BaseObject>(Item.Platform, Item.Trap);
            var spawnedObject = Instantiate(randomItem);
            Vector3 pos = cam.position;
            float size = cam.GetComponent<Camera>().orthographicSize;
            Vector3 newPosition;

            do
            {
                newPosition = new Vector3((int)Random.Range(pos.x - size / 2, pos.x + size / 2), (int)Random.Range(pos.y - size / 2, pos.y + size / 2), 0);
            } while (usedPositions.Any(p => Vector3.Distance(p, newPosition) < 2.0f));
            spawnedObject.transform.position = newPosition;
            usedPositions.Add(newPosition);
            objectsToPlace.Add(spawnedObject);
        }
        
        ChangeState(PlatformState.SelectObject);
    }
    
    public void SelectObject()
    {
        selectedObjectIndex = -1;
        Cursor.visible = true;
    }

    private void SelectRay()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if(hit.collider != null)
        {
            for (int i = 0; i < objectsToPlace.Count(); i++)
            {
                if (hit.collider.GetComponent<BaseObject>() == objectsToPlace[i])
                {
                    Debug.Log($"Object {objectsToPlace[i].gameObject.name} is selected");
                    objectsToPlace[i].isSelectedToPlace = true;
                    selectedObjectIndex = i;
                    ChangeState(PlatformState.PlaceObject);
                    break;
                }
            }
        }
    }
    
    public void PlaceObject()
    {
        if (selectedObjectIndex < 0 || selectedObjectIndex >= objectsToPlace.Count)
        {
            Debug.LogError("selectedObjectIndex is out of range.");
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            var tile = hit.collider.GetComponent<Tile>();
            var obj = objectsToPlace[selectedObjectIndex].GetComponent<BaseObject>();

            if (tile != null && obj != null && tile.Placeable && obj.isSelectedToPlace)
            {
                obj.transform.position = tile.transform.position;
                obj.occupiedTile = tile;
                obj.isPlaced = true;
                obj.isSelectedToPlace = false;
                tile.occupiedObject = obj;
                for (int i = 0; i < objectsToPlace.Count; i++)
                {
                    if (i != selectedObjectIndex)
                    {
                        Destroy(objectsToPlace[i].gameObject);
                    }
                }
                objectsToPlace.Clear();
                selectedObjectIndex = -1;
                ChangeState(PlatformState.End);
            }
        }
        else
        {
            Debug.LogError("No collider hit.");
        }
    }
    
    public void ChangeState(PlatformState newState)
    {
        platformState = newState;
        switch (newState)
        {
            case PlatformState.None: // Not doing anything related to the platforms
                GridManager.instance.HideGrid();
                break;
            case PlatformState.ShowObject: // Show the object to place
                GridManager.instance.ShowGrid();
                SpawnObjects();
                break;
            case PlatformState.SelectObject: // Select the object to place
                SelectObject();
                break;
            case PlatformState.PlaceObject: // Place the object on the grid
                PlaceObject();
                break;
            case PlatformState.End: // End the platform phase
                GameManager.instance.ChangeState(GameState.Countdown);
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
