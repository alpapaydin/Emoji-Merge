using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridItem : MonoBehaviour
{
    public BaseItemProperties properties;
    protected Cell occupiedCell;
    protected Vector2Int gridPosition;
    
    protected int currentLevel = 1;
    protected bool isReadyToMerge = true;
    protected SpriteRenderer spriteRenderer;

    public int CurrentLevel => currentLevel;
    public bool IsReadyToMerge => isReadyToMerge;
    public Cell OccupiedCell => occupiedCell;
    public Vector2Int GridPosition => gridPosition;
    public bool CanMerge(GridItem other) => 
        other != null && 
        other.properties.itemType == properties.itemType && 
        other.currentLevel == currentLevel && 
        currentLevel < properties.maxLevel;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer component missing on GridItem {gameObject.name}");
        }
    }

    protected virtual void Start()
    {
        UpdateVisuals();
    }

    public virtual void Initialize(BaseItemProperties props, int level = 1)
    {
        properties = props;
        currentLevel = Mathf.Clamp(level, 1, props.maxLevel);
        UpdateVisuals();
    }

    public virtual void SetGridPosition(Vector2Int position, Cell cell)
    {
        gridPosition = position;
        occupiedCell = cell;
        if (cell != null)
        {
            transform.position = cell.transform.position;
        }
    }

    protected virtual void OnDestroy()
    {
        if (occupiedCell != null)
        {
            GridManager.Instance?.ClearCell(gridPosition);
        }
    }

    public virtual void OnTapped() { }

    public virtual bool CanPerformAction()
    {
        return true;
    }

    protected virtual void UpdateVisuals()
    {
        if (spriteRenderer && properties != null && properties.levelSprites != null && 
            currentLevel <= properties.levelSprites.Length)
        {
            spriteRenderer.sprite = properties.levelSprites[currentLevel - 1];
        }
    }

    public virtual GridItem MergeWith(GridItem other)
    {
        if (!CanMerge(other))
            return null;

        // merge item logic
        int newLevel = currentLevel + 1;
        return ItemManager.Instance.CreateMergedItem(gridPosition, properties.itemType, newLevel);        
    }

    public Sprite GetNextLevelSprite()
    {
        if (properties != null && properties.levelSprites != null &&
            currentLevel < properties.maxLevel)
        {
            return properties.levelSprites[currentLevel];
        }
        return null;
    }

    public virtual GridItem MergeWith(GridItem other, Vector2Int mergePosition)
    {
        if (!CanMerge(other))
            return null;

        ShowParticleEffect("merge");
        return ItemManager.Instance.CreateMergedItem(mergePosition, properties.itemType, currentLevel + 1);
    }

    protected void ShowParticleEffect(string effectType)
    {
        // particle spawn logic for "spawn", "merge", "ready"
    }
}
