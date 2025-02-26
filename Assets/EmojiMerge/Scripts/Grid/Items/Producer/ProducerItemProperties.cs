using UnityEngine;

[CreateAssetMenu(fileName = "ProducerItemProperties", menuName = "Game/Items/Producer Properties")]
public class ProducerItemProperties : BaseItemProperties
{
    public int energyCost;
    public ProducerLevel[] levels = new ProducerLevel[5];
    
    private void OnEnable()
    {
        itemType = ItemType.Producer;
        maxLevel = 5;
        InitializeDefaultLevels();
    }

    private void OnValidate()
    {
        if (levels == null || levels.Length != maxLevel)
        {
            levels = new ProducerLevel[maxLevel];
            InitializeDefaultLevels();
        }
    }

    private void InitializeDefaultLevels()
    {
        for (int i = 0; i < 3; i++)
        {
            levels[i] = new ProducerLevel 
            { 
                canProduce = false,
                itemCapacities = new ProducerItemCapacity[0]
            };
        }

        levels[3] = new ProducerLevel
        {
            canProduce = true,
            rechargeTime = 10f,
            itemCapacities = new ProducerItemCapacity[]
            {
                new ProducerItemCapacity { level = 1, count = 10 }
            }
        };

        levels[4] = new ProducerLevel
        {
            canProduce = true,
            rechargeTime = 20f,
            itemCapacities = new ProducerItemCapacity[]
            {
                new ProducerItemCapacity { level = 1, count = 10 },
                new ProducerItemCapacity { level = 2, count = 10 }
            }
        };
    }

    public ProducerLevel GetLevelData(int level)
    {
        return levels[Mathf.Clamp(level - 1, 0, levels.Length - 1)];
    }
}