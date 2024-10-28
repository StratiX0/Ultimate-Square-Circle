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

    public Vector2 randomStartShooting;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        timeToShoot = Random.Range(randomStartShooting.x, randomStartShooting.y);
    }

    // Update is called once per frame
    void Update()
    {
        timeToShoot += Time.deltaTime;

        Shoot();
    }

    // Make the Bow Shoot an arrow from the bow position with the current direction
    private void Shoot()
    {
        if (timeToShoot >= shootInterval)
        {
            GameObject arrowObject = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            Arrow arrowComp = arrowObject.GetComponent<Arrow>();
            arrowComp.directionLeft = directionLeft;
            Transform arrowTransform = arrowObject.GetComponent<Transform>();
            if (!directionLeft) arrowTransform.localScale = new Vector3(-arrowTransform.localScale.x, arrowTransform.localScale.y, arrowTransform.localScale.z);

            timeToShoot = 0;
        }
    }
}