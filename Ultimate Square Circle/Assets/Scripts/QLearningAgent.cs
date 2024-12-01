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

    // Initialize Q-Table with zeros
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

    // Place trap at the best position
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


    // Get current state of the player
    (int, int, int) GetCurrentState()
    {
        Vector2 playerPosition = Player.instance.transform.position;

        int playerGridX = Mathf.Clamp((int)(playerPosition.x - GridManager.instance.transform.position.x), 0, gridWidth - 1);
        int playerGridY = Mathf.Clamp((int)(playerPosition.y - GridManager.instance.transform.position.y), 0, gridHeight - 1);

        int heatValue = HeatmapManager.instance.GetHeatValue(playerGridX, playerGridY);

        return (playerGridX, playerGridY, heatValue);
    }

    // Get reward for the action
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
    
    // Update Q-Table
    public void UpdateQTable((int, int, int) state, (int, int) action, float reward)
    {
        var key = (state.Item1, state.Item2, action.Item1, action.Item2);
    
        if (qTable.ContainsKey(key)) // Check if the state-action pair exists in the Q-Table
        {
            float oldQValue = qTable[key]; // Get old Q-Value

            float bestNextQValue = GetBestQValue(action); // Get best Q-Value for the next state

            float newQValue = oldQValue + learningRate * (reward + discountFactor * bestNextQValue - oldQValue); // Update Q-Value

            qTable[key] = newQValue;
        }
        else
        {
            qTable[key] = reward;
        }
        
        explorationRate = Mathf.Max(0.01f, explorationRate * 0.99f); // Decay exploration rate
    }

    // Get best Q-Value for the given state
    float GetBestQValue((int, int) state)
    {
        float maxQValue = float.MinValue;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (qTable[(state.Item1, state.Item2, x, y)] > maxQValue)
                {
                    maxQValue = qTable[(state.Item1, state.Item2, x, y)]; // Update max Q-Value
                }
            }
        }

        return maxQValue;
    }

    // Place trap at the given position
    void PlaceTrapAt((int, int) position)
    {
        Tile tile = GridManager.instance.GetTileAtPosition(new Vector2(position.Item1, position.Item2));
        if (tile != null && !tile.IsOverlapped()) // Check if the tile is not occupied
        {
            var randomItem = PlatformManager.instance.GetRandomTrap<BaseTrap>(Item.Trap);
            if (randomItem != null) // Check if the random item is not null
            {
                var trap = Instantiate(randomItem, tile.transform.position, Quaternion.identity, parentTrap.transform);
                trap.occupiedTile = tile;
                tile.occupiedObject = trap;
                tile.objectOnTile = trap.gameObject;
                traps.Add(trap.gameObject);
            }
        }
    }
    
    // Monte Carlo Tree Search for trap placement
    (int, int) MonteCarloPlacement((int, int, int) state)
    {
        Dictionary<(int, int), float> positionRewards = new Dictionary<(int, int), float>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!GridManager.instance.GetTileAtPosition(new Vector2(x, y)).Placeable) continue; // Check if the tile is placeable

                float totalReward = 0f;

                for (int i = 0; i < simulations; i++) // Simulate trap placement for the given number of simulations
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
        foreach (var position in positionRewards) // Get the best position with the highest average reward
        {
            if (position.Value > maxAverageReward)
            {
                maxAverageReward = position.Value;
                bestPosition = position.Key;
            }
        }

        return bestPosition; // Return the best position
    }
    
    // Simulate trap placement reward
    float SimulateTrapPlacementReward((int, int, int) state, (int, int) action)
    {
        int heatValue = HeatmapManager.instance.GetHeatValue(action.Item1, action.Item2);
        float simulatedReward = GetReward(action);

        simulatedReward += heatValue * 0.2f;

        return simulatedReward;
    }
    
    // Adjust difficulty based on performance
    void AdjustDifficulty()
    {
        if (PerformanceMetrics.instance.failureCount > PerformanceMetrics.instance.successCount)
        {
            // Ease difficulty
            explorationRate = Mathf.Min(explorationRate + 0.3f, 1.0f);
            discountFactor = Mathf.Max(discountFactor - 0.3f, 0.8f);
            simulations = Mathf.Max(0, simulations - 10);
        }
        else if (PerformanceMetrics.instance.successCount > PerformanceMetrics.instance.failureCount + 2)
        {
            // Increase difficulty
            explorationRate = Mathf.Max(explorationRate - 0.3f, 0.1f);
            discountFactor = Mathf.Min(discountFactor + 0.3f, 0.99f);
            simulations += 10;
        }
    }

    private void EndOfLevel(bool success, float timeTaken)
    {
        PerformanceMetrics.instance.UpdateMetrics(success, timeTaken);
        AdjustDifficulty();
    }
}