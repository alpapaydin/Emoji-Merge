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
    private SpriteRenderer backgroundRenderer;
    private GridManager gridManager;
    private GameObject backgroundObject;

    private void OnDestroy()
    {
        if (gridManager != null)
            gridManager.OnGridResized -= UpdateGridBackground;
        
        DestroyBackgroundRenderer();
    }

    private void UpdateGridBackground()
    {
        CreateBackgroundRenderer();
        
        backgroundRenderer.sprite = gridBackground;
        backgroundRenderer.transform.localScale = new Vector2(gridManager.GridSize.x, gridManager.GridSize.y) * backgroundScaleMultiplier;
        backgroundRenderer.transform.position = gridManager.transform.position +
            new Vector3(gridManager.GridSize.x * gridManager.GridScaleMultiplier.x / 2f, gridManager.GridSize.y * gridManager.GridScaleMultiplier.y / 2f, 0f);
    }

    private void DestroyBackgroundRenderer()
    {
        if (backgroundObject != null)
        {
            Destroy(backgroundObject);
            backgroundObject = null;
            backgroundRenderer = null;
        }
    }

    private void CreateBackgroundRenderer()
    {
        DestroyBackgroundRenderer();
        
        backgroundObject = new GameObject("GridBackground");
        backgroundObject.transform.SetParent(transform);
        backgroundObject.transform.localPosition = Vector3.zero;
        
        backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sortingOrder = -1;
    }

    public void Initialize(GridManager manager)
    {
        gridManager = manager;
        gridManager.OnGridResized += UpdateGridBackground;
        
        UpdateGridBackground();
    }

    public Color GetCellColor(Vector2Int position)
    {
        return (position.x + position.y) % 2 == 0 ? cellColor1 : cellColor2;
    }
}
