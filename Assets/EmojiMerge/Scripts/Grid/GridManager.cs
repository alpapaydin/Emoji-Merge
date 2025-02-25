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

    public Dictionary<Vector2Int, Cell> Cells = new Dictionary<Vector2Int, Cell>();
    public InputManager Inputs => inputManager;
    public Grid Grid => grid;
    public GridStyling Styling => gridStyling;
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
        OnGridInitialized?.Invoke();
    }

    private void SetupInputManager()
    {
        GameObject inputObj = new GameObject("Grid Input Manager");
        inputObj.transform.SetParent(transform);
        inputManager = inputObj.AddComponent<InputManager>();
        inputManager.Initialize(grid);
        inputManager.OnTouchStart += CheckCellTapped;
    }

    private void CheckCellTapped(Vector2Int position)
    {
        if (position.x >= 0 && position.x < gridSize.x && position.y >= 0 && position.y < gridSize.y)
        {
            Cells[position].OnTapped();
        }
    }

}
