using UnityEngine;
using System;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject itemsContainer;

    [Header("Item Properties")]
    [SerializeField] private ProducerItemProperties producerProperties;
    [SerializeField] private ResourceItemProperties energyProperties;
    [SerializeField] private ResourceItemProperties coinProperties;
    [SerializeField] private ProducedItemProperties producedItemProperties;
    [SerializeField] private ChestItemProperties chestProperties;

    [Header("Prefabs")]
    [SerializeField] private GameObject gridItemPrefab;

    public event Action OnGridItemCreated;

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

    public void SpawnTestItems()
    {
        CreateProducedItem(new Vector2Int(2, 2), 1);
        CreateProducedItem(new Vector2Int(3, 2), 1);
        CreateProducedItem(new Vector2Int(4, 2), 1);
        CreateProducedItem(new Vector2Int(2, 3), 2);
        CreateProducedItem(new Vector2Int(3, 3), 2);
        
        CreateProducerItem(new Vector2Int(1, 1), 3);
        CreateProducerItem(new Vector2Int(1, 2), 4);
        CreateProducerItem(new Vector2Int(1, 3), 5);
    }

    private GameObject CreateGridItemBase(string name)
    {
        if (gridItemPrefab == null)
        {
            return null;
        }

        if (itemsContainer == null)
        {
            itemsContainer = new GameObject("Items Container");
            itemsContainer.transform.SetParent(transform);
            itemsContainer.transform.localPosition = Vector3.zero;
        }

        GameObject itemObj = Instantiate(gridItemPrefab, itemsContainer.transform);
        itemObj.name = name;
        
        itemObj.transform.localScale *= GridManager.Instance.GridScaleMultiplier;
        OnGridItemCreated?.Invoke();
        return itemObj;
    }

    public GridItem CreateProducedItem(Vector2Int gridPosition, int level)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue || !GridManager.Instance.Cells.ContainsKey(emptyPos.Value)) 
            return null;

        GameObject itemObj = CreateGridItemBase("Produced Item");
        if (itemObj == null) return null;

        ProducedItem item = itemObj.AddComponent<ProducedItem>();
        item.Initialize(producedItemProperties, level);
        
        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            var cell = GridManager.Instance.Cells[emptyPos.Value];
            cell.SetItem(item);
            item.SetGridPosition(emptyPos.Value, cell);
            return item;
        }
        
        Destroy(itemObj);
        return null;
    }

    public GridItem CreateProducedItemWithAnimation(Vector2Int gridPosition, int level, Vector3 startPosition)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue || !GridManager.Instance.Cells.ContainsKey(emptyPos.Value)) 
            return null;

        GameObject itemObj = CreateGridItemBase("Produced Item");
        if (itemObj == null) return null;

        ProducedItem item = itemObj.AddComponent<ProducedItem>();
        ItemAnimator animator = itemObj.AddComponent<ItemAnimator>();
        item.Initialize(producedItemProperties, level);
        
        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            var cell = GridManager.Instance.Cells[emptyPos.Value];
            cell.SetItem(item);
            item.SetGridPosition(emptyPos.Value, cell);
            
            animator.AnimateProduction(startPosition, cell.transform.position);
            
            return item;
        }
        
        Destroy(itemObj);
        return null;
    }

    public GridItem CreateResourceItem(Vector2Int gridPosition, BaseItemProperties itemProperties, int level)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue || !GridManager.Instance.Cells.ContainsKey(emptyPos.Value)) 
            return null;

        GameObject itemObj = CreateGridItemBase($"{itemProperties.itemType} Item");
        if (itemObj == null) return null;

        ResourceItem item = itemObj.AddComponent<ResourceItem>();
        item.Initialize(itemProperties, level);
        
        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            var cell = GridManager.Instance.Cells[emptyPos.Value];
            cell.SetItem(item);
            item.SetGridPosition(emptyPos.Value, cell);
            return item;
        }
        
        Destroy(itemObj);
        return null;
    }

    public GridItem CreateMergedItem(Vector2Int gridPosition, ItemType type, int level)
    {
        GameObject itemObj = CreateGridItemBase($"Merged {type} Item");
        if (itemObj == null) return null;

        GridItem item = null;
        switch (type)
        {
            case ItemType.ProducedItem:
                item = itemObj.AddComponent<ProducedItem>();
                item.Initialize(producedItemProperties, level);
                break;
            case ItemType.Energy:
            case ItemType.Coin:
                item = itemObj.AddComponent<ResourceItem>();
                item.Initialize(type == ItemType.Energy ? energyProperties : coinProperties, level);
                break;
        }

        if (item != null)
        {
            if (GridManager.Instance.TryPlaceItemInCell(gridPosition, item, force: true))
            {
                return item;
            }
        }
        
        Destroy(itemObj);
        return null;
    }

    public GridItem CreateProducerItem(Vector2Int gridPosition, int level = 1)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue || !GridManager.Instance.Cells.ContainsKey(emptyPos.Value)) 
            return null;

        GameObject itemObj = CreateGridItemBase("Producer Item");
        if (itemObj == null) return null;

        ProducerItem item = itemObj.AddComponent<ProducerItem>();
        item.Initialize(producerProperties, level);

        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            var cell = GridManager.Instance.Cells[emptyPos.Value];
            cell.SetItem(item);
            item.SetGridPosition(emptyPos.Value, cell);
            return item;
        }
        
        Destroy(itemObj);
        return null;
    }

    public GridItem CreateChestItem(Vector2Int gridPosition, int level = 1)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue || !GridManager.Instance.Cells.ContainsKey(emptyPos.Value)) 
            return null;

        GameObject itemObj = CreateGridItemBase("Chest Item");
        if (itemObj == null) return null;

        ChestItem item = itemObj.AddComponent<ChestItem>();
        item.Initialize(chestProperties, level);

        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            var cell = GridManager.Instance.Cells[emptyPos.Value];
            cell.SetItem(item);
            item.SetGridPosition(emptyPos.Value, cell);
            return item;
        }
        
        Destroy(itemObj);
        return null;
    }
}