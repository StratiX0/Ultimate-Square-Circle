using System;
using System.Collections.Generic;
using UnityEngine;

public class QLearningAgent : MonoBehaviour
{
    public static QLearningAgent instance;
    private Dictionary<(int, int, int, int), float> qTable;
    [SerializeField] private float learningRate = 1f;
    [SerializeField] private float discountFactor = 0.9f;
    [SerializeField] private float explorationRate = 1f;
    [SerializeField] private int simulations;
    private int gridWidth;
    private int gridHeight;
    [SerializeField] private Transform playerSpawnTransform;
    [SerializeField] private Transform finishPointTransform;
    public List<GameObject> traps;
    [SerializeField] private GameObject parentTrap;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gridWidth = GridManager.instance.width;
        gridHeight = GridManager.instance.height;
        qTable = new Dictionary<(int, int, int, int), float>();
        InitializeQTable();
    }

    void InitializeQTable()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int aX = 0; aX < gridWidth; aX++)
                {
                    for (int aY = 0; aY < gridHeight; aY++)
                    {
                        qTable[(x, y, aX, aY)] = 0f;
                    }
                }
            }
        }
    }

    public void PlaceTrap(bool success, float timeTaken)
    {
        EndOfLevel(success, timeTaken);
        
        (int, int, int) state = GetCurrentState();

        (int, int) bestAction = MonteCarloPlacement(state);

        float reward = GetReward(bestAction);
        UpdateQTable(state, bestAction, reward);

        PlaceTrapAt(bestAction);

        GameManager.instance.ChangeState(GameState.Countdown);
    }


    (int, int, int) GetCurrentState()
    {
        Vector2 playerPosition = Player.instance.transform.position;

        int playerGridX = Mathf.Clamp((int)(playerPosition.x - GridManager.instance.transform.position.x), 0, gridWidth - 1);
        int playerGridY = Mathf.Clamp((int)(playerPosition.y - GridManager.instance.transform.position.y), 0, gridHeight - 1);

        int heatValue = HeatmapManager.instance.GetHeatValue(playerGridX, playerGridY);

        return (playerGridX, playerGridY, heatValue);
    }

    (int, int) ChooseAction((int, int, int) state)
    {
        if (UnityEngine.Random.value < explorationRate)
        {
            return (UnityEngine.Random.Range(0, gridWidth), UnityEngine.Random.Range(0, gridHeight));
        }
        else
        {
            return GetBestActionBasedOnHeatmap(state);
        }
    }

    (int, int) GetBestActionBasedOnHeatmap((int, int, int) state)
    {
        float maxQValue = float.MinValue;
        (int, int) bestAction = (0, 0);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int heatValue = HeatmapManager.instance.GetHeatValue(x, y);
                float qValue = qTable[(state.Item1, state.Item2, x, y)] + heatValue * 2f;

                if (qValue > maxQValue && GridManager.instance.GetTileAtPosition(new Vector2(x, y)).Placeable && !GetUnPlaceableTiles().Contains(new Vector2(x, y)))
                {
                    maxQValue = qValue;
                    bestAction = (x, y);
                }
            }
        }
        return bestAction;
    }
    
    List<Vector2> GetUnPlaceableTiles()
    {
        List<Vector2> unPlaceableTiles = new List<Vector2>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Tile tile = GridManager.instance.GetTileAtPosition(new Vector2(x, y));
                if (!tile.Placeable)
                {
                    unPlaceableTiles.Add(new Vector2(x, y));
                }
            }
        }
        
        return unPlaceableTiles;
    }

    float GetReward((int, int) action)
    {
        Tile tile = GridManager.instance.GetTileAtPosition(new Vector2(action.Item1, action.Item2));
        if (tile != null && tile.Placeable)
        {
            int heatValue = HeatmapManager.instance.GetHeatValue(action.Item1, action.Item2);
            float baseReward = 1.0f;
            return baseReward + heatValue * 0.2f;
        }
        return -1.0f;
    }


    public void UpdateQTable((int, int, int) state, (int, int) action, float reward)
    {
        var key = (state.Item1, state.Item2, action.Item1, action.Item2);
    
        if (qTable.ContainsKey(key))
        {
            float oldQValue = qTable[key];

            float bestNextQValue = GetBestQValue(action);

            float newQValue = oldQValue + learningRate * (reward + discountFactor * bestNextQValue - oldQValue);

            qTable[key] = newQValue;
        }
        else
        {
            qTable[key] = reward;
        }
        
        explorationRate = Mathf.Max(0.01f, explorationRate * 0.99f);
    }

    float GetBestQValue((int, int) state)
    {
        float maxQValue = float.MinValue;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (qTable[(state.Item1, state.Item2, x, y)] > maxQValue)
                {
                    maxQValue = qTable[(state.Item1, state.Item2, x, y)];
                }
            }
        }

        return maxQValue;
    }

    void PlaceTrapAt((int, int) position)
    {
        Tile tile = GridManager.instance.GetTileAtPosition(new Vector2(position.Item1, position.Item2));
        if (tile != null && !tile.IsOverlapped())
        {
            var randomItem = PlatformManager.instance.GetRandomTrap<BaseTrap>(Item.Trap);
            if (randomItem != null)
            {
                var trap = Instantiate(randomItem, tile.transform.position, Quaternion.identity, parentTrap.transform);
                trap.occupiedTile = tile;
                tile.occupiedObject = trap;
                tile.objectOnTile = trap.gameObject;
                traps.Add(trap.gameObject);
            }
        }
    }
    
    (int, int) MonteCarloPlacement((int, int, int) state)
    {
        Dictionary<(int, int), float> positionRewards = new Dictionary<(int, int), float>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!GridManager.instance.GetTileAtPosition(new Vector2(x, y)).Placeable) continue;

                float totalReward = 0f;

                for (int i = 0; i < simulations; i++)
                {
                    float reward = SimulateTrapPlacementReward(state, (x, y));
                    totalReward += reward;
                }

                float averageReward = totalReward / simulations;
                positionRewards[(x, y)] = averageReward;
            }
        }

        (int, int) bestPosition = (0, 0);
        float maxAverageReward = float.MinValue;
        foreach (var position in positionRewards)
        {
            if (position.Value > maxAverageReward)
            {
                maxAverageReward = position.Value;
                bestPosition = position.Key;
            }
        }

        return bestPosition;
    }
    
    float SimulateTrapPlacementReward((int, int, int) state, (int, int) action)
    {
        int heatValue = HeatmapManager.instance.GetHeatValue(action.Item1, action.Item2);
        float simulatedReward = GetReward(action);

        simulatedReward += heatValue * 0.2f;

        return simulatedReward;
    }

    void AdjustDifficulty()
    {
        if (PerformanceMetrics.instance.failureCount > PerformanceMetrics.instance.successCount)
        {
            // Ease difficulty
            explorationRate = Mathf.Min(explorationRate + 0.1f, 1.0f);
            discountFactor = Mathf.Max(discountFactor - 0.1f, 0.8f);
            simulations = Mathf.Max(0, simulations - 10);
        }
        else if (PerformanceMetrics.instance.successCount > PerformanceMetrics.instance.failureCount + 2)
        {
            // Increase difficulty
            explorationRate = Mathf.Max(explorationRate - 0.1f, 0.1f);
            discountFactor = Mathf.Min(discountFactor + 0.1f, 0.99f);
            simulations += 10;
        }
    }

    private void EndOfLevel(bool success, float timeTaken)
    {
        PerformanceMetrics.instance.UpdateMetrics(success, timeTaken);
        AdjustDifficulty();
    }
}