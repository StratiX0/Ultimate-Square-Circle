using UnityEngine;

public class Arrow : MonoBehaviour
{
    private static Arrow _instance;
    private Rigidbody2D _rb;
    
    [Header("Movement Properties")]
    public bool directionLeft = true;
    public float speed;

    [Header("Time Properties")]
    public float currentTime;
    public float timeToDestroy = 5f;

    [Header("Status Properties")]
    public bool hasHitPlayer;
    public bool shouldDestroy;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        MoveArrow();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        CheckSelfDestruct();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Check if the Player has been hit
        {
            hasHitPlayer = true;
        }
        if (!collision.gameObject.CompareTag("Trap")) // Check if the Arrow has hit something
        {
            shouldDestroy = true;
        }
    }

    // Make the arrow move in a specific direction
    private void MoveArrow()
    {
        _rb.velocity = directionLeft ? new Vector2(-speed, 0) : new Vector2(speed, 0);
    }

    // Destroy the arrow if the conditions are met
    private void CheckSelfDestruct()
    {
        if (currentTime >= timeToDestroy || hasHitPlayer || shouldDestroy || Player.instance.playerState == PlayerState.Waiting) // Destroy itself if the Arrow has hit a Player or if the arrow should be destroyed since its launched
        {
            Destroy(gameObject);
        }
    }
}
