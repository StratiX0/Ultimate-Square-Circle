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
    
    private void Awake()
    {
        instance = this;
        
        _items = Resources.LoadAll<ScriptableItem>("Objects").ToList();
    }

    private void Start()
    {
        objectsToPlace = new List<BaseObject>();
    }

    private void Update()
    {
        if (GameManager.instance.gameState == GameState.SelectObject && inObjectSelection && Input.GetMouseButtonDown(0))
        {
            SelectRay();
        }
        
        if (GameManager.instance.gameState == GameState.PlaceObject && Input.GetMouseButtonDown(0))
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
        
        GameManager.instance.ChangeState(GameState.SelectObject);
    }
    
    public void SelectObject()
    {
        selectedObjectIndex = -1;
        Cursor.visible = true;
        inObjectSelection = true;
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
                    GameManager.instance.ChangeState(GameState.PlaceObject);
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
                GameManager.instance.ChangeState(GameState.HideGrid);
            }
        }
        else
        {
            Debug.LogError("No collider hit.");
        }
    }
}
