using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public event Action<Vector2Int> OnTouchStart;
    public event Action<Vector2Int> OnTouchEnd;
    public event Action<Vector2> OnTouchDrag;

    [Header("Drag and Drop Settings")]
    [SerializeField] private float dragThreshold = 1f;
    [SerializeField] private Color validMergeHighlight = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color invalidMergeHighlight = new Color(1, 0, 0, 0.3f);

    private Grid grid;
    private Vector2 lastTouchPosition;
    private bool isDragging;
    private Camera mainCamera;

    private GridItem draggedItem;
    private SpriteRenderer dragPreview;
    private Vector2Int dragStartGridPos;
    private GridItem currentHoverTarget;

    public static bool IsDragging { get; private set; }

    public void Initialize(Grid targetGrid)
    {
        grid = targetGrid;
        mainCamera = Camera.main;
        CreateDragPreview();
    }

    private void CreateDragPreview()
    {
        GameObject previewObj = new GameObject("Drag Preview");
        previewObj.transform.SetParent(transform);
        dragPreview = previewObj.AddComponent<SpriteRenderer>();
        dragPreview.sortingOrder = 100;
        dragPreview.gameObject.SetActive(false);
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
        dragStartGridPos = GetGridPosition(screenPosition);

        TryStartDraggingItem(dragStartGridPos);
        OnTouchStart?.Invoke(dragStartGridPos);
    }

    private void TryStartDraggingItem(Vector2Int gridPosition)
    {
        var item = GridManager.Instance.GetItemAtCell(gridPosition);
        if (item != null && item.IsReadyToMerge && !item.IsPendingDestruction)
        {
            draggedItem = item;
            SetupDragPreview(item);
            SetItemDragVisuals(item, true);
        }
    }

    private void SetupDragPreview(GridItem item)
    {
        dragPreview.sprite = item.GetComponent<SpriteRenderer>().sprite;
        dragPreview.transform.position = item.transform.position;
        dragPreview.transform.localScale = item.transform.localScale;
        dragPreview.gameObject.SetActive(true);
    }

    private void SetItemDragVisuals(GridItem item, bool isDragging)
    {
        var itemRenderer = item.GetComponent<SpriteRenderer>();
        itemRenderer.color = isDragging ? new Color(1, 1, 1, 0.5f) : Color.white;
    }

    private void HandleTouchDrag(Vector2 screenPosition)
    {
        if (!IsDragThresholdExceeded(screenPosition) && !isDragging)
            return;

        isDragging = true;
        IsDragging = true;
        
        if (draggedItem != null)
        {
            UpdateDragPreviewPosition(screenPosition);
            UpdateMergeTarget(GetGridPosition(screenPosition));
        }

        OnTouchDrag?.Invoke(screenPosition);
        lastTouchPosition = screenPosition;
    }

    private bool IsDragThresholdExceeded(Vector2 screenPosition)
    {
        return Vector2.Distance(screenPosition, lastTouchPosition) > dragThreshold;
    }

    private void UpdateDragPreviewPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x, 
            screenPosition.y, 
            -mainCamera.transform.position.z + draggedItem.transform.position.z
        ));
        dragPreview.transform.position = worldPos;
    }

    private void HandleTouchEnd(Vector2 screenPosition)
    {
        Vector2Int endGridPos = GetGridPosition(screenPosition);

        if (draggedItem != null)
        {
            SetItemDragVisuals(draggedItem, false);

            if (isDragging && currentHoverTarget != null)
            {
                HandleDraggedItemRelease(endGridPos);
            }
            else
            {
                ReturnDraggedItemToOriginalPosition();
            }

            CleanupDragState();
        }

        if (!isDragging)
        {
            OnTouchEnd?.Invoke(endGridPos);
        }

        isDragging = false;
        IsDragging = false;
    }

    private void HandleDraggedItemRelease(Vector2Int endGridPos)
    {
        if (draggedItem.CanMerge(currentHoverTarget))
        {
            Vector2Int mergePos = GetValidMergePosition(endGridPos);
            MergeManager.Instance.PerformMerge(draggedItem, currentHoverTarget, mergePos);
        }
        else
        {
            ReturnDraggedItemToOriginalPosition();
        }
    }

    private Vector2Int GetValidMergePosition(Vector2Int endGridPos)
    {
        if (IsValidGridPosition(endGridPos))
        {
            return endGridPos;
        }
        return currentHoverTarget.GridPosition;
    }

    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && 
               position.x < GridManager.Instance.GridSize.x &&
               position.y >= 0 && 
               position.y < GridManager.Instance.GridSize.y &&
               GridManager.Instance.Cells.ContainsKey(position);
    }

    private void ReturnDraggedItemToOriginalPosition()
    {
        draggedItem.transform.position = draggedItem.OccupiedCell.transform.position;
    }

    private void CleanupDragState()
    {
        dragPreview.gameObject.SetActive(false);
        ClearMergeHighlight();
        draggedItem = null;
        currentHoverTarget = null;
    }

    private void UpdateMergeTarget(Vector2Int gridPos)
    {
        ClearMergeHighlight();

        if (gridPos == dragStartGridPos)
            return;

        var targetItem = GridManager.Instance.GetItemAtCell(gridPos);
        UpdateMergeHighlight(targetItem, gridPos);
    }

    private void UpdateMergeHighlight(GridItem targetItem, Vector2Int gridPos)
    {
        if (targetItem != null)
        {
            bool canMerge = draggedItem.CanMerge(targetItem);
            currentHoverTarget = targetItem;
            HighlightMergeTarget(canMerge);
        }
        else
        {
            currentHoverTarget = null;
        }
    }

    private void HighlightMergeTarget(bool isValid)
    {
        if (currentHoverTarget != null)
        {
            var targetRenderer = currentHoverTarget.GetComponent<SpriteRenderer>();
            if (isValid)
            {
                Sprite nextLevelSprite = draggedItem.GetNextLevelSprite();
                if (nextLevelSprite != null)
                {
                    targetRenderer.sprite = nextLevelSprite;
                    targetRenderer.color = new Color(1, 1, 1, 0.5f);
                }
            }
            else
            {
                targetRenderer.color = invalidMergeHighlight;
            }
        }
    }

    private void ClearMergeHighlight()
    {
        if (currentHoverTarget != null)
        {
            var targetRenderer = currentHoverTarget.GetComponent<SpriteRenderer>();
            targetRenderer.sprite = currentHoverTarget.GetComponent<GridItem>().properties.levelSprites[currentHoverTarget.CurrentLevel - 1];
            targetRenderer.color = Color.white;
            currentHoverTarget = null;
        }
    }

    private Vector2Int GetGridPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z));
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }

    private void OnDestroy()
    {
        if (dragPreview != null)
        {
            Destroy(dragPreview.gameObject);
        }
    }
}
