using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class OrdersUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualElement ordersContainer;
    [SerializeField] private VisualTreeAsset orderTemplate;
    [SerializeField] private VisualTreeAsset orderItemTemplate;
    [SerializeField] private OrderManager orderManager;

    private Dictionary<Order, VisualElement> orderElements = new Dictionary<Order, VisualElement>();

    private void Awake()
    {
        if (orderManager == null)
        {
            orderManager = FindObjectOfType<OrderManager>();
        }
        ordersContainer = uiDocument.rootVisualElement.Q<VisualElement>("OrdersPanel");

        orderManager.OnOrderAdded += AddOrder;
    }

    public void AddOrder(Order order)
    {
        if (orderElements.ContainsKey(order))
            return;

        var orderElement = orderTemplate.CloneTree();
        VisualElement customerFrame = orderElement.Q<VisualElement>("CustomerFrame");
        VisualElement orderItems = orderElement.Q<VisualElement>("OrderItems");
        customerFrame.style.backgroundImage = new StyleBackground(order.CustomerSprite);
        orderElement.Q<Button>("CompleteButton").clicked += () => orderManager.TryCompleteOrder(order);
        
        orderElements.Add(order, orderElement);
        ordersContainer.Add(orderElement);

        AddOrderItems(order, orderItems);

        StartCoroutine(AnimateOrder(customerFrame, orderItems));
    }

    private void AddOrderItems(Order order, VisualElement itemsContainer)
    {
        foreach (var orderItem in order.GetRemainingItems())
        {
            var orderItemElement = orderItemTemplate.CloneTree();
            VisualElement orderItemIcon = orderItemElement.Q<VisualElement>("ItemIcon");
            Label orderItemCount = orderItemElement.Q<Label>("ItemCount");
            orderItemCount.text = orderItem.Value.ToString();
            orderItemIcon.style.backgroundImage = new StyleBackground(orderItem.Key.itemDefinition.icon);
            itemsContainer.Add(orderItemElement);
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