using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyRechargeTime = 60f;
    [SerializeField] private float currentEnergy;
    [SerializeField] private int currentCoins;

    private float lastEnergyRechargeTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        currentEnergy = maxEnergy;
        currentCoins = 0;
        lastEnergyRechargeTime = Time.time;
    }

    private void Update()
    {
        UpdateEnergyRecharge();
    }

    private void UpdateEnergyRecharge()
    {
        if (currentEnergy < maxEnergy)
        {
            float timeSinceLastRecharge = Time.time - lastEnergyRechargeTime;
            if (timeSinceLastRecharge >= energyRechargeTime)
            {
                float energyToAdd = timeSinceLastRecharge / energyRechargeTime;
                AddEnergy(energyToAdd);
                lastEnergyRechargeTime = Time.time;
            }
        }
    }

    public bool HasEnoughEnergy(float amount)
    {
        bool hasEnough = currentEnergy >= amount;
        if (!hasEnough)
            ShowEnergyWarning();
        return hasEnough;
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy = Mathf.Max(0f, currentEnergy - amount);
        UpdateUIEnergy();
    }

    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        UpdateUIEnergy();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateUICoins();
    }

    public void UpdateUIEnergy()
    {
        UIManager.Instance.UpdateEnergy(Mathf.RoundToInt(currentEnergy));
    }

    public void UpdateUICoins()
    {
        UIManager.Instance.UpdateGold(currentCoins);
    }

    public void ShowEnergyWarning()
    {
        UIManager.Instance.ShowEnergyWarning();
    }
}
