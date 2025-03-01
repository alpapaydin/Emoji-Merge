using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class WinConditionUI : MonoBehaviour
{
    [SerializeField] private UIDocument winConditionDocument;

    private VisualElement background;
    private Label winConditionText;
    private VisualElement popupBox;

    private void Awake()
    {
        background = winConditionDocument.rootVisualElement.Q<VisualElement>("Background");
        winConditionText = winConditionDocument.rootVisualElement.Q<Label>("WinText");
        popupBox = winConditionDocument.rootVisualElement.Q<VisualElement>("PopupBox");
    }

    public void ShowWinCondition(BaseWinConditionData winCondition)
    {
        if (winCondition is GetGoldWinCondition goldWinCondition)
        {
            ShowWinPopup($"Get {goldWinCondition.goldNeeded} gold to win!");
        }
        else if (winCondition is CompleteOrdersWinCondition ordersWinCondition)
        {
            ShowWinPopup($"Complete {ordersWinCondition.ordersToComplete} orders to win!");
        }
    }

    private void ShowWinPopup(string text)
    {
        winConditionText.text = text;
        background.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        popupBox.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        StartCoroutine(AnimateWinPopup());
    }

    private IEnumerator AnimateWinPopup()
    {
        yield return new WaitForSeconds(0.5f);
        background.style.opacity = 1;
        yield return new WaitForSeconds(0.3f);
        popupBox.style.scale = new StyleScale(new Scale(Vector2.one));
        yield return new WaitForSeconds(1.5f);
        popupBox.style.scale = new StyleScale(new Scale(Vector2.one));
        yield return new WaitForSeconds(0.5f);
        background.style.opacity = 0;
        yield return new WaitForSeconds(0.5f);
        background.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        GameManager.Instance.SetGameStarted();
    }
}
