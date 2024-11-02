using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float _time;
    private int _deathNumber, _winNumber;
    public TextMeshProUGUI timeText, deaths, wins;
    [SerializeField] GameObject playerPrefab;
    private GameObject _player;
    private Player _playerScript;
    public GameObject spawnPoint;
    public int round;

    public static GameManager instance;
    public GameState gameState;
    public static event Action<GameState> OnGameStateChanged;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        CreatePlayer();
        timeText = GameObject.Find("Time Value").GetComponent<TextMeshProUGUI>();
        deaths = GameObject.Find("Deaths number").GetComponent<TextMeshProUGUI>();
        wins = GameObject.Find("Wins number").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        OnGameStateChanged += state => Debug.Log($"Game state changed to {state}");
        ChangeState(GameState.CreateGrid);
    }

    // Update is called once per frame
    private void Update()
    {
        // Update _time count while the player is alive and have not reached the finish point 
        if (!CheckDeath() && !CheckFinish())
        {
            UpdateTime();
        }
        
        if (CheckPlayerExists() && CheckDeath())
        {
            Destroy(_player);
        }

        if (!CheckPlayerExists() && CheckDeath())
        {
            CreatePlayer();
            ResetTime();
            UpdateDn();
        }

        if (CheckPlayerExists() && CheckFinish())
        {
            Destroy(_player);
        }

        if (!CheckPlayerExists() && CheckFinish())
        {
            CreatePlayer();
            ResetTime();
            UpdateWin();
        }
    }

    // Create a player gameObject
    private void CreatePlayer()
    {
        _player = Instantiate(playerPrefab, spawnPoint.transform.localPosition, Quaternion.identity);
        _playerScript = _player.GetComponent<Player>();
    }

    private bool CheckPlayerExists()
    {
        return _player != null;
    }

    // Update the value of the variable _time
    private void UpdateTime()
    {
        _time += Time.deltaTime;

        int minutes = Mathf.FloorToInt(_time / 60F);
        int seconds = Mathf.FloorToInt(_time % 60);
        int centiseconds = Mathf.FloorToInt((_time * 100) % 100);

        var format = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
        timeText.text = format;
    }

    // Reset the value of the variable _time
    private void ResetTime()
    {
        _time = 0;
    }

    // Update the value of the variable _deathNumber
    private void UpdateDn()
    {
        _deathNumber += 1;
        deaths.text = _deathNumber.ToString("F0");
    }

    // Update the value of the variable _winNumber
    private void UpdateWin()
    {
        _winNumber += 1;
        wins.text = _winNumber.ToString("F0");
    }

    // Check if the player has reached the finish of the map
    private bool CheckFinish()
    {
        return (_playerScript.hasFinished);
    }

    // Check if the player is dead
    private bool CheckDeath()
    {
        return (_playerScript.isDead);
    }
    
    public void ChangeState(GameState newState)
    {
        gameState = newState;
        switch (newState)
        {
            case GameState.Playing:
                break;
            case GameState.CreateGrid:
                GridManager.instance.GenerateGrid();
                break;
            case GameState.ShowObject:
                PlatformManager.instance.SpawnObjects();
                break;
            case GameState.SelectObject:
                PlatformManager.instance.SelectObject();
                break;
            case GameState.PlaceObject:
                PlatformManager.instance.PlaceObject();
                break;
            case GameState.SpawnPlatform:
                PlatformManager.instance.SpawnPlatforms();
                break;
            case GameState.SpawnTrap:
                PlatformManager.instance.SpawnTraps();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        
        OnGameStateChanged?.Invoke(newState);
    }
    
    private void OnValidate()
    {
        ChangeState(gameState);
    }
}

public enum GameState
{
    Playing,
    CreateGrid,
    ShowObject,
    SelectObject,
    PlaceObject,
    SpawnPlatform,
    SpawnTrap
}
