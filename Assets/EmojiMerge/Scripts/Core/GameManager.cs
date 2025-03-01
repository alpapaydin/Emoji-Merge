using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState
    {
        MainMenu,
        Gameplay,
        GameOver
    }

    public event Action<int> OnGoldEarned;

    [SerializeField] private int levelIndex = 0;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyRechargeTime = 60f;
    
    private const string LEVEL_INDEX_KEY = "LevelIndex";
    private GameState gameState = GameState.MainMenu;
    private float currentEnergy;
    private int currentCoins;
    private float lastEnergyRechargeTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        levelManager.LoadLevel(levelIndex);
        InitializeGame();
    }

    public void NextLevel()
    {
        levelIndex++;
        SaveLevelIndex();
        StartGame();
    }

    public void ResetLevelIndex()
    {
        levelIndex = 0;
        SaveLevelIndex();
    }
    
    private void InitializeGame()
    {
        currentEnergy = maxEnergy;
        currentCoins = 0;
        lastEnergyRechargeTime = Time.time;
    }

    public void SetGameStarted()
    {
        gameState = GameState.Gameplay;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    private void Update()
    {
        if (gameState == GameState.Gameplay)
            UpdateEnergyRecharge();
    }

    public void WinGame()
    {
        gameState = GameState.GameOver;
        if (UIManager.Instance != null)
            UIManager.Instance.WinPopup.ShowWin();
    }

    public void LoseGame()
    {
        gameState = GameState.GameOver;
        print("Game Lost");
    }

    private void SaveLevelIndex()
    {
        PlayerPrefs.SetInt(LEVEL_INDEX_KEY, levelIndex);
        PlayerPrefs.Save();
    }

    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey(LEVEL_INDEX_KEY))
        {
            levelIndex = PlayerPrefs.GetInt(LEVEL_INDEX_KEY);
        }
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
        OnGoldEarned?.Invoke(amount);
    }

    public void UpdateUIEnergy()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateEnergy(Mathf.RoundToInt(currentEnergy));
    }

    public void UpdateUICoins()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateGold(currentCoins);
    }

    public void ShowEnergyWarning()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.ShowEnergyWarning();
    }

}
