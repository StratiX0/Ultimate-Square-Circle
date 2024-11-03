using UnityEngine;

public class Bow : MonoBehaviour
{
    private static Bow _instance;

    [Header("Arrow Properties")]
    public GameObject arrowPrefab;

    [Header("Time Properties")]
    public float shootInterval = 1f;
    public float timeToShoot;
    public Vector2 randomStartShooting;

    [Header("Direction Properties")]
    public bool directionLeft = true;

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
        timeToShoot = Random.Range(randomStartShooting.x, randomStartShooting.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.gameState == GameState.Playing) // Shoot only when the game is playing
        {
            timeToShoot += Time.deltaTime;

            Shoot();
        }
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