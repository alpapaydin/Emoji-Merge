using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [SerializeField] private ItemManager itemManager;
    [SerializeField] private Sprite[] customerSprites;
    [SerializeField] private float checkCooldown = 5f;
    
    
    private OrderData[] possibleOrders;
    private int maxOrders = 3;
    private float orderCooldown = 5f;
    private List<Order> currentOrders = new List<Order>();
    private HashSet<OrderData> usedOrders = new HashSet<OrderData>();
    private float lastOrderTime;
    private float lastCheckTime;
    private bool isInitialized;
    private int ordersCompleted = 0;


    public int OrdersCompleted => ordersCompleted;
    public event Action<Order> OnOrderCanBeCompleted;
    public event Action<Order> OnOrderCompleted;
    public event Action<Order> OnOrderAdded;

    public void Initialize(RandomLevelData levelData)
    {
        possibleOrders = levelData.possibleOrders;
        maxOrders = levelData.maxOrders;
        orderCooldown = levelData.orderCooldown;
        isInitialized = true;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            lastOrderTime = Time.time - orderCooldown;
            
            if (itemManager == null)
            {
                itemManager = FindObjectOfType<ItemManager>();
            }
            
            itemManager.OnGridItemCreated += OnItemCreated;
            itemManager.OnGridItemDestroyed += OnItemDestroyed;
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

        if (itemManager != null)
        {
            itemManager.OnGridItemCreated -= OnItemCreated;
            itemManager.OnGridItemDestroyed -= OnItemDestroyed;
        }
    }

    private void Update()
    {
        if (!isInitialized) return;
        if (Time.time - lastCheckTime < checkCooldown) return;
        lastCheckTime = Time.time;

        if (currentOrders.Count < maxOrders && Time.time - lastOrderTime >= orderCooldown)
        {
            TryAddNewOrder();
        }

        currentOrders.RemoveAll(order => order.IsCompleted);
    }

    public void OnItemUnblocked(GridItem item)
    {
        OnItemCreated(item);
    }

    private void OnItemCreated(GridItem item)
    {
        if (item == null) return;

        var needingOrders = currentOrders.Where(order => 
            !order.CanBeCompleted && 
            order.RequiresItem(item.properties, item.CurrentLevel)).ToList();

        var otherOrders = currentOrders.Where(order => 
            order.CanBeCompleted && 
            order.RequiresItem(item.properties, item.CurrentLevel)).ToList();

        foreach (var order in needingOrders)
        {
            bool statusChanged = order.TryMarkAvailableItems();
            if (statusChanged)
            {
                OnOrderCanBeCompleted?.Invoke(order);
            }
        }

        foreach (var order in otherOrders)
        {
            bool statusChanged = order.TryMarkAvailableItems();
            if (statusChanged)
            {
                OnOrderCanBeCompleted?.Invoke(order);
            }
        }
    }

    private void OnItemDestroyed(GridItem item)
    {
        if (item == null) return;

        var affectedOrders = item.MarkingOrders.ToList();
        
        var otherPotentialOrders = currentOrders.Where(order => 
            !affectedOrders.Contains(order) && 
            order.RequiresItem(item.properties, item.CurrentLevel)).ToList();
            
        affectedOrders.AddRange(otherPotentialOrders);

        foreach (var order in affectedOrders.Distinct())
        {
            bool wasComplete = order.CanBeCompleted;
            order.ClearMarkedItems();
            order.TryMarkAvailableItems();
            
            if (wasComplete != order.CanBeCompleted)
            {
                OnOrderCanBeCompleted?.Invoke(order);
            }
        }
    }

    private void OnItemChanged(GridItem item)
    {
        var affectedOrders = currentOrders.Where(order => 
            item == null ||
            order.CanBeCompleted ||
            order.RequiresItem(item.properties, item.CurrentLevel)).ToList();

        foreach (var order in affectedOrders)
        {
            order.ClearMarkedItems();
        }

        foreach (var order in affectedOrders)
        {
            order.TryMarkAvailableItems();
            order.UpdateCompletionStatus();
            
            OnOrderCanBeCompleted?.Invoke(order);
        }
    }

    private void TryAddNewOrder()
    {
        if (usedOrders.Count >= possibleOrders.Length)
        {
            usedOrders.Clear();
        }

        var availableOrders = possibleOrders.Where(order => !usedOrders.Contains(order)).ToList();
        if (availableOrders.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableOrders.Count);
            OrderData selectedOrder = availableOrders[randomIndex];
            
            var order = new Order(selectedOrder, GetUniqueCustomerSprite());
            currentOrders.Add(order);
            usedOrders.Add(selectedOrder);
            lastOrderTime = Time.time;
            
            order.TryMarkAvailableItems();
            order.UpdateCompletionStatus();
            
            OnOrderAdded?.Invoke(order);
            SoundManager.Instance.PlaySound("newOrder");

            if (order.CanBeCompleted)
            {
                OnOrderCanBeCompleted?.Invoke(order);
            }
        }
    }

    private Sprite GetUniqueCustomerSprite()
    {
        if (customerSprites.Length == 0)
            return null;

        var availableSprites = customerSprites.ToList();
        foreach (var order in currentOrders)
        {
            availableSprites.Remove(order.CustomerSprite);
        }
        return availableSprites[UnityEngine.Random.Range(0, availableSprites.Count)];
    }

    public bool TryCompleteOrder(Order order)
    {
        if (!order.CanBeCompleted || !currentOrders.Contains(order))
            return false;

        var itemsToDestroy = order.GetMarkedItems().ToList();
        
        if (itemsToDestroy.Count == 0)
            return false;
            
        currentOrders.Remove(order);
        OnOrderCompleted?.Invoke(order);
        ordersCompleted++;
        SoundManager.Instance.PlaySound("orderComplete");
        
        var affectedOrders = currentOrders.Where(o => 
            itemsToDestroy.Any(item => o.RequiresItem(item.properties, item.CurrentLevel))).ToList();
            
        order.ClearMarkedItems();

        foreach (var item in itemsToDestroy)
        {
            if (item != null)
            {
                foreach (var affectedOrder in affectedOrders)
                {
                    if (item.MarkingOrders.Contains(affectedOrder))
                    {
                        item.RemoveOrderMark(affectedOrder);
                    }
                }
                DeliverItem(item);
            }
        }

        foreach (var affectedOrder in affectedOrders)
        {
            affectedOrder.ClearMarkedItems();
            affectedOrder.TryMarkAvailableItems();
            affectedOrder.UpdateCompletionStatus();
            
            if (affectedOrder.CanBeCompleted)
            {
                OnOrderCanBeCompleted?.Invoke(affectedOrder);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(order.data.goldReward);
            if (order.data.energyReward > 0)
            {
                GameManager.Instance.AddEnergy(order.data.energyReward);
            }
        }

        return true;
    }

    private void DeliverItem(GridItem item)
    {
        if (UIManager.Instance.ItemDetails.CurrentItem == item)
        {
            UIManager.Instance.CloseItemDetailsPane();
        }
        itemManager.DestroyItem(item);
    }

    public IReadOnlyList<Order> GetCurrentOrders()
    {
        return currentOrders;
    }

    public void ClearDeliveryMarks()
    {
        foreach (var order in currentOrders)
        {
            order.ClearMarkedItems();
        }
    }
}