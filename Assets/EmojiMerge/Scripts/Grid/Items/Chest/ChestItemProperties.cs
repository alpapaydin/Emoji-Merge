using UnityEngine;

[CreateAssetMenu(fileName = "ChestItemProperties", menuName = "Game/Items/Chest Properties")]
public class ChestItemProperties : BaseItemProperties
{    
    public ChestLevel[] levels = new ChestLevel[3];

    private void OnEnable()
    {
        itemType = ItemType.Chest;
        maxLevel = 3;
        InitializeDefaultLevels();
    }

    private void OnValidate()
    {
        if (levels == null || levels.Length != maxLevel)
        {
            levels = new ChestLevel[maxLevel];
            InitializeDefaultLevels();
        }
    }

    private void InitializeDefaultLevels()
    {
        levels[0] = new ChestLevel
        {
            unlockTime = 60f,
            resourceDrops = new ChestResourceDrop[]
            {
                new ChestResourceDrop { resourceType = ItemType.Energy, level = 1, count = 2 },
                new ChestResourceDrop { resourceType = ItemType.Coin, level = 1, count = 1 }
            }
        };

        levels[1] = new ChestLevel
        {
            unlockTime = 60f,
            resourceDrops = new ChestResourceDrop[]
            {
                new ChestResourceDrop { resourceType = ItemType.Energy, level = 2, count = 3 },
                new ChestResourceDrop { resourceType = ItemType.Coin, level = 2, count = 2 }
            }
        };

        levels[2] = new ChestLevel
        {
            unlockTime = 60f,
            resourceDrops = new ChestResourceDrop[]
            {
                new ChestResourceDrop { resourceType = ItemType.Energy, level = 3, count = 5 },
                new ChestResourceDrop { resourceType = ItemType.Coin, level = 3, count = 3 }
            }
        };
    }

    public ChestLevel GetLevelData(int level)
    {
        return levels[Mathf.Clamp(level - 1, 0, levels.Length - 1)];
    }
}