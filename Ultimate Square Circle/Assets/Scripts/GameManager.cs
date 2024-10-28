using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float time = 0;
    public TextMeshProUGUI timeText;
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
            playerScript.playerTransform.DetachChildren();
            Destroy(_player);
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
