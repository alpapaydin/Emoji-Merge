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
    [SerializeField] private float gridPadding = 0.5f;
    
    private InputManager inputManager;
    private Dictionary<Vector2Int, GridItem> itemsOnGrid = new Dictionary<Vector2Int, GridItem>();
    
    public Vector2 GridScaleMultiplier {get; private set;} = new Vector2(1f, 1f);
    public Dictionary<Vector2Int, Cell> Cells = new Dictionary<Vector2Int, Cell>();
    public InputManager Inputs => inputManager;
    public Grid Grid => grid;
    public GridStyling Styling => gridStyling;
    public Vector2Int GridSize => gridSize;
    public event Action OnGridInitialized;

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

        if (gridInitializer != null)
            gridInitializer.InitializeGrid(grid, gridSize);
            
        if (gridStyling != null)
            gridStyling.Initialize(this);
            
        GridLayoutManager.PositionAndScaleGrid(grid, gridSize, gridPadding);
        GridScaleMultiplier = grid.transform.localScale;
        OnGridInitialized?.Invoke();
    }

    private void SetupInputManager()
    {
        GameObject inputObj = new GameObject("Grid Input Manager");
        inputObj.transform.SetParent(transform);
        inputManager = inputObj.AddComponent<InputManager>();
        inputManager.Initialize(grid);
        inputManager.OnTouchEnd += CheckCellTapped;
    }

    private void CheckCellTapped(Vector2Int position)
    {
        if (position.x >= 0 && position.x < gridSize.x && 
            position.y >= 0 && position.y < gridSize.y && 
            IsCellOccupied(position))
        {
            var item = GetItemAtCell(position);
            item?.OnTapped();
        }
    }

    public bool IsCellOccupied(Vector2Int position)
    {
        return itemsOnGrid.ContainsKey(position);
    }

    public GridItem GetItemAtCell(Vector2Int position)
    {
        return itemsOnGrid.TryGetValue(position, out var item) ? item : null;
    }

    public bool TryPlaceItemInCell(Vector2Int position, GridItem item)
    {
        if (!Cells.TryGetValue(position, out var cell) || IsCellOccupied(position))
            return false;

        itemsOnGrid[position] = item;
        item.SetGridPosition(position, cell);
        return true;
    }

    public void ClearCell(Vector2Int position)
    {
        if (itemsOnGrid.ContainsKey(position))
        {
            itemsOnGrid.Remove(position);
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
}
