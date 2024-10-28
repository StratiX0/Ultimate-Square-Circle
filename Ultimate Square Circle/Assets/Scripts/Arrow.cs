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
        
        CheckDestroy();
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
    
    private void CheckDestroy()
    {
        if (currentTime >= timeToDestroy)
        {
            Destroy(gameObject);
        }
    }
}
