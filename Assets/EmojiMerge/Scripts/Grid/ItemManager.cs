using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject itemsContainer;

    [Header("Item Properties")]
    [SerializeField] private ProducerItemProperties[] producerProperties;
    [SerializeField] private ResourceItemProperties energyProperties;
    [SerializeField] private ResourceItemProperties coinProperties;
    [SerializeField] private ProducedItemProperties producedItemProperties;
    [SerializeField] private ChestItemProperties chestProperties;

    [Header("Prefabs")]
    [SerializeField] private GameObject gridItemPrefab;

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
        itemObj.transform.localScale *= GridManager.Instance.GridScaleMultiplier;
        itemObj.name = name;
        return itemObj;
    }

    public GridItem CreateProducedItem(Vector2Int gridPosition, int level)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue) return null;

        GameObject itemObj = CreateGridItemBase("Produced Item");
        if (itemObj == null) return null;

        ProducedItem item = itemObj.AddComponent<ProducedItem>();
        item.Initialize(producedItemProperties, level);
        
        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            return item;
        }
        else
        {
            Destroy(itemObj);
            return null;
        }
    }

    public GridItem CreateResourceItem(Vector2Int gridPosition, ItemType type, int level)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue) return null;

        GameObject itemObj = CreateGridItemBase($"{type} Item");
        if (itemObj == null) return null;

        ResourceItem item = itemObj.AddComponent<ResourceItem>();
        item.Initialize(type == ItemType.Energy ? energyProperties : coinProperties, level);
        
        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            return item;
        }
        else
        {
            Destroy(itemObj);
            return null;
        }
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

        if (item != null && GridManager.Instance.TryPlaceItemInCell(gridPosition, item))
        {
            return item;
        }
        else
        {
            Destroy(itemObj);
            return null;
        }
    }

    public GridItem CreateProducerItem(Vector2Int gridPosition, int producerIndex, int level = 1)
    {
        if (producerIndex < 0 || producerIndex >= producerProperties.Length)
            return null;

        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue) return null;

        GameObject itemObj = CreateGridItemBase("Producer Item");
        if (itemObj == null) return null;

        ProducerItem item = itemObj.AddComponent<ProducerItem>();
        item.Initialize(producerProperties[producerIndex], level);

        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            return item;
        }
        else
        {
            Destroy(itemObj);
            return null;
        }
    }

    public GridItem CreateChestItem(Vector2Int gridPosition, int level = 1)
    {
        var emptyPos = GridManager.Instance.FindNearestEmptyCell(gridPosition);
        if (!emptyPos.HasValue) return null;

        GameObject itemObj = CreateGridItemBase("Chest Item");
        if (itemObj == null) return null;

        ChestItem item = itemObj.AddComponent<ChestItem>();
        item.Initialize(chestProperties, level);

        if (GridManager.Instance.TryPlaceItemInCell(emptyPos.Value, item))
        {
            return item;
        }
        else
        {
            Destroy(itemObj);
            return null;
        }
    }
}