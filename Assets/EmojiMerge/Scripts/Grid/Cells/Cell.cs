using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cell : MonoBehaviour
{
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;
    private GridItem currentItem;

    public Vector2Int GridPosition => gridPosition;
    public GridItem CurrentItem => currentItem;
    public bool IsOccupied => currentItem != null;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int position)
    {
        gridPosition = position;
        ApplyStyling();
    }

    private void ApplyStyling()
    {
        if (GridManager.Instance != null && GridManager.Instance.Styling != null)
        {
            spriteRenderer.color = GridManager.Instance.Styling.GetCellColor(gridPosition);
        }
    }

    public void SetItem(GridItem item)
    {
        if (currentItem == item)
            return;
            
        if (currentItem != null)
        {
            currentItem = null;
        }
        
        currentItem = item;
    }

    public void ClearItem()
    {
        if (currentItem != null)
        {
            if (currentItem.OccupiedCell == this)
            {
                currentItem = null;
            }
        }
    }
}
