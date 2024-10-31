using TMPro;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    private float _time;
    private int _deathNumber, _winNumber;
    public TextMeshProUGUI timeText, deaths, wins;
    [SerializeField] GameObject death_image;
    [SerializeField] GameObject win_image;
    [SerializeField] GameObject restart_text;
    [SerializeField] GameObject playerPrefab;
    private GameObject _player;
    private Player _playerScript;
    private IA_script _ia;
    public GameObject spawnPoint;
    public int round;

    private static GameManager _instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
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

    // Update is called once per frame
    private void Update()
    {
        // Update _time count while the player is alive and have not reached the finish point 
        if (!CheckDeath() && !CheckFinish())
        {
            UpdateTime();
        }
        
        if (_time >= 20.0)
        {
            restart_text.SetActive(true);
        }

        if (CheckPlayerExists() && CheckDeath() || CheckFinish() || Input.GetKeyDown(KeyCode.R))
        {
            Destroy(_player);

            if (CheckDeath() || Input.GetKeyDown(KeyCode.R))
            {
                _ia.UpdateQTable(Vector2 state, int action, float reward, Vector2 nextState);
                StartCoroutine(ShowDead());
                UpdateDn();
            }
            else if (CheckFinish())
            {
                StartCoroutine(ShowWin());
                UpdateWin();
            }
        }

        if (!CheckPlayerExists())
        {
            CreatePlayer();
            ResetTime();
            restart_text.SetActive(false);
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

    IEnumerator ShowDead()
    {
        death_image.SetActive(true); // Affiche l'image de "Game Over"
        Time.timeScale = 0; // Met le jeu en pause
        yield return new WaitForSecondsRealtime(1); // Attendre 1 secondes en temps réel
        Time.timeScale = 1; // Reprendre le jeu
        death_image.SetActive(false); // Cache l'image de "Game Over"
    }

    IEnumerator ShowWin()
    {
        win_image.SetActive(true);
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        win_image.SetActive(false);
    }
}
