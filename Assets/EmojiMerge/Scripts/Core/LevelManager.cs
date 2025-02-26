using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private void Start()
    {
        GridManager.Instance.OnGridInitialized += OnGridInitialized;
    }

    private void OnGridInitialized()
    {
        SpawnTestItems();
    }

    private void SpawnTestItems()
    {
        ItemManager.Instance.CreateProducedItem(new Vector2Int(2, 2), 1);
        ItemManager.Instance.CreateProducedItem(new Vector2Int(3, 2), 1);
        ItemManager.Instance.CreateProducedItem(new Vector2Int(4, 2), 1);
        ItemManager.Instance.CreateProducedItem(new Vector2Int(2, 3), 2);
        ItemManager.Instance.CreateProducedItem(new Vector2Int(3, 3), 2);
    }

}
