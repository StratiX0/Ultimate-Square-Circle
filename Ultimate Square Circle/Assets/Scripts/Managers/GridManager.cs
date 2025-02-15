using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    
    public int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject gridObject;
    
    private Dictionary<Vector2, Tile> _tiles;

    private void Awake()
    {
        instance = this;
        GenerateGrid();
    }
    
    public void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = gridObject.transform.position;
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y) + pos, Quaternion.identity, gridObject.transform);
                spawnedTile.name = $"Tile ({x}, {y})";
                
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
                
                spawnedTile.gameObject.SetActive(false);
                
                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
    }
    
    public void ShowGrid()
    {
        if (_tiles != null)
        {
            foreach (var tile in _tiles)
            {
                tile.Value.gameObject.SetActive(true);
            }
        }
    }
    
    public void HideGrid()
    {
        if (_tiles != null)
        {
            foreach (var tile in _tiles)
            {
                tile.Value.gameObject.SetActive(false);
            }
        }
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
