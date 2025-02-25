using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(9, 9);
    [SerializeField] private GridInitializer gridInitializer;
    [SerializeField] private float gridPadding = 0.5f;
    
    private InputManager inputManager;

    private void Start()
    {
        if (gridInitializer != null)
            gridInitializer.InitializeGrid(grid, gridSize);
            
        GridLayoutManager.PositionAndScaleGrid(grid, gridSize, gridPadding);

        SetupInputManager();
    }

    private void SetupInputManager()
    {
        GameObject inputObj = new GameObject("Grid Input Manager");
        inputObj.transform.SetParent(transform);
        inputManager = inputObj.AddComponent<InputManager>();
        inputManager.Initialize(grid);
    }
}
