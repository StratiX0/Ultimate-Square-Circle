using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance;

    private List<ScriptableItem> _items;
    
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
}
