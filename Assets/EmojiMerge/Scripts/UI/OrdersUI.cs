using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OrdersUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset orderTemplate;
    [SerializeField] private VisualTreeAsset orderItemTemplate;
    [SerializeField] private OrderManager orderManager;
    
    private VisualElement ordersContainer;
    private Dictionary<Order, VisualElement> orderElements = new Dictionary<Order, VisualElement>();
    private Dictionary<Order, Dictionary<(BaseItemProperties, int), VisualElement>> orderItemElements 
        = new Dictionary<Order, Dictionary<(BaseItemProperties, int), VisualElement>>();

    private void Awake()
    {
        if (orderManager == null)
        {
            orderManager = FindObjectOfType<OrderManager>();
        }
        
        ordersContainer = uiDocument.rootVisualElement.Q<VisualElement>("OrdersPanel");

        orderManager.OnOrderAdded += AddOrder;
        orderManager.OnOrderCompleted += OnOrderCompleted;
        orderManager.OnOrderCanBeCompleted += UpdateOrderCompletionStatus;
    }

    private void OnDisable()
    {
        if (orderManager != null)
        {
            orderManager.OnOrderAdded -= AddOrder;
            orderManager.OnOrderCompleted -= OnOrderCompleted;
            orderManager.OnOrderCanBeCompleted -= UpdateOrderCompletionStatus;
        }
    }

    private void OnOrderCompleted(Order order)
    {
        if (orderElements.TryGetValue(order, out var element))
        {
            ordersContainer.Remove(element);
            orderElements.Remove(order);
            orderItemElements.Remove(order);
        }
    }

    private void UpdateOrderCompletionStatus(Order order)
    {
        if (!orderElements.TryGetValue(order, out var element))
            return;

        var completeButton = element.Q<Button>("CompleteButton");
        if (completeButton != null)
        {
            completeButton.style.display = order.CanBeCompleted ? DisplayStyle.Flex : DisplayStyle.None;
        }

        UpdateOrderItemsUI(order);
    }

    public void AddOrder(Order order)
    {
        if (orderElements.ContainsKey(order))
            return;

        var orderElement = orderTemplate.CloneTree();
        
        var customerFrame = orderElement.Q<VisualElement>("CustomerFrame");
        customerFrame.style.backgroundImage = new StyleBackground(order.CustomerSprite);
        
        var completeButton = orderElement.Q<Button>("CompleteButton");
        completeButton.clicked += () => orderManager.TryCompleteOrder(order);
        completeButton.style.display = order.CanBeCompleted ? DisplayStyle.Flex : DisplayStyle.None;
        
        orderElements.Add(order, orderElement);
        orderItemElements.Add(order, new Dictionary<(BaseItemProperties, int), VisualElement>());
        
        var orderItemsContainer = orderElement.Q<VisualElement>("OrderItems");
        var orderDetails = orderElement.Q<VisualElement>("OrderDetails");
        AddOrderItems(order, orderItemsContainer);
        
        ordersContainer.Add(orderElement);
        StartCoroutine(AnimateOrder(customerFrame, orderDetails));

        order.OnItemsUpdated += (updatedOrder) => UpdateOrderItemsUI(updatedOrder);
    }

    private void AddOrderItems(Order order, VisualElement itemsContainer)
    {
        foreach (var item in order.GetRemainingItems())
        {
            var itemKey = item.Key;
            var orderItemElement = orderItemTemplate.CloneTree();
            
            VisualElement orderItemIcon = orderItemElement.Q<VisualElement>("ItemIcon");
            Label orderItemCount = orderItemElement.Q<Label>("ItemCount");
            VisualElement completedTick = orderItemElement.Q<VisualElement>("CompletedTick");
            
            orderItemIcon.style.backgroundImage = new StyleBackground(itemKey.Item1.levelSprites[itemKey.Item2 - 1]);
            
            orderItemCount.text = item.Value > 1 ? item.Value.ToString() : "";
            if (completedTick != null)
            {
                completedTick.style.display = DisplayStyle.None;
            }

            orderItemElements[order][itemKey] = orderItemElement;
            itemsContainer.Add(orderItemElement);
        }

        UpdateOrderItemsUI(order);
    }

    private void UpdateOrderItemsUI(Order order)
    {
        if (!orderElements.ContainsKey(order)) return;

        var remainingItems = order.GetRemainingItems();
        var markedItems = order.GetMarkedItems();
        var orderItemElementsDict = orderItemElements[order];
        
        foreach (var kvp in orderItemElementsDict)
        {
            var itemKey = kvp.Key;
            var element = kvp.Value;
            
            if (!remainingItems.TryGetValue(itemKey, out int needed))
                continue;

            int markedCount = markedItems.Count(item => 
                item != null && 
                item.properties == itemKey.Item1 && 
                item.CurrentLevel == itemKey.Item2);

            var completedTick = element.Q<VisualElement>("CompletedTick");
            if (completedTick != null)
            {
                completedTick.style.display = markedCount >= needed ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var countLabel = element.Q<Label>("ItemCount");
            if (countLabel != null)
            {
                if (markedCount > 0)
                {
                    countLabel.text = $"{markedCount}/{needed}";
                }
                else
                {
                    countLabel.text = needed > 1 ? needed.ToString() : "";
                }
            }
        }

        var completeButton = orderElements[order].Q<Button>("CompleteButton");
        if (completeButton != null)
        {
            completeButton.style.display = order.CanBeCompleted ? DisplayStyle.Flex : DisplayStyle.None;
            completeButton.style.scale = order.CanBeCompleted ? StyleKeyword.Null : new StyleScale(new Scale(Vector2.zero));
        }
    }

    private IEnumerator AnimateOrder(VisualElement customerFrame, VisualElement orderItems)
    {
        yield return new WaitForSeconds(0.1f);
        customerFrame.style.scale = new StyleScale(new Scale(Vector2.one));
        yield return new WaitForSeconds(1f);
        orderItems.style.scale = new StyleScale(new Scale(Vector2.one));
    }
}