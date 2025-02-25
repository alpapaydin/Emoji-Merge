using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public event Action<Vector2Int> OnTouchStart;
    public event Action<Vector2Int> OnTouchEnd;
    public event Action<Vector2> OnTouchDrag;

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
        OnTouchStart?.Invoke(GetGridPosition(screenPosition));
    }

    private void HandleTouchDrag(Vector2 screenPosition)
    {
        if (Vector2.Distance(screenPosition, lastTouchPosition) > 20f)
        {
            isDragging = true;
            OnTouchDrag?.Invoke(screenPosition);
        }
        lastTouchPosition = screenPosition;
    }

    private void HandleTouchEnd(Vector2 screenPosition)
    {
        if (!isDragging)
        {
            OnTouchEnd?.Invoke(GetGridPosition(screenPosition));
        }
    }

    private Vector2Int GetGridPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        Vector3Int cellPosition = grid.WorldToCell(worldPosition);
        return new Vector2Int(cellPosition.x, cellPosition.y);
    }
}
