using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float time = 0;
    private int death_number, win_number;
    public TextMeshProUGUI timeText, Deaths, Wins;
    [SerializeField] GameObject playerPrefab;
    private GameObject _player;
    public Player playerScript;
    public GameObject spawnPoint;
    public int round = 0;

    public static GameManager Instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        CreatePlayer();
        timeText = GameObject.Find("Time Value").GetComponent<TextMeshProUGUI>();
        Deaths = GameObject.Find("Death number").GetComponent<TextMeshProUGUI>();
        Wins = GameObject.Find("Wins number").GetComponent<TextMeshProUGUI>();
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        // Update time count while the player is alive and have not reached the finish point 
        if (!CheckDeath() && !CheckFinish())
        {
            UpdateTime();
        }
        
        if (_player != null && CheckDeath())
        {
            Destroy(_player);
        }

        if (_player == null && CheckDeath())
        {
            CreatePlayer();
            ResetTime();
            UpdateDN();
        }

        if (_player != null && CheckFinish())
        {
            Destroy(_player);
        }

        if (_player == null && CheckFinish())
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
        playerScript = _player.GetComponent<Player>();
    }

    // Update the value of the variable time
    public void UpdateTime()
    {
        time += Time.deltaTime;
        timeText.text = time.ToString("F2");
    }

    // Reset the value of the variable time
    public void ResetTime()
    {
        time = 0;
    }

    // Update the value of the variable death_number
    public void UpdateDN()
    {
        death_number += 1;
        Deaths.text = death_number.ToString("F0");
    }

    // Update the value of the variable win_number
    public void UpdateWin()
    {
        win_number += 1;
        Wins.text = death_number.ToString("F0");
    }

    // Check if the player has reached the finish of the map
    public bool CheckFinish()
    {
        if (playerScript.inFinish)
        {
            return true;
        }
        return false;
    }

    // Check if the player is dead
    public bool CheckDeath()
    {
        if (playerScript.isDead)
        {
            return true;
        }
        return false;
    }
}
