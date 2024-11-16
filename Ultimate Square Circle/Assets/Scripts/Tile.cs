using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetColor;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject highlight;
    [SerializeField] private bool isPlaceable;

    public BaseObject occupiedObject;
    public bool Placeable
    {
        get => isPlaceable && occupiedObject == null && !IsNearStartOrFinish() && !IsOverlapped();
        set => isPlaceable = value;
    }

    private void Start()
    {
        IsOverlapped();
    }

    public void Init(bool isOffset)
    {
        spriteRenderer.color = isOffset ? offsetColor : baseColor;
    }

    private void OnMouseEnter()
    {
        highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }

    private bool IsNearStartOrFinish()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3.0f);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Respawn") || collider.gameObject.CompareTag("Finish"))
            {
                return true;
            }
        }
        return false;
    }
    
    public bool IsOverlapped()
    {
        Collider2D[] colliders = new Collider2D[1];
        Collider2D collider = GetComponent<Collider2D>();
        int colliderCount = Physics2D.OverlapBox(collider.bounds.center, collider.bounds.size - new Vector3(0.1f, 0.1f, 0), 0f, new ContactFilter2D(), colliders);
        for (int i = 0; i < colliderCount; i++)
        {
            if (colliders[i] != null && (colliders[i].gameObject.CompareTag("Trap") || colliders[i].gameObject.CompareTag("Platform") || colliders[i].gameObject.CompareTag("Ground")))
            {
                Placeable = false;
                return true;
            }
        }
        Placeable = true;
        return false;
    }
}