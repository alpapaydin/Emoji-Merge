using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStyling : MonoBehaviour
{
    [Header("Cell Colors")]
    [SerializeField] private Color cellColor1 = Color.white;
    [SerializeField] private Color cellColor2 = new Color(0.9f, 0.9f, 0.9f);

    private SpriteRenderer backgroundRenderer;
    private GridManager gridManager;

    public void Initialize(GridManager manager)
    {
        gridManager = manager;
    }

    public Color GetCellColor(Vector2Int position)
    {
        return (position.x + position.y) % 2 == 0 ? cellColor1 : cellColor2;
    }
}
