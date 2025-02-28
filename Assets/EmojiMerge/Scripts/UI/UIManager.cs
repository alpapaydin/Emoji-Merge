using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ItemDetailsUI itemDetails;
    [SerializeField] private StatsUI stats;

    public ItemDetailsUI ItemDetails => itemDetails;
    
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OpenItemDetailsPane(GridItem item)
    {
        itemDetails.ShowDetails(item);
    }

    public void CloseItemDetailsPane()
    {
        itemDetails.Hide();
    }

    public void UpdateGold(int newGoldValue)
    {
        stats.UpdateGold(newGoldValue);
    }

    public void UpdateEnergy(int newEnergyValue)
    {
        stats.UpdateEnergy(newEnergyValue);
    }
}
