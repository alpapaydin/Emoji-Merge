using UnityEngine;

public abstract class BaseItemProperties : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    public Sprite[] levelSprites;
    public int maxLevel;
}