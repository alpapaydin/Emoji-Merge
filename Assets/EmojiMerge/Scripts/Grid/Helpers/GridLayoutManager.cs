using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridLayoutManager
{
    public static void PositionAndScaleGrid(Grid grid, Vector2Int gridSize, float padding = 0.5f)
    {
        if (grid == null || Camera.main == null) return;

        Vector3 firstCellPos = grid.GetCellCenterLocal(new Vector3Int(0, 0, 0));
        Vector3 lastCellPos = grid.GetCellCenterLocal(new Vector3Int(gridSize.x - 1, gridSize.y - 1, 0));
        
        float rawWidth = Mathf.Abs(lastCellPos.x - firstCellPos.x);
        float rawHeight = Mathf.Abs(lastCellPos.y - firstCellPos.y);

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;
        
        float scaleX = (screenWidth - padding * 2) / rawWidth;
        float scaleY = (screenHeight - padding * 2) / rawHeight;
        float scale = Mathf.Min(scaleX, scaleY);
        
        grid.transform.localScale = new Vector3(scale, scale, 1f);

        float scaledWidth = rawWidth * scale;
        float scaledHeight = rawHeight * scale;
        
        Vector3 centerPosition = new Vector3(
            -scaledWidth / 2f - firstCellPos.x * scale,
            -scaledHeight / 2f - firstCellPos.y * scale,
            0f
        );
        
        grid.transform.position = centerPosition;
    }
}
