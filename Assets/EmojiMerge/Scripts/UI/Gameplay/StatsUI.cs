using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float animationSpeed = 50f;
    
    private Label goldLabel;
    private Label levelProgress;
    private ProgressBar energyProgress;
    private VisualElement energyTitle;
    private VisualElement energyWarning;
    
    private float displayedEnergy, targetEnergy;
    private float displayedGold, targetGold;
    private Coroutine goldAnimation;
    private Coroutine energyAnimation;
    
    private readonly StyleScale normalScale = new StyleScale(new Scale(Vector2.one));
    private readonly StyleScale punchScale = new StyleScale(new Scale(Vector2.one * 1.2f));
    private readonly StyleColor normalColor = new StyleColor(Color.white);
    private readonly StyleColor increaseColor = new StyleColor(Color.green);
    private readonly StyleColor decreaseColor = new StyleColor(Color.red);

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Start play from main menu!");
            return;
        }
        var root = uiDocument.rootVisualElement;
        goldLabel = root.Q<Label>("GoldLabel");
        energyProgress = root.Q<ProgressBar>("EnergyProgress");
        energyTitle = energyProgress?.Q<VisualElement>("unity-title");
        energyWarning = root.Q<VisualElement>("EnergyWarning");
        levelProgress = root.Q<Label>("LevelProgress");
        
        if (goldLabel == null || energyProgress == null)
        {
            Debug.LogError($"UI elements not found in {gameObject.name}");
            return;
        }

        ResetStyles();
        GameManager.Instance.UpdateUIEnergy();
        GameManager.Instance.UpdateUICoins();
    }

    private void ResetStyles()
    {
        goldLabel.style.scale = normalScale;
        goldLabel.style.color = normalColor;
        if (energyTitle != null)
        {
            energyTitle.style.scale = normalScale;
            energyTitle.style.color = normalColor;
        }
    }

    public void UpdateLevelProgress(int current, int total)
    {
        levelProgress.text = $"{current} / {total}";
    }

    public void UpdateGold(int value)
    {
        targetGold = value;
        if (goldAnimation == null)
        {
            goldAnimation = StartCoroutine(AnimateGold());
        }
    }

    public void UpdateEnergy(int value)
    {
        targetEnergy = value;
        if (energyAnimation == null)
        {
            energyAnimation = StartCoroutine(AnimateEnergy());
        }
    }

    public void ShowEnergyWarning()
    {
        if (energyWarning.style.opacity.value == 0f)
            StartCoroutine(AnimateEnergyWarning());
    }

    private IEnumerator AnimateEnergyWarning()
    {
        energyWarning.style.opacity = 1;
        yield return new WaitForSeconds(1f);
        energyWarning.style.opacity = 0;
    }

    private IEnumerator AnimateGold()
    {
        goldLabel.style.scale = punchScale;
        bool increasing = targetGold > displayedGold;
        goldLabel.style.color = increasing ? increaseColor : decreaseColor;

        while (Mathf.Abs(displayedGold - targetGold) > 0.01f)
        {
            float step = Time.deltaTime * animationSpeed;
            displayedGold = Mathf.MoveTowards(displayedGold, targetGold, step);
            goldLabel.text = Mathf.RoundToInt(displayedGold).ToString();
            
            bool newIncreasing = targetGold > displayedGold;
            if (newIncreasing != increasing)
            {
                increasing = newIncreasing;
                goldLabel.style.color = increasing ? increaseColor : decreaseColor;
            }
            
            yield return null;
        }

        displayedGold = targetGold;
        goldLabel.text = targetGold.ToString();
        goldLabel.style.scale = normalScale;
        goldLabel.style.color = normalColor;
        goldAnimation = null;
    }

    private IEnumerator AnimateEnergy()
    {
        if (energyTitle != null)
        {
            energyTitle.style.scale = punchScale;
        }

        while (Mathf.Abs(displayedEnergy - targetEnergy) > 0.01f)
        {
            float step = Time.deltaTime * animationSpeed;
            displayedEnergy = Mathf.MoveTowards(displayedEnergy, targetEnergy, step);
            
            bool increasing = targetEnergy > displayedEnergy;
            if (energyTitle != null)
            {
                energyTitle.style.color = increasing ? increaseColor : decreaseColor;
            }

            int roundedEnergy = Mathf.RoundToInt(displayedEnergy);
            energyProgress.title = $"{roundedEnergy} / 100";
            energyProgress.value = displayedEnergy;
            
            yield return null;
        }

        displayedEnergy = targetEnergy;
        if (energyTitle != null)
        {
            energyTitle.style.scale = normalScale;
            energyTitle.style.color = normalColor;
        }
        energyAnimation = null;
    }

    private void OnDestroy()
    {
        if (goldAnimation != null)
            StopCoroutine(goldAnimation);
        if (energyAnimation != null)
            StopCoroutine(energyAnimation);
    }
}