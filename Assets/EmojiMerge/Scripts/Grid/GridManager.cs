using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private Grid grid;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(9, 9);
    [SerializeField] private GridInitializer gridInitializer;
    [SerializeField] private GridStyling gridStyling;
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private float gridPadding = 0.5f;
    
    private InputManager inputManager;
    
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
    }

    private void Start()
    {
        SetupInputManager();
        SetupMergeManager();

        if (gridInitializer != null)
            gridInitializer.InitializeGrid(grid, gridSize);
            
        if (gridStyling != null)
            gridStyling.Initialize(this);
            
        GridLayoutManager.PositionAndScaleGrid(grid, gridSize, gridPadding);
        GridScaleMultiplier = grid.transform.localScale;
        OnGridResized?.Invoke();
        OnGridInitialized?.Invoke();
        itemManager.SpawnTestItems();

    }

    private void SetupInputManager()
    {
        GameObject inputObj = new GameObject("Grid Input Manager");
        inputManager = inputObj.AddComponent<InputManager>();
        inputManager.Initialize(grid);
        inputManager.OnTouchEnd += CheckCellTapped;
    }

    private void SetupMergeManager()
    {
        GameObject mergeObj = new GameObject("Grid Merge Manager");
        mergeObj.AddComponent<MergeManager>();
    }

    private void CheckCellTapped(Vector2Int position)
    {
        if (position.x >= 0 && position.x < gridSize.x && 
            position.y >= 0 && position.y < gridSize.y && 
            IsCellOccupied(position))
        {
            var item = GetItemAtCell(position);
            item?.OnTapped();
        } else
        {
            UIManager.Instance.CloseItemDetailsPane();
        }
    }

    public bool IsCellOccupied(Vector2Int position)
    {
        return Cells.TryGetValue(position, out var cell) && cell.IsOccupied;
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
            cell.ClearItem();
        }

        cell.SetItem(item);
        item.SetGridPosition(position, cell);
        return true;
    }

    public void ClearCell(Vector2Int position)
    {
        if (Cells.TryGetValue(position, out var cell))
        {
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
                        !IsCellOccupied(checkPos) && Cells.ContainsKey(checkPos))
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
