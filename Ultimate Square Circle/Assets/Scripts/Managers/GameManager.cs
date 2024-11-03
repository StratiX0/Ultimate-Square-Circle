using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Game State")]
    public GameState gameState;

    [Header("UI Elements and Game Stats")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI deaths;
    [SerializeField] private TextMeshProUGUI wins;
    [SerializeField] private int round;
    [SerializeField] private float time;
    [SerializeField] private float countdownTime;
    [SerializeField] private float countdownDefaultTime;
    [SerializeField] private int deathNumber;
    [SerializeField] private int winNumber;

    private static event Action<GameState> OnGameStateChanged;
    
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
    }

    void Start()
    {
        ChangeState(GameState.Countdown);
        countdownTime = countdownDefaultTime;
    }

    // Update is called once per frame
    private void Update()
    {
        PlayerManager();
    }
    
    private void OnValidate()
    {
        ChangeState(gameState);
    }
    
    // Change the state of the game
    public void ChangeState(GameState newState)
    {
        gameState = newState;
        switch (newState)
        {
            case GameState.None:
                break;
            case GameState.Countdown:
                ResetTime();
                Color originalColor = countdownText.color;
                countdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
                countdownText.gameObject.SetActive(true);
                countdownTime = countdownDefaultTime;
                StartCoroutine(UpdateCountdown());
                break;
            case GameState.Playing:
                _playerScript.ChangeState(PlayerState.Alive);
                StartCoroutine(FadeOutCountdownText());
                break;
            case GameState.ShowObject:
                GridManager.instance.ShowGrid();
                PlatformManager.instance.SpawnObjects();
                _playerScript.ChangeState(PlayerState.Waiting);
                break;
            case GameState.SelectObject:
                PlatformManager.instance.SelectObject();
                break;
            case GameState.PlaceObject:
                PlatformManager.instance.PlaceObject();
                break;
            case GameState.HideGrid:
                GridManager.instance.HideGrid();
                ChangeState(GameState.Countdown);
                _playerScript.ChangeState(PlayerState.Waiting);
                break;
            case GameState.SpawnPlatform:
                // PlatformManager will manage itself
                break;
            case GameState.SpawnTrap:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        
        OnGameStateChanged?.Invoke(newState);
    }
    
    #region Player

    [Header("Player Properties")]
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] GameObject playerPrefab;
    private GameObject _player;
    private Player _playerScript;
    
    // Manage the player's state
    private void PlayerManager()
    {
        switch (_playerScript.playerState)
        {
            case PlayerState.Spawn:
                break;
            case PlayerState.Respawn:
                ChangeState(GameState.ShowObject);
                break;
            case PlayerState.Waiting:
                break;
            case PlayerState.Alive:
                break;
            case PlayerState.Dead:
                
                UpdateDeathCount();
                _playerScript.ChangeState(PlayerState.Respawn);
                break;
            case PlayerState.Finished:
                UpdateWin();
                _playerScript.ChangeState(PlayerState.Respawn);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Create a player gameObject
    private void CreatePlayer()
    {
        _player = Instantiate(playerPrefab, spawnPoint.transform.localPosition, Quaternion.identity);
        _playerScript = _player.GetComponent<Player>();
    }

    #endregion

    #region UI and Game Stats

    // Update the value of the variable _time
    private IEnumerator UpdateTimeCounter()
    {
        while (_playerScript.playerState == PlayerState.Alive)
        {
            time += Time.deltaTime;

            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time % 60);
            int centiseconds = Mathf.FloorToInt((time * 100) % 100);

            var format = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
            timeText.text = format;
            
            yield return null;
        }
    }
    
    private IEnumerator UpdateCountdown()
    {
        while (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            countdownText.text = countdownTime.ToString("F0");
            
            yield return null;
        }
        countdownText.text = "GO!";
        ChangeState(GameState.Playing);
        StartCoroutine(UpdateTimeCounter());
    }
    
    private IEnumerator FadeOutCountdownText()
    {
        float duration = 2.0f;
        float elapsedTime = 0f;
        Color originalColor = countdownText.color;
    
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            countdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, elapsedTime / duration));
            
            yield return null;
        }
        
        countdownText.gameObject.SetActive(false);
        countdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
    }

    // Reset the value of the variable _time
    private void ResetTime()
    {
        time = 0;
    }

    // Update the value of the variable _deathNumber
    private void UpdateDeathCount()
    {
        deathNumber += 1;
        deaths.text = deathNumber.ToString("F0");
    }

    // Update the value of the variable _winNumber
    private void UpdateWin()
    {
        winNumber += 1;
        wins.text = winNumber.ToString("F0");
    }

    #endregion
}

public enum GameState
{
    None,
    Countdown,
    Playing,
    ShowObject,
    SelectObject,
    PlaceObject,
    HideGrid,
    SpawnPlatform,
    SpawnTrap
}
