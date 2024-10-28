using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public static Arrow Instance;

    private Rigidbody2D arrowRb;
    
    public bool directionLeft = true;
    
    public float speed;

    public float timeToDestroy = 5f;
    
    public float currentTime = 0;

    public bool hasHitPlayer = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        arrowRb = GetComponent<Rigidbody2D>();
        MoveArrow();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        CheckSelfDestruct();
    }
    
    private void MoveArrow()
    {
        if (directionLeft)
        {
            arrowRb.velocity = new Vector2(-speed, 0);
        }
        else
        {
            arrowRb.velocity = new Vector2(speed, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasHitPlayer = true;
        }
    }

    private void CheckSelfDestruct()
    {
        if (currentTime >= timeToDestroy)
        {
            Destroy(gameObject);
        }

        if (hasHitPlayer)
        {
            Destroy(gameObject);
        }
    }
}
