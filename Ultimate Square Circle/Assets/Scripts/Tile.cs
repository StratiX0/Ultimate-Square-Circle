using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetColor;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject highlight;
    [SerializeField] private bool isPlaceable;
    
    public BaseObject occupiedObject;
    public bool Placeable => isPlaceable && occupiedObject == null;
    
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

    public void SetObject(BaseObject item)
    {
        if (item.occupiedTile != null) item.occupiedTile.occupiedObject = null;
        item.transform.position = transform.position;
        occupiedObject = item;
        item.occupiedTile = this;
    }
}
