using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    
    private Dictionary<Vector2, Tile> _tiles;

    private void Awake()
    {
        instance = this;
    }
    
    public void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile ({x}, {y})";
                
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
                
                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
        
        cam.transform.position = new Vector3(width / 2f, height / 2f - 0.5f, -10); 
        
        GameManager.instance.ChangeState(GameState.SpawnPlatform);
    }
    
    public Tile GetPlatformSpawnTile()
    {
        return _tiles.Where(t => t.Key.x < (float)width / 2 && t.Value.Placeable).OrderBy(t => Random.value).First().Value;
    }
    
    public Tile GetTrapSpawnTile()
    {
        return _tiles.Where(t => t.Key.x > (float)width / 2 && t.Value.Placeable).OrderBy(t => Random.value).First().Value;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        return _tiles.GetValueOrDefault(pos);
    }
}
