using UnityEngine;
using UnityEngine.UIElements;

public class ItemDetailsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement rootElement;

    private void Awake()
    {
        rootElement = uiDocument.rootVisualElement.Q<VisualElement>("BottomBar");
    }

    public void ShowDetails(GridItem item)
    {
        rootElement.Q<Label>("ItemName").text = item.properties.itemName;
        rootElement.Q<Label>("ItemLevel").text = $"Level: {item.CurrentLevel}";
        rootElement.Q<VisualElement>("ItemIcon").style.backgroundImage = 
            new StyleBackground(item.properties.levelSprites[item.CurrentLevel - 1]);
        rootElement.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f)));
    }

    public void Hide()
    {
        rootElement.style.scale = new StyleScale(new Scale(new Vector2(1f, 0f)));
    }
}