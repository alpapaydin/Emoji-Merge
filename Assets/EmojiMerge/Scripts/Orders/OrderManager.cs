using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [SerializeField] private ItemManager itemManager;
    [SerializeField] private OrderData[] possibleOrders;
    [SerializeField] private Sprite[] customerSprites;
    [SerializeField] private int maxOrders = 3;
    [SerializeField] private float orderCooldown = 5f;
    
    private List<Order> currentOrders = new List<Order>();
    private HashSet<OrderData> usedOrders = new HashSet<OrderData>();
    private float lastOrderTime;

    public event Action<Order> OnOrderCanBeCompleted;
    public event Action<Order> OnOrderCompleted;
    public event Action<Order> OnOrderAdded;

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
            
            itemManager.OnGridItemCreated += OnItemChanged;
            itemManager.OnGridItemDestroyed += OnItemChanged;
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
            itemManager.OnGridItemCreated -= OnItemChanged;
            itemManager.OnGridItemDestroyed -= OnItemChanged;
        }
    }

    private void Update()
    {
        if (currentOrders.Count < maxOrders && Time.time - lastOrderTime >= orderCooldown)
        {
            TryAddNewOrder();
        }

        currentOrders.RemoveAll(order => order.IsCompleted);
    }

    private void OnItemChanged(GridItem item)
    {
        foreach (var order in currentOrders)
        {
            bool wasCompletable = order.CanBeCompleted;
            order.OnItemChanged(item);
            
            if (!wasCompletable && order.CanBeCompleted)
            {
                OnOrderCanBeCompleted?.Invoke(order);
            }
            else if (wasCompletable && !order.CanBeCompleted)
            {
                order.ClearMarkedItems();
            }
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
            print("Order added");
            var order = new Order(selectedOrder, GetUniqueCustomerSprite());
            currentOrders.Add(order);
            usedOrders.Add(selectedOrder);
            lastOrderTime = Time.time;
            OnOrderAdded?.Invoke(order);
            order.UpdateCompletionStatus();
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

        foreach (var item in order.GetMarkedItems())
        {
            if (item != null)
            {
                itemManager.DestroyItem(item);
            }
        }

        GameManager.Instance.AddCoins(order.data.goldReward);
        if (order.data.energyReward > 0)
        {
            GameManager.Instance.AddEnergy(order.data.energyReward);
        }

        OnOrderCompleted?.Invoke(order);
        currentOrders.Remove(order);
        return true;
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