using UnityEngine;

public class BouncyObject : MonoBehaviour
{
    public float bounceHeight = 5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = new Vector2(0, bounceHeight);
            }
        }
    }
}