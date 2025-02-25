using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Grid grid;
    private Vector2 lastTouchPosition;
    private bool isDragging;
    private Camera mainCamera;

    public void Initialize(Grid targetGrid)
    {
        grid = targetGrid;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (grid == null || mainCamera == null)
        {
            Debug.LogError("InputManager not properly initialized!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouchStart(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            HandleTouchDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleTouchEnd(Input.mousePosition);
        }
    }

    private void HandleTouchStart(Vector2 screenPosition)
    {
        lastTouchPosition = screenPosition;
        isDragging = false;
        CheckGridPosition(screenPosition, "Tapped");
    }

    private void HandleTouchDrag(Vector2 screenPosition)
    {
        if (Vector2.Distance(screenPosition, lastTouchPosition) > 20f)
        {
            isDragging = true;
            CheckGridPosition(screenPosition, "Dragging over");
        }
        lastTouchPosition = screenPosition;
    }

    private void HandleTouchEnd(Vector2 screenPosition)
    {
        if (!isDragging)
        {
            CheckGridPosition(screenPosition, "Released at");
        }
    }

    private void CheckGridPosition(Vector2 screenPosition, string actionType)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        Vector3Int cellPosition = grid.WorldToCell(worldPosition);
        
        // Check if the position is within grid bounds
        if (cellPosition.x >= 0 && cellPosition.x < 9 && cellPosition.y >= 0 && cellPosition.y < 9)
        {
            Debug.Log($"{actionType} grid cell at: {cellPosition}");
            Vector3 cellCenter = grid.GetCellCenterLocal(cellPosition);
            Debug.Log($"Cell center world position: {cellCenter}");
        }
        else
        {
            Debug.Log($"{actionType} outside grid: {cellPosition}");
        }
    }
}
