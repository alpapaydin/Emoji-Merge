using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridLayoutManager
{
    public static void PositionAndScaleGridWithMargins(Grid grid, Vector2Int gridSize, float leftMargin, float rightMargin, float topMargin, float bottomMargin)
    {
        if (grid == null || Camera.main == null) return;

        Vector3 firstCellPos = grid.GetCellCenterLocal(new Vector3Int(0, 0, 0));
        Vector3 lastCellPos = grid.GetCellCenterLocal(new Vector3Int(gridSize.x - 1, gridSize.y - 1, 0));
        
        float rawWidth = Mathf.Abs(lastCellPos.x - firstCellPos.x);
        float rawHeight = Mathf.Abs(lastCellPos.y - firstCellPos.y);

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;
        
        float horizontalMargins = leftMargin + rightMargin;
        float verticalMargins = topMargin + bottomMargin;
        
        float scaleX = (screenWidth - horizontalMargins) / rawWidth;
        float scaleY = (screenHeight - verticalMargins) / rawHeight;
        float scale = Mathf.Min(scaleX, scaleY);
        
        grid.transform.localScale = new Vector3(scale, scale, 1f);

        float scaledWidth = rawWidth * scale;
        float scaledHeight = rawHeight * scale;
        
        float horizontalOffset = (rightMargin - leftMargin) / 2f;
        float verticalOffset = (bottomMargin - topMargin) / 2f;
        
        Vector3 centerPosition = new Vector3(
            -scaledWidth / 2f - firstCellPos.x * scale + horizontalOffset,
            -scaledHeight / 2f - firstCellPos.y * scale + verticalOffset,
            0f
        );
        
        grid.transform.position = centerPosition;
    }
    
    public static void PositionAndScaleGridWithPercentageMargins(Grid grid, Vector2Int gridSize, 
                float leftPercent = 0.05f, float rightPercent = 0.05f, float topPercent = 0.05f, float bottomPercent = 0.05f)
    {
        if (grid == null || Camera.main == null) return;
        
        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;
        
        float leftMargin = screenWidth * leftPercent;
        float rightMargin = screenWidth * rightPercent;
        float topMargin = screenHeight * topPercent;
        float bottomMargin = screenHeight * bottomPercent;
        
        float availableWidth = screenWidth - (leftMargin + rightMargin);
        float availableHeight = screenHeight - (topMargin + bottomMargin);
        
        Vector3 firstCellPos = grid.GetCellCenterLocal(new Vector3Int(0, 0, 0));
        Vector3 lastCellPos = grid.GetCellCenterLocal(new Vector3Int(gridSize.x - 1, gridSize.y - 1, 0));
        
        float rawWidth = Mathf.Abs(lastCellPos.x - firstCellPos.x);
        float rawHeight = Mathf.Abs(lastCellPos.y - firstCellPos.y);
        
        float scaleX = availableWidth / rawWidth;
        float scaleY = availableHeight / rawHeight;
        float scale = Mathf.Min(scaleX, scaleY);
        
        grid.transform.localScale = new Vector3(scale, scale, 1f);
        
        float scaledWidth = rawWidth * scale;
        float scaledHeight = rawHeight * scale;
        
        float horizontalOffset = (rightMargin - leftMargin) / 2f;
        
        float verticalOffset = (bottomMargin - topMargin) / 2f;
        
        Vector3 centerPosition = new Vector3(
            -scaledWidth / 2f - firstCellPos.x * scale + horizontalOffset,
            -scaledHeight / 2f - firstCellPos.y * scale + verticalOffset,
            0f
        );
        
        grid.transform.position = centerPosition;
    }
}
