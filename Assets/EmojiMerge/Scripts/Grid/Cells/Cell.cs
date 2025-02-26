using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cell : MonoBehaviour
{
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;

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
}
