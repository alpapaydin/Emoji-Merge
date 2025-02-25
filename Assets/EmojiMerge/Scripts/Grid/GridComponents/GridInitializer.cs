using UnityEngine;

public class GridInitializer : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;

    public void InitializeGrid(Grid grid, Vector2Int gridSize)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 cellPosition = grid.GetCellCenterLocal(new Vector3Int(x, y, 0));
                GameObject cellObject = Instantiate(cellPrefab, grid.transform);
                cellObject.transform.localPosition = cellPosition;
                cellObject.name = $"Cell ({x}, {y})";
                Cell cell = cellObject.GetComponent<Cell>();
                cell.Initialize(new Vector2Int(x, y));
                GridManager.Instance.Cells.Add(new Vector2Int(x, y), cell);
            }
        }
    }
}
