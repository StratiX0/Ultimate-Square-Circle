using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player _instance;
    private Rigidbody2D _rb;
    
    [Header("Ground Properties")]
    public Transform groundCheck;
    public LayerMask groundMask;

    [Header("Movement Properties")]
    public float speed;
    public float jumpForce;
    
    private float _horInput;
    private bool _vertInputDown;
    private bool _vertInputUp;
    
    [Header("Status Properties")]
    public bool hasFinished;
    public bool isDead;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        hasFinished = false;
    }

    // Update is called once per frame
    void Update()
    {
        InputManager();
        MovementManager();
    }

    // Manage the input of the player to move
    void InputManager()
    {
        _horInput = Input.GetAxis("Horizontal");
        _vertInputDown = Input.GetButtonDown("Jump");
        _vertInputUp = Input.GetButtonUp("Jump");
    }

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
    
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundMask);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Finish")) // The player have finished if he enters a trigger with a tag "Finish"
        {
            hasFinished = true;
        }
        if (collision.gameObject.CompareTag("KillPlayer")) // The player is considered dead if he enters a trigger with a tag "KillPlayer"
        {
            isDead = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("KillPlayer")) // The player is considered dead if he is in collision with an object of tag "KillPlayer"
        {
            isDead = true;
        }
    }
}
