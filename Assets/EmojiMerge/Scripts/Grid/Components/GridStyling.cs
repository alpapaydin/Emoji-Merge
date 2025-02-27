using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStyling : MonoBehaviour
{
    [Header("Cell Colors")]
    [SerializeField] private Color cellColor1 = Color.white;
    [SerializeField] private Color cellColor2 = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Sprite gridBackground;
    [SerializeField] private float backgroundScaleMultiplier = 0.5f;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    private GridManager gridManager;

    private void OnDestroy()
    {
        gridManager.OnGridResized -= UpdateGridBackground;
    }

    private void UpdateGridBackground()
    {
        backgroundRenderer.sprite = gridBackground;
        backgroundRenderer.transform.localScale = new Vector2(gridManager.GridSize.x, gridManager.GridSize.y) * gridManager.GridScaleMultiplier * backgroundScaleMultiplier;
    }

    public void Initialize(GridManager manager)
    {
        gridManager = manager;
        gridManager.OnGridResized += UpdateGridBackground;
    }

    public Color GetCellColor(Vector2Int position)
    {
        return (position.x + position.y) % 2 == 0 ? cellColor1 : cellColor2;
    }
}
