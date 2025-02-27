using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }
    
    [SerializeField] private ItemManager itemManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (itemManager == null)
            {
                itemManager = FindObjectOfType<ItemManager>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanMergeItems(GridItem item1, GridItem item2)
    {
        if (item1 == null || item2 == null)
            return false;

        return item1.properties.itemType == item2.properties.itemType &&
               item1.CurrentLevel == item2.CurrentLevel &&
               item1.CurrentLevel < item1.properties.maxLevel;
    }

    public GridItem PerformMerge(GridItem sourceItem, GridItem targetItem, Vector2Int mergePosition)
    {
        if (!CanMergeItems(sourceItem, targetItem))
            return null;

        sourceItem.SetMergeState(true);
        targetItem.SetMergeState(true);

        Vector2Int sourcePos = sourceItem.GridPosition;
        Vector2Int targetPos = targetItem.GridPosition;

        GridManager.Instance.ClearCell(sourcePos);
        GridManager.Instance.ClearCell(targetPos);

        GridItem mergedItem = ItemManager.Instance.CreateMergedItem(
            mergePosition,
            sourceItem.properties.itemType,
            sourceItem.CurrentLevel + 1
        );

        if (mergedItem != null)
        {
            itemManager.DestroyItem(sourceItem);
            itemManager.DestroyItem(targetItem);
            return mergedItem;
        }
        else
        {
            sourceItem.SetMergeState(false);
            targetItem.SetMergeState(false);
            
            GridManager.Instance.TryPlaceItemInCell(sourcePos, sourceItem);
            GridManager.Instance.TryPlaceItemInCell(targetPos, targetItem);
            return null;
        }
    }
}