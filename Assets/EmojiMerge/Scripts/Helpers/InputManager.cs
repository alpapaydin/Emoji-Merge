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

    // Drag and drop state
    private GridItem draggedItem;
    private SpriteRenderer dragPreview;
    private Vector2Int dragStartGridPos;
    private GridItem currentHoverTarget;

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

        var item = GridManager.Instance.GetItemAtCell(dragStartGridPos);
        if (item != null && item.IsReadyToMerge)
        {
            draggedItem = item;
            dragPreview.sprite = item.GetComponent<SpriteRenderer>().sprite;
            dragPreview.transform.position = item.transform.position;
            dragPreview.transform.localScale = item.transform.localScale;
            dragPreview.gameObject.SetActive(true);
            
            var itemRenderer = item.GetComponent<SpriteRenderer>();
            itemRenderer.color = new Color(1, 1, 1, 0.5f);
        }

        OnTouchStart?.Invoke(dragStartGridPos);
    }

    private void HandleTouchDrag(Vector2 screenPosition)
    {
        if (Vector2.Distance(screenPosition, lastTouchPosition) > dragThreshold || isDragging)
        {
            isDragging = true;

            if (draggedItem != null)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 
                    -mainCamera.transform.position.z + draggedItem.transform.position.z));
                dragPreview.transform.position = worldPos;

                Vector2Int currentGridPos = GetGridPosition(screenPosition);
                UpdateMergeTarget(currentGridPos);
            }

            OnTouchDrag?.Invoke(screenPosition);
        }
        lastTouchPosition = screenPosition;
    }

    private void HandleTouchEnd(Vector2 screenPosition)
    {
        //rework
        Vector2Int endGridPos = GetGridPosition(screenPosition);

        if (draggedItem != null)
        {
            var itemRenderer = draggedItem.GetComponent<SpriteRenderer>();
            itemRenderer.color = Color.white;

            if (isDragging && currentHoverTarget != null)
            {
                if (draggedItem.CanMerge(currentHoverTarget))
                {
                    Vector2Int pos1 = draggedItem.GridPosition;
                    Vector2Int pos2 = currentHoverTarget.GridPosition;
                    Vector2Int mergePos = pos2;

                    if (endGridPos.x >= 0 && endGridPos.x < GridManager.Instance.GridSize.x &&
                        endGridPos.y >= 0 && endGridPos.y < GridManager.Instance.GridSize.y &&
                        GridManager.Instance.Cells.ContainsKey(endGridPos))
                    {
                        mergePos = endGridPos;
                    }
                    
                    GridManager.Instance.ClearCell(pos1);
                    GridManager.Instance.ClearCell(pos2);

                    GridItem mergedItem = draggedItem.MergeWith(currentHoverTarget, mergePos);
                    
                    if (mergedItem != null)
                    {
                        Destroy(draggedItem.gameObject);
                        Destroy(currentHoverTarget.gameObject);
                    }
                    else
                    {
                        // restore the cells on fail
                        GridManager.Instance.TryPlaceItemInCell(pos1, draggedItem);
                        GridManager.Instance.TryPlaceItemInCell(pos2, currentHoverTarget);
                    }
                }
            }

            dragPreview.gameObject.SetActive(false);
            ClearMergeHighlight();
            draggedItem = null;
            currentHoverTarget = null;
        }

        if (!isDragging)
        {
            OnTouchEnd?.Invoke(endGridPos);
        }

        isDragging = false;
    }

    private void UpdateMergeTarget(Vector2Int gridPos)
    {
        ClearMergeHighlight();

        if (gridPos == dragStartGridPos)
            return;

        var targetItem = GridManager.Instance.GetItemAtCell(gridPos);
        if (targetItem != null && draggedItem.CanMerge(targetItem))
        {
            currentHoverTarget = targetItem;
            HighlightMergeTarget(true);
        }
        else if (targetItem != null)
        {
            currentHoverTarget = targetItem;
            HighlightMergeTarget(false);
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
