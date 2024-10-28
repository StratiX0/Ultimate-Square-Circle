using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player Instance;
    private Rigidbody2D rb;
    public Transform playerTransform;

    public float speed;

    public float jumpForce;

    public float horInput;

    public bool vertInput;

    public bool isGrounded;

    public bool isJumping;

    public bool inFinish;

    public bool isDead;

    public bool inDeath;
    
    private GameObject playerCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GetComponent<Transform>();
        inFinish = false;
    }

    // Update is called once per frame
    void Update()
    {
        InputManager();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horInput * speed, rb.velocity.y);

        if (vertInput && !isJumping && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
    }

    // Manage the input of the player to move
    void InputManager()
    {
        horInput = Input.GetAxis("Horizontal");
        vertInput = Input.GetAxis("Vertical") > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Finish")) // The player have finished if he enters a trigger with a tag "Finish"
        {
            inFinish = true;
        }
        if (other.gameObject.CompareTag("KillPlayer")) // The player is considered dead if he enters a trigger with a tag "DeathTrigger"
        {
            isDead = true;
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground")) // The player can jump if he is in collision with an object of tag "Ground"
        {
            isJumping = false;
            isGrounded = true;
        }
        if (other.gameObject.CompareTag("KillPlayer")) // The player is considered dead if he is in collision with an object of tag "DeathTrigger"
        {
            isDead = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
