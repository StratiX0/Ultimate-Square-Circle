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
    private int gridWidth;
    private int gridHeight;

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

    public void PlaceTrap()
    {
        (int, int) state = GetCurrentState();
        (int, int) action = ChooseAction(state);
        float reward = GetReward(action);
        UpdateQTable(state, action, reward);
        PlaceTrapAt(action);
        GameManager.instance.ChangeState(GameState.Countdown);
    }

    (int, int) GetCurrentState()
    {
        Vector2 position = transform.position;
        return ((int)position.x, (int)position.y);
    }

    (int, int) ChooseAction((int, int) state)
    {
        if (UnityEngine.Random.value < explorationRate)
        {
            return (UnityEngine.Random.Range(0, gridWidth), UnityEngine.Random.Range(0, gridHeight));
        }
        else
        {
            return GetBestAction(state);
        }
    }

    (int, int) GetBestAction((int, int) state)
    {
        float maxQValue = float.MinValue;
        (int, int) bestAction = (0, 0);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (qTable[(state.Item1, state.Item2, x, y)] > maxQValue)
                {
                    maxQValue = qTable[(state.Item1, state.Item2, x, y)];
                    bestAction = (x, y);
                }
            }
        }

        return bestAction;
    }

    float GetReward((int, int) action)
    {
        Tile tile = GridManager.instance.GetTileAtPosition(new Vector2(action.Item1, action.Item2));
        if (tile != null && tile.Placeable)
        {
            return 1.0f;
        }
        return -1.0f;
    }

    void UpdateQTable((int, int) state, (int, int) action, float reward)
    {
        float oldQValue = qTable[(state.Item1, state.Item2, action.Item1, action.Item2)];
        float bestNextQValue = GetBestQValue(action);
        float newQValue = oldQValue + learningRate * (reward + discountFactor * bestNextQValue - oldQValue);
        qTable[(state.Item1, state.Item2, action.Item1, action.Item2)] = newQValue;
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
        if (tile != null && tile.Placeable)
        {
            var randomItem = PlatformManager.instance.GetRandomTrap<BaseTrap>(Item.Trap);
            if (randomItem != null)
            {
                var trap = Instantiate(randomItem, tile.transform.position, Quaternion.identity);
                trap.occupiedTile = tile;
                tile.occupiedObject = trap;
            }
        }
    }
}