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
                int energyToAdd = Mathf.FloorToInt(timeSinceLastRecharge / energyRechargeTime);
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyToAdd);
                lastEnergyRechargeTime = Time.time;
            }
        }
    }

    public bool HasEnoughEnergy(int amount)
    {
        return currentEnergy >= amount;
    }

    public void ConsumeEnergy(int amount)
    {
        if (HasEnoughEnergy(amount))
        {
            currentEnergy -= amount;
        }
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
    }
}
