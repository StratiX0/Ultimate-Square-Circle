using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    private Rigidbody2D _rb;
    
    [Header("Ground Properties")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask killPlayerMask;

    [Header("Movement Properties")]
    public float speed;
    public float jumpForce;
    
    private float _horInput;
    private bool _vertInputDown;
    private bool _vertInputUp;

    [Header("Status Properties")] 
    public bool canMove;
    private HeatmapManager heatmapManager;

    
    [SerializeField] private Transform respawnPoint;
    
    public PlayerState playerState;
    public static event Action<PlayerState> OnPlayerStateChanged;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        respawnPoint = GameObject.Find("Spawn Point").transform;
        OnPlayerStateChanged += state => Debug.Log($"Player state changed to {state}");
        ChangeState(PlayerState.Spawn);
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        heatmapManager = HeatmapManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            InputManager();
            MovementManager();
            
            heatmapManager.IncrementHeatmap(transform.position);
        }
    }

    public void MoveToSpawn()
    {
        if (playerState == PlayerState.Spawn || playerState == PlayerState.Respawn)
        {
            transform.position = respawnPoint.position;
        }
    }

    // Manage the input of the player to move
    void InputManager()
    {
        _horInput = Input.GetAxis("Horizontal");
        _vertInputDown = Input.GetButtonDown("Jump");
        _vertInputUp = Input.GetButtonUp("Jump");
    }

    // Manage the movement of the player
    void MovementManager()
    {
        _rb.velocity = new Vector2(_horInput * speed, _rb.velocity.y);

        if (_vertInputDown && IsGrounded())
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        }
        
        if (_vertInputUp && _rb.velocity.y > 0f)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * 0.5f);
        }
    }
    
    // Check if the player is grounded
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundMask);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Finish")) // The player have finished if he enters a trigger with a tag "Finish"
        {
            instance.ChangeState(PlayerState.Finished);
        }
        if (killPlayerMask == (killPlayerMask | (1 << collision.gameObject.layer)))
        {
            instance.ChangeState(PlayerState.Dead);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (killPlayerMask == (killPlayerMask | (1 << collision.gameObject.layer)))
        {
            instance.ChangeState(PlayerState.Dead);
        }
    }
    
    // Change the state of the player
    public void ChangeState(PlayerState newState)
    {
        playerState = newState;
        switch (newState)
        {
            case PlayerState.Spawn:
                MoveToSpawn();
                break;
            case PlayerState.Respawn:
                MoveToSpawn();
                break;
            case PlayerState.Waiting:
                break;
            case PlayerState.Alive:
                canMove = true;
                break;
            case PlayerState.Dead:
                canMove = false;
                _rb.velocity = Vector2.zero;
                break;
            case PlayerState.Finished:
                canMove = false;
                _rb.velocity = Vector2.zero;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        
        OnPlayerStateChanged?.Invoke(newState);
    }
}

// Enum to manage the state of the player
public enum PlayerState
{
    Spawn,
    Respawn,
    Waiting,
    Alive,
    Dead,
    Finished
}
