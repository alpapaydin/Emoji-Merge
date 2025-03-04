using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [SerializeField] private RandomLevelData[] allLevels;
    private static RandomLevelData currentLevelData;

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < allLevels.Length)
        {
            RandomLevelData level = allLevels[levelIndex];
            currentLevelData = level;
        }
        else
        {
            GameManager.Instance.ResetLevelIndex();
            currentLevelData = allLevels[0];
        }
        
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Gameplay");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        InitializeLevel();
    }

    void InitializeLevel() 
    {
        if (GridManager.Instance != null && OrderManager.Instance != null)
        {
            GridManager.Instance.Initialize(currentLevelData);
            OrderManager.Instance.Initialize(currentLevelData);
            WinManager winManager = new GameObject("WinManager").AddComponent<WinManager>();
            winManager.InitializeWinCondition(currentLevelData);
            UIManager.Instance.WinCondition.ShowWinCondition(currentLevelData.winCondition);
        }
        else
        {
            Debug.LogError("Failed to find GridManager or OrderManager instances!");
        }
    }

    public static RandomLevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }

}
