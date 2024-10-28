using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public static Bow Instance;

    public GameObject arrowPrefab;

    public float shootInterval = 1f;

    public bool directionLeft = true;

    public float timeToShoot = 0;

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
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeToShoot += Time.deltaTime;

        Shoot();
    }

    private void Shoot()
    {
        if (timeToShoot >= shootInterval)
        {
            Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            timeToShoot = 0;
        }
    }
}