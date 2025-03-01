using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private RandomLevelData levelData;

    void Start() 
    {
        GridManager.Instance.Initialize(levelData);
        OrderManager.Instance.Initialize(levelData);
    }
}
