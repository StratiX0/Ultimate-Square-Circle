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
    [SerializeField] private bool lastRoundWin;

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
        OnGameStateChanged += state => Debug.Log($"Game state changed to {state}");
        CreatePlayer();
    }

    void Start()
    {
        ChangeState(GameState.Countdown);
        countdownTime = countdownDefaultTime;
        Cursor.visible = false;
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
            case GameState.Countdown: // Countdown before the game starts
                ResetCountdown();
                StartCoroutine(UpdateCountdown());
                break;
            case GameState.Playing: // The game is running
                _playerScript.ChangeState(PlayerState.Alive);
                StartCoroutine(FadeOutCountdownText());
                break;
            case GameState.ObjectSystem: // Activate the platform system
                _playerScript.ChangeState(PlayerState.Waiting);
                PlatformManager.instance.ChangeState(PlatformState.ShowObject);
                break;
            case GameState.AiObjectSystem: // Activate the platform system for the AI
                QLearningAgent.instance.PlaceTrap(lastRoundWin, time);
                ResetTime();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        
        OnGameStateChanged?.Invoke(newState);
    }
    
    #region Player Management

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
                ChangeState(GameState.ObjectSystem);
                break;
            case PlayerState.Waiting:
                break;
            case PlayerState.Alive:
                break;
            case PlayerState.Dead:
                UpdateDeathCount();
                lastRoundWin = false;
                _playerScript.ChangeState(PlayerState.Respawn);
                break;
            case PlayerState.Finished:
                UpdateWin();
                lastRoundWin = true;
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
    
    // Update the value of the variable _countdownTime
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
    
    // Fade out the countdown text
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

    // Reset the value of the variable time
    private void ResetTime()
    {
        time = 0;
        timeText.text = "00:00.00";
    }

    // Update the value of the variable deathNumber
    private void UpdateDeathCount()
    {
        deathNumber += 1;
        deaths.text = deathNumber.ToString("F0");
    }

    // Update the value of the variable winNumber
    private void UpdateWin()
    {
        winNumber += 1;
        wins.text = winNumber.ToString("F0");
    }

    // Reset the value of the variable countdownTime
    private void ResetCountdown()
    {
        Color originalColor = countdownText.color;
        countdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        countdownText.gameObject.SetActive(true);
        countdownTime = countdownDefaultTime;
    }

    #endregion
}

public enum GameState
{
    None,
    Countdown,
    Playing,
    ObjectSystem,
    AiObjectSystem
}
