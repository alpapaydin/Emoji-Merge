using UnityEngine;
using UnityEngine.UIElements;

public class ItemDetailsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement rootElement;

    //Fields
    private Label itemName;
    private Label itemLevel;
    private VisualElement itemIcon;
    private VisualElement containedItems;
    private VisualElement progressBar;

    private VisualElement itemExtras;
    private VisualElement itemContainer;
    private ProgressBar itemProgress;

    private void Awake()
    {
        rootElement = uiDocument.rootVisualElement.Q<VisualElement>("BottomBar");
        itemName = rootElement.Q<Label>("ItemName");
        itemLevel = rootElement.Q<Label>("ItemLevel");
        itemIcon = rootElement.Q<VisualElement>("ItemIcon");

        itemExtras = rootElement.Q<VisualElement>("Extras");
        itemContainer = rootElement.Q<VisualElement>("ContainedItems");
        itemProgress = rootElement.Q<ProgressBar>("ProgressBar");
    }

    public void ShowDetails(GridItem item)
    {
        itemName.text = item.properties.itemName;
        itemLevel.text = $"Level: {item.CurrentLevel}";
        itemIcon.style.backgroundImage = 
            new StyleBackground(item.properties.levelSprites[item.CurrentLevel - 1]);
        rootElement.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f)));

        if (item is ContainerItem containerItem)
        {    
            ShowContainerStats(containerItem);
            itemExtras.style.display = DisplayStyle.Flex;
        } else
        {
            itemExtras.style.display = DisplayStyle.None;
        }

    }

    public void Hide()
    {
        rootElement.style.scale = new StyleScale(new Scale(new Vector2(1f, 0f)));
    }

    private void ShowContainerStats(ContainerItem item) 
    {
        // show contained items and progress bar
        VisualElement itemContainer = rootElement.Q<VisualElement>("ContainedItems");
    }
}