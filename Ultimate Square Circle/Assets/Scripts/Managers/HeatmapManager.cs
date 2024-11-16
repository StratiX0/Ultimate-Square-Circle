using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int[,] heatmap;
    public static HeatmapManager instance;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        width = GridManager.instance.width;
        height = GridManager.instance.height;
        heatmap = new int[width, height];
    }

    public void IncrementHeatmap(Vector2 playerPosition)
    {
        int x = Mathf.Clamp((int)((playerPosition.x - transform.position.x) / 1), 0, width - 1);
        int y = Mathf.Clamp((int)((playerPosition.y - transform.position.y) / 1), 0, height - 1);
        heatmap[x, y]++;
    }

    public int GetHeatValue(int x, int y)
    {
        return heatmap[x, y];
    }
    
    public Vector2 GetHotSpot()
    {
        int maxHeat = 0;
        Vector2 hotSpot = Vector2.zero;

        for (int x = 0; x < heatmap.GetLength(0); x++)
        {
            for (int y = 0; y < heatmap.GetLength(1); y++)
            {
                if (heatmap[x, y] > maxHeat)
                {
                    maxHeat = heatmap[x, y];
                    hotSpot = new Vector2(x, y);
                }
            }
        }
        return hotSpot;
    }
    
    void OnDrawGizmos()
    {
        if (heatmap == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int heat = heatmap[x, y];
                if (heat > 0)
                {
                    Color color = Color.Lerp(Color.green, Color.red, (float)heat / 250);
                    Gizmos.color = color;
                    Vector3 cellPosition = new Vector3(x * 1, y * 1) + transform.position;
                    Gizmos.DrawCube(cellPosition, Vector3.one * 1 * 0.9f);
                }
            }
        }
    }

}
