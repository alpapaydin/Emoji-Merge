using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ItemDetailsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset inventoryItemDocument;
    
    private VisualElement rootElement;
    private Label itemName;
    private Label itemLevel;
    private VisualElement itemIcon;
    private VisualElement itemExtras;
    private VisualElement itemContainer;
    private ProgressBar itemProgress;

    private ContainerItem currentContainer;
    private Coroutine progressTextAnimation;
    private string[] progressDots = new string[] { "", ".", "..", "..." };
    private int currentDotIndex;

    private void Awake()
    {
        rootElement = uiDocument.rootVisualElement.Q<VisualElement>("BottomBar");
        if (rootElement == null)
        {
            Debug.LogError($"BottomBar not found in {gameObject.name}");
            return;
        }

        itemName = rootElement.Q<Label>("ItemName");
        itemLevel = rootElement.Q<Label>("ItemLevel");
        itemIcon = rootElement.Q<VisualElement>("ItemIcon");
        itemExtras = rootElement.Q<VisualElement>("Extras");
        
        if (itemExtras != null)
        {
            itemContainer = itemExtras.Q<VisualElement>("ContainedItems");
            itemProgress = itemExtras.Q<ProgressBar>("ItemProgress");
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromCurrentContainer();
    }

    public void ShowDetails(GridItem item)
    {
        UnsubscribeFromCurrentContainer();
        
        itemName.text = item.properties.itemName;
        itemLevel.text = $"Level: {item.CurrentLevel}";
        itemIcon.style.backgroundImage = 
            new StyleBackground(item.properties.levelSprites[item.CurrentLevel - 1]);
        rootElement.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f)));

        if (item is ContainerItem containerItem)
        {    
            currentContainer = containerItem;
            currentContainer.OnContainerStateChanged += UpdateContainerStats;
            ShowContainerStats(containerItem);
            itemExtras.style.display = DisplayStyle.Flex;
            StartProgressAnimation();
        }
        else
        {
            currentContainer = null;
            itemExtras.style.display = DisplayStyle.None;
            StopProgressAnimation();
        }
    }

    private void UnsubscribeFromCurrentContainer()
    {
        if (currentContainer != null)
        {
            currentContainer.OnContainerStateChanged -= UpdateContainerStats;
            currentContainer = null;
        }
    }

    public void Hide()
    {
        UnsubscribeFromCurrentContainer();
        StopProgressAnimation();
        rootElement.style.scale = new StyleScale(new Scale(new Vector2(1f, 0f)));
    }

    private void UpdateContainerStats()
    {
        if (currentContainer != null)
        {
            ShowContainerStats(currentContainer);
        }
    }

    private void ShowContainerStats(ContainerItem item)
    {
        if (item is ChestItem chest)
        {
            itemProgress.value = chest.UnlockProgress * 100;
            UpdateProgressText(chest.IsUnlocking ? "Unlocking" : chest.IsLocked ? "Locked" : "Finished!");
        }
        else
        {
            itemProgress.value = item.RechargeProgress * 100;
            UpdateProgressText(item.IsRecharging ? "Producing" : "Finished!");
        }

        itemContainer.Clear();
        foreach (var (properties, level, count) in item.GetContainedItems())
        {
            var itemElement = inventoryItemDocument.CloneTree();
            itemElement.Q<Label>("ItemCount").text = count.ToString();
            itemElement.Q<VisualElement>("ItemIcon").style.backgroundImage = 
                new StyleBackground(properties.levelSprites[level - 1]);
            itemContainer.Add(itemElement);
        }
    }

    private void UpdateProgressText(string baseText)
    {
        if (string.IsNullOrEmpty(baseText)) return;
        
        if (baseText == "Finished!")
        {
            itemProgress.title = baseText;
        }
        else
        {
            itemProgress.title = baseText + progressDots[currentDotIndex];
        }
    }

    private void StartProgressAnimation()
    {
        StopProgressAnimation();
        progressTextAnimation = StartCoroutine(AnimateProgressText());
    }

    private void StopProgressAnimation()
    {
        if (progressTextAnimation != null)
        {
            StopCoroutine(progressTextAnimation);
            progressTextAnimation = null;
        }
    }

    private IEnumerator AnimateProgressText()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            currentDotIndex = (currentDotIndex + 1) % progressDots.Length;
            if (currentContainer != null)
            {
                UpdateContainerStats();
            }
            yield return wait;
        }
    }
}