using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance;

    private List<ScriptableItem> _items;
    
    [SerializeField] private Transform cam;

    public int itemsNbrToShow;
    
    private void Awake()
    {
        instance = this;
        
        _items = Resources.LoadAll<ScriptableItem>("Objects").ToList();
    }

    public void SpawnPlatforms()
    {
        var platformCount = 1;
        
        for (int i = 0; i < platformCount; i++)
        {
            var randomItem = GetRandomItem<BasePlatform>(Item.Platform);
            var spawnedObject = Instantiate(randomItem);
            var randomSpawnTile = GridManager.instance.GetPlatformSpawnTile();
            
            randomSpawnTile.SetObject(spawnedObject);
        }
        
        GameManager.instance.ChangeState(GameState.SpawnTrap);
    }
    
    public void SpawnTraps()
    {
        var trapCount = 1;
        
        for (int i = 0; i < trapCount; i++)
        {
            var randomItem = GetRandomItem<BaseTrap>(Item.Trap);
            var spawnedTrap = Instantiate(randomItem);
            var randomSpawnTile = GridManager.instance.GetTrapSpawnTile();
            
            randomSpawnTile.SetObject(spawnedTrap);
        }
        
        GameManager.instance.ChangeState(GameState.Playing);
    }
    
    private T GetRandomItem<T>(Item item) where T : BaseObject
    {
        return (T)_items.Where(i => i.item == item).OrderBy(o => Random.value).First().objectPrefab;
    }
    
    private T GetRandomItem2<T>(params Item[] items) where T : BaseObject
    {
        return (T)_items.Where(i => items.Contains(i.item)).OrderBy(o => Random.value).First().objectPrefab;
    }
    
    public void ShowObjects()
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
                newPosition = new Vector3(Random.Range(pos.x - size / 2, pos.x + size / 2), Random.Range(pos.y - size / 2, pos.y + size / 2), 0);
            } while (usedPositions.Any(p => Vector3.Distance(p, newPosition) < 2.0f));

            spawnedObject.transform.position = newPosition;
            usedPositions.Add(newPosition);
        }

        GameManager.instance.ChangeState(GameState.Playing);
    }
}
