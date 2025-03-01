using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GameObject cellPrefab;
    [Header("Grid Settings")]
    [SerializeField] private Grid grid;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(9, 9);
    [SerializeField] private GridStyling gridStyling;
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private Vector4 gridMargins = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

    
    private InputManager inputManager;
    private RandomLevelData levelData;
    
    public Vector2 GridScaleMultiplier {get; private set;} = new Vector2(1f, 1f);
    public Dictionary<Vector2Int, Cell> Cells = new Dictionary<Vector2Int, Cell>();
    public InputManager Inputs => inputManager;
    public Grid Grid => grid;
    public GridStyling Styling => gridStyling;
    public Vector2Int GridSize => gridSize;
    public event Action OnGridInitialized;
    public event Action OnGridResized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        if (inputManager != null)
        {
            inputManager.OnTouchEnd -= CheckCellTapped;
            inputManager.OnTouchStart -= CheckCellTouchStart;
            inputManager.OnDragStarted -= CheckCellDragStart;
        }
    }

    public void Initialize(RandomLevelData newLevelData)
    {
        levelData = newLevelData;
        SetupInputManager();
        SetupMergeManager();
        InitializeGrid();
        SetupGridStyling();
        SetupGridLayout();
        SpawnGridItems();
        OnGridInitialized?.Invoke();
    }

    private void SetupInputManager()
    {
        GameObject inputObj = new GameObject("Grid Input Manager");
        inputManager = inputObj.AddComponent<InputManager>();
        inputManager.Initialize(grid);
        inputManager.OnTouchEnd += CheckCellTapped;
        inputManager.OnTouchStart += CheckCellTouchStart;
        inputManager.OnDragStarted += CheckCellDragStart;
    }

    private void SetupMergeManager()
    {
        GameObject mergeObj = new GameObject("Grid Merge Manager");
        mergeObj.AddComponent<MergeManager>();
    }

    private void InitializeGrid()
    {
        gridSize = LevelGenerator.GetRandomGridSize(levelData);
        CreateCells();
    }

    private void SetupGridStyling()
    {
        if (gridStyling != null)
            gridStyling.Initialize(this);
    }

    private void SetupGridLayout()
    {
        GridLayoutManager.PositionAndScaleGridWithPercentageMargins(grid, gridSize, gridMargins.x, gridMargins.y, gridMargins.z, gridMargins.w);
        GridScaleMultiplier = grid.transform.localScale;
        OnGridResized?.Invoke();
    }

    private void SpawnGridItems()
    {
        LevelGenerator.GenerateRandomLevel(levelData, gridSize, itemManager);
    }


    private bool IsValidItemPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize.x && 
                position.y >= 0 && position.y < gridSize.y && 
                IsCellOccupied(position);
    }

    private void CheckCellTouchStart(Vector2Int position)
    {
        if (IsValidItemPosition(position))
        {
            var item = GetItemAtCell(position);
            item?.OnTouchStart();
        } else
        {
            UIManager.Instance.CloseItemDetailsPane();
        }
    }

    private void CheckCellTapped(Vector2Int position)
    {
        if (!CanInteractWithCell(position)) return;

        var item = GetItemAtCell(position);
        item?.OnTapped();
    }

    private void CheckCellDragStart(Vector2Int position)
    {
        if (!CanInteractWithCell(position)) return;
        
        var item = GetItemAtCell(position);
        item?.OnDragStart();
    }

    private bool CanInteractWithCell(Vector2Int position)
    {
        return !IsCellBlocked(position) && IsValidItemPosition(position) && !IsItemPendingDestruction(position);
    }


    public bool IsItemPendingDestruction(Vector2Int position)
    {
        return IsValidItemPosition(position) && GetItemAtCell(position).IsPendingDestruction;
    }

    public bool IsCellOccupied(Vector2Int position)
    {
        return Cells.TryGetValue(position, out var cell) && cell.IsOccupied;
    }

    public bool IsCellBlocked(Vector2Int position)
    {
        return Cells.TryGetValue(position, out var cell) && cell.IsCellBlocked;
    }

    public Cell GetCell(Vector2Int position)
    {
        return Cells.TryGetValue(position, out var cell) ? cell : null;
    }

    public Cell[] GetAdjacentCells(Vector2Int position)
    {
        List<Cell> adjacentCells = new List<Cell>();
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var direction in directions)
        {
            Vector2Int checkPos = position + direction;
            if (Cells.TryGetValue(checkPos, out var cell))
            {
                adjacentCells.Add(cell);
            }
        }

        return adjacentCells.ToArray();
    }

    public GridItem GetItemAtCell(Vector2Int position)
    {
        return Cells.TryGetValue(position, out var cell) ? cell.CurrentItem : null;
    }

    public bool TryPlaceItemInCell(Vector2Int position, GridItem item, bool force = false)
    {
        if (!Cells.TryGetValue(position, out var cell))
            return false;

        if (cell.IsOccupied && !force)
            return false;

        if (force && cell.IsOccupied)
        {
            var existingItem = cell.CurrentItem;
            existingItem.MarkForDestruction();
            cell.ClearItem();
        }

        cell.SetItem(item);
        item.SetGridPosition(position, cell);
        return true;
    }

    private void CreateCells()
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
                Cells.Add(new Vector2Int(x, y), cell);
            }
        }
    }

    public void ClearCell(Vector2Int position)
    {
        if (Cells.TryGetValue(position, out var cell))
        {
            var existingItem = cell.CurrentItem;
            existingItem?.MarkForDestruction();
            cell.ClearItem();
        }
    }

    public Vector2Int? FindNearestEmptyCell(Vector2Int position)
    {
        if (!IsCellOccupied(position) && Cells.ContainsKey(position))
            return position;

        for (int radius = 1; radius < gridSize.x + gridSize.y; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        continue;

                    Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);
                    if (checkPos.x >= 0 && checkPos.x < gridSize.x && 
                        checkPos.y >= 0 && checkPos.y < gridSize.y &&
                        !IsCellOccupied(checkPos) && !IsCellBlocked(checkPos) && Cells.ContainsKey(checkPos))
                    {
                        return checkPos;
                    }
                }
            }
        }

        return null;
    }

    public List<GridItem> FindItemsWithLevel(ItemType type, int level)
    {
        List<GridItem> items = new List<GridItem>();
        foreach (var cell in Cells.Values)
        {
            if (cell.CurrentItem != null && 
                cell.CurrentItem.properties.itemType == type && 
                cell.CurrentItem.CurrentLevel == level)
            {
                items.Add(cell.CurrentItem);
            }
        }
        return items;
    }
}
