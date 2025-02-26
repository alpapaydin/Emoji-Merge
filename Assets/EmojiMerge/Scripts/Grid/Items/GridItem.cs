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
    private bool isBeingMerged = false;

    public int CurrentLevel => currentLevel;
    public bool IsReadyToMerge => isReadyToMerge;
    public Cell OccupiedCell => occupiedCell;
    public Vector2Int GridPosition => gridPosition;
    public bool CanMerge(GridItem other) => MergeManager.Instance.CanMergeItems(this, other);

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

    public void SetMergeState(bool isMerging)
    {
        isBeingMerged = isMerging;
    }

    protected virtual void OnDestroy()
    {
        if (occupiedCell != null && !isBeingMerged)
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
        SetMergeState(true);
        if (other != null)
        {
            other.SetMergeState(true);
        }
        
        ShowParticleEffect("merge");
        return MergeManager.Instance.PerformMerge(this, other, mergePosition);
    }

    protected void ShowParticleEffect(string effectType)
    {
        // particle spawn logic for "spawn", "merge", "ready"
    }
}
