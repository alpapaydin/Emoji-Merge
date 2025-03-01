using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WinLosePopup : MonoBehaviour
{
    [SerializeField] private UIDocument winConditionDocument;

    private VisualElement background;
    private Label winConditionText;
    private VisualElement popupBox;
    private Button controlButton;

    private void Awake()
    {
        background = winConditionDocument.rootVisualElement.Q<VisualElement>("Background");
        winConditionText = winConditionDocument.rootVisualElement.Q<Label>("WinText");
        popupBox = winConditionDocument.rootVisualElement.Q<VisualElement>("PopupBox");
        controlButton = winConditionDocument.rootVisualElement.Q<Button>("Control");
    }

    public void ShowWin()
    {
        ShowWinPopup("You Win!");
        controlButton.text = "Next Level";
        controlButton.clicked += () => { GameManager.Instance.NextLevel(); };
    }

    public void ShowLose()
    {
        ShowWinPopup("You Lose!");
        controlButton.text = "Retry";
        controlButton.clicked += () => { GameManager.Instance.StartGame(); };
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
    }
}
