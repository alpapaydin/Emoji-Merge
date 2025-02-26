using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float updateDuration = 0.5f;
    
    private VisualElement rootElement;
    private Label goldLabel;
    private ProgressBar energyProgress;
    
    private int currentGold;
    private float currentEnergy;
    private float displayedEnergy;
    private int displayedGold;
    private Coroutine goldUpdateRoutine;
    private Coroutine energyUpdateRoutine;

    private int targetGold;
    private float targetEnergy;
    private bool isAnimatingGold = false;
    private bool isAnimatingEnergy = false;

    private void Awake()
    {
        rootElement = uiDocument.rootVisualElement.Q<VisualElement>("TopBar");
        goldLabel = rootElement.Q<Label>("GoldLabel");
        energyProgress = rootElement.Q<ProgressBar>("EnergyProgress");
        
        currentGold = displayedGold = 0;
        currentEnergy = displayedEnergy = 0;
    }

    public void UpdateGold(int newGoldValue)
    {
        targetGold = newGoldValue;
        currentGold = displayedGold;
        
        if (!isAnimatingGold)
        {
            if (goldUpdateRoutine != null)
            {
                StopCoroutine(goldUpdateRoutine);
            }
            goldUpdateRoutine = StartCoroutine(AnimateGoldUpdate());
        }
    }

    public void UpdateEnergy(int newEnergyValue)
    {
        targetEnergy = newEnergyValue;
        currentEnergy = displayedEnergy;
        
        if (!isAnimatingEnergy)
        {
            if (energyUpdateRoutine != null)
            {
                StopCoroutine(energyUpdateRoutine);
            }
            energyUpdateRoutine = StartCoroutine(AnimateEnergyUpdate());
        }
    }

    private IEnumerator AnimateGoldUpdate()
    {
        isAnimatingGold = true;
        float animationTimeLeft = updateDuration;
        
        while (displayedGold != targetGold)
        {
            int startGold = displayedGold;
            int difference = targetGold - startGold;
            bool isIncrease = difference > 0;
            
            if ((animationTimeLeft == updateDuration || 
                (isIncrease && displayedGold < currentGold) || 
                (!isIncrease && displayedGold > currentGold)))
            {
                goldLabel.style.scale = new StyleScale(new Scale(new Vector2(1.2f, 1.2f)));
                goldLabel.style.color = isIncrease ? new StyleColor(Color.green) : new StyleColor(Color.red);
            }

            float elapsed = 0f;
            while (elapsed < animationTimeLeft)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationTimeLeft;
                
                progress = progress * progress * (3f - 2f * progress);
                displayedGold = startGold + Mathf.RoundToInt(difference * progress);
                goldLabel.text = displayedGold.ToString();
                
                float scaleProgress = 1f + (0.2f * (1f - progress));
                goldLabel.style.scale = new StyleScale(new Scale(new Vector2(scaleProgress, scaleProgress)));
                
                if (displayedGold != targetGold && targetGold != startGold + difference)
                {
                    currentGold = displayedGold;
                    animationTimeLeft = updateDuration * (1f - progress);
                    break;
                }
                
                yield return null;
            }
            
            currentGold = targetGold;
            displayedGold = targetGold;
        }
        
        displayedGold = targetGold;
        currentGold = targetGold;
        goldLabel.text = targetGold.ToString();
        goldLabel.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f)));
        goldLabel.style.color = new StyleColor(Color.white);
        goldUpdateRoutine = null;
        isAnimatingGold = false;
    }

    private IEnumerator AnimateEnergyUpdate()
    {
        isAnimatingEnergy = true;
        float animationTimeLeft = updateDuration;
        var energyTitle = energyProgress.Q<VisualElement>("unity-title");
        
        while (Mathf.Abs(displayedEnergy - targetEnergy) > 0.01f)
        {
            float startEnergy = displayedEnergy;
            float difference = targetEnergy - startEnergy;
            bool isIncrease = difference > 0;
            
            if ((animationTimeLeft == updateDuration || 
                (isIncrease && displayedEnergy < currentEnergy) || 
                (!isIncrease && displayedEnergy > currentEnergy)) 
                && energyTitle != null)
            {
                energyTitle.style.scale = new StyleScale(new Scale(new Vector2(1.2f, 1.2f)));
                energyTitle.style.color = isIncrease ? new StyleColor(Color.green) : new StyleColor(Color.red);
            }

            float elapsed = 0f;
            while (elapsed < animationTimeLeft)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationTimeLeft;
                
                progress = progress * progress * (3f - 2f * progress);
                displayedEnergy = startEnergy + (difference * progress);
                energyProgress.title = Mathf.RoundToInt(displayedEnergy).ToString() + " / 100";
                energyProgress.value = displayedEnergy;
                
                if (energyTitle != null)
                {
                    float scaleProgress = 1f + (0.2f * (1f - progress));
                    energyTitle.style.scale = new StyleScale(new Scale(new Vector2(scaleProgress, scaleProgress)));
                }
                
                if (Mathf.Abs(displayedEnergy - targetEnergy) > 0.01f && 
                    Mathf.Abs(targetEnergy - (startEnergy + difference)) > 0.01f)
                {
                    currentEnergy = displayedEnergy;
                    animationTimeLeft = updateDuration * (1f - progress);
                    break;
                }
                
                yield return null;
            }
            
            currentEnergy = targetEnergy;
            displayedEnergy = targetEnergy;
        }
        
        displayedEnergy = targetEnergy;
        currentEnergy = targetEnergy;
        energyProgress.title = Mathf.RoundToInt(targetEnergy).ToString() + " / 100";
        energyProgress.value = targetEnergy;
        if (energyTitle != null)
        {
            energyTitle.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f)));
            energyTitle.style.color = new StyleColor(Color.white);
        }
        energyUpdateRoutine = null;
        isAnimatingEnergy = false;
    }
}