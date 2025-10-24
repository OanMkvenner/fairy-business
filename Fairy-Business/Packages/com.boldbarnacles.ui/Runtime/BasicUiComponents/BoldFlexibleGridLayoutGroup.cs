/* FlexibleGridLayout.cs
 * From: Game Dev Guide - Fixing Grid Layouts in Unity With a Flexible Grid Component
 * Created: June 2020, NowWeWake
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : BoldLayoutGroup
{    
    public enum FitType
    {
        FLEXIBLEFILL,
        FIXEDROWS,
        FIXEDCOLUMNS
    }
	public enum Axis { Horizontal = 0, Vertical = 1 }

    public Vector2 spacing;

    [Header("Flexible Grid")]
    public FitType fitType = FitType.FLEXIBLEFILL;

    [ShowIf("@fitType == FitType.FIXEDROWS")]
    public int rows;
    [ShowIf("@fitType == FitType.FIXEDCOLUMNS")]
    public int columns;

    public bool stretchFill = true;
    [HideIf("stretchFill")]
    public bool centered = true;
    [ShowIf("stretchFill")]
    public bool OverstretchLastRowOrColumn = true;

    [HorizontalGroup("fitLayoutSizeToContent", LabelWidth = 120)]
    public bool fitLayoutSizeToContent = false;
    [ShowIf("fitLayoutSizeToContent")]
    [HorizontalGroup("fitLayoutSizeToContent", LabelWidth = 60)]
    [LabelText("Grid Ratio w/h")]
    public float desiredGridRatio = 1.0f;

    [HorizontalGroup("ContentAspectRatio", LabelWidth = 120)]
    [ShowIf("@!stretchFill")]
    public bool setContentAspectRatio = false;
    [ShowIf("@!stretchFill && setContentAspectRatio")]
    [HorizontalGroup("ContentAspectRatio", LabelWidth = 60)]
    [LabelText("Ratio w/h")]
    public float contentAspectRatioOverride = 1.0f;

    public Axis startAxis = Axis.Horizontal;


    public float2 GetMinWidthAndHeight(){
        return minWidthHeight;
    }

    float2 minWidthHeight = new float2(1,1);

    public struct GridItemPlacement
    {
        public float2 position;
        public float2 size;
    }

    public List<GridItemPlacement> CalculateGridPlacements(int amountOfItems, float2? contentSize = null, bool positionValuesAxisOriented = false){
        List<GridItemPlacement> resultList = new();
        if (amountOfItems == 0) return resultList;

        float parentWidth = rectTransform.rect.width - padding.left - padding.right;
        float parentHeight = rectTransform.rect.height - padding.top - padding.bottom;

        // This spaces fields out in a way, where each content item has enough space to fulfill its height-width ratio properly within the field
        // value > 0 == higherThanWide, value < 0 => wider than high
        float contentAspectRatio = 1.0f;
        if (contentSize.HasValue){
            if (contentSize.Value.y != 0 && contentSize.Value.x != 0){
                contentAspectRatio = contentSize.Value.x / contentSize.Value.y;
            } else {
                Debug.LogError($"invalid contentsize values: {contentSize.Value}");
            }
        }
        if (setContentAspectRatio) {
            contentAspectRatio = contentAspectRatioOverride;
        }

        // find optimal columnCount
        var desiredRowColumnRatio = parentHeight / parentWidth;

        bool layoutFittingPossible = false;
        LayoutElement layoutElement = null;
        if (fitLayoutSizeToContent){
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null) {
                Debug.LogError("fitLayoutSizeToContent requires LayoutElement on the Gameobject (and any layout as parent that uses its PreferredWidth/Height values)");
            }
            if (!contentSize.HasValue){
                Debug.LogError("fitLayoutSizeToContent requires to set contentSize parameter when calling CalculateGridPlacement(). Otherwise we dont know how big each element should be when fitting the grid!");
            }
            
            if (layoutElement != null && contentSize.HasValue)
            {
                desiredRowColumnRatio = 1.0f / desiredGridRatio;
                layoutFittingPossible = true;
            }
        }

        if (!stretchFill) desiredRowColumnRatio *= contentAspectRatio;
        var columnCount = 0;
        var rowCount = 1;
        var lastCheckedDistance = 9999999f;
        var checkColumn = 0;
        var checkRow = 0;
        if (fitType == FitType.FIXEDROWS || fitType == FitType.FIXEDCOLUMNS){
            if (fitType == FitType.FIXEDROWS) {
                rowCount = rows;
                columnCount = Mathf.CeilToInt((float)amountOfItems / (float)rowCount);
            }
            if (fitType == FitType.FIXEDCOLUMNS) {
                columnCount = columns;
                rowCount = Mathf.CeilToInt((float)amountOfItems / (float)columnCount);
            }
        } else {
            // find optimal row/column set
            if (startAxis == Axis.Horizontal){
                while (true)
                {
                    // iterate up column count
                    // divide childcount by column count, round up new number (this is row-count)
                    // divide row-count by column count, once this gets lower than parentWidth / parentHeight, we have the right one. then continue
                    checkColumn++;
                    checkRow = Mathf.CeilToInt((float)amountOfItems / (float)checkColumn);
                    if (checkRow == 0) checkRow = 1;

                    var currentRowColumnRatio = (float)checkRow / (float)checkColumn;
                    var columnRatioDistance = Mathf.Abs(currentRowColumnRatio - desiredRowColumnRatio);
                    if (lastCheckedDistance > columnRatioDistance)
                    {
                        columnCount = checkColumn;
                        rowCount = checkRow;
                        lastCheckedDistance = columnRatioDistance;
                    }
                    if (checkColumn >= amountOfItems) break;
                }
            } else {
                while (true)
                {
                    // similar as above but reversed for rows
                    checkRow++;
                    checkColumn = Mathf.CeilToInt((float)amountOfItems / (float)checkRow);
                    if (checkColumn == 0) checkColumn = 1;

                    var currentRowColumnRatio = (float)checkRow / (float)checkColumn;
                    var columnRatioDistance = Mathf.Abs(currentRowColumnRatio - desiredRowColumnRatio);
                    if (lastCheckedDistance > columnRatioDistance)
                    {
                        columnCount = checkColumn;
                        rowCount = checkRow;
                        lastCheckedDistance = columnRatioDistance;
                    }
                    if (checkRow >= amountOfItems) break;
                }
            }
        }

        if (columnCount == 0) columnCount = 1;
        if (amountOfItems == 1) {
            columnCount = 1;
            rowCount = 1;
        }
        if (startAxis == Axis.Horizontal && amountOfItems % columnCount > 0){
            var fillableSlots = columnCount - amountOfItems % columnCount;
            var reduction = Mathf.CeilToInt(fillableSlots / rowCount);
            columnCount -= reduction;
        }
        if (startAxis == Axis.Vertical && amountOfItems % rowCount > 0){
            var fillableSlots = rowCount - amountOfItems % rowCount;
            var reduction = Mathf.CeilToInt(fillableSlots / columnCount);
            rowCount -= reduction;
        }

        // calculate cellsizes
        var cellWidth = ((parentWidth + spacing.x) / (float)columnCount) - spacing.x;
        var cellHeight = ((parentHeight + spacing.y) / (float)rowCount) - spacing.y;
        if (layoutFittingPossible){
            cellWidth = contentSize.Value.x;
            cellHeight = contentSize.Value.y;
            parentWidth = columnCount * (cellWidth + spacing.x) - spacing.x;
            parentHeight = rowCount * (cellHeight + spacing.y) - spacing.y;
            layoutElement.preferredWidth = parentWidth + padding.left + padding.right;
            layoutElement.preferredHeight = parentHeight + padding.top + padding.bottom;
        }

        if (!stretchFill){
            // try to find the smallest side while trying to keep the desired height-widht ratio (usually a 1:1 square)
            if (cellWidth / contentAspectRatio > cellHeight) cellWidth = cellHeight * contentAspectRatio;
            else cellHeight = cellWidth / contentAspectRatio;
        }

        minWidthHeight = new float2(cellWidth, cellHeight);

        float lastRowOrColumnWidth = cellWidth;
        float lastRowOrColumnHeight = cellHeight;
        bool lastRowIsNotFull = amountOfItems % columnCount != 0;
        bool lastColumnIsNotFull = amountOfItems % rowCount != 0;
        if (startAxis == Axis.Horizontal){
            if (stretchFill && OverstretchLastRowOrColumn && lastRowIsNotFull){
                lastRowOrColumnWidth = ((parentWidth + spacing.x) / (float)(amountOfItems % columnCount)) - spacing.x;
            }
        }
        if (startAxis == Axis.Vertical){
            if (stretchFill && OverstretchLastRowOrColumn && lastColumnIsNotFull){
                lastRowOrColumnHeight = ((parentHeight + spacing.y) / (float)(amountOfItems % rowCount)) - spacing.y;
            }
        }
        float centeringOffsetX = 0;
        float centeringOffsetY = 0;
        float centeringLastRowOffsetX = 0;
        float centeringLastColOffsetY = 0;
        if (!stretchFill && centered){
            centeringOffsetX = centeringLastRowOffsetX = (parentWidth + spacing.x - columnCount * (cellWidth + spacing.x)) / 2.0f;
            centeringOffsetY = centeringLastColOffsetY = (parentHeight + spacing.y - rowCount * (cellHeight + spacing.y)) / 2.0f;
            if (startAxis == Axis.Horizontal && lastRowIsNotFull){
                centeringLastRowOffsetX = (parentWidth + spacing.x - (amountOfItems % columnCount) * (cellWidth + spacing.x)) / 2.0f;
            }
            if (startAxis == Axis.Vertical && lastColumnIsNotFull){
                centeringLastColOffsetY = (parentHeight + spacing.y - (amountOfItems % rowCount) * (cellHeight + spacing.y)) / 2.0f;
            }
        }


        int currentRow = 0;
        int currentColumn = 0;
        for (int i = 0; i < amountOfItems; i++){
            if (startAxis == Axis.Horizontal){
                currentRow = i / columnCount;
                currentColumn = i % columnCount;
                if (currentRow == rowCount - 1 && lastRowIsNotFull){
                    cellWidth = lastRowOrColumnWidth;
                    centeringOffsetX = centeringLastRowOffsetX;
                }
            } else {
                currentRow = i % rowCount;
                currentColumn = i / rowCount;
                if (currentColumn == columnCount - 1 && lastColumnIsNotFull){
                    cellHeight = lastRowOrColumnHeight;
                    centeringOffsetY = centeringLastColOffsetY;
                }
            }

            float finalWidth = cellWidth;
            float finalHeight = cellHeight;

            var xPos = centeringOffsetX + (finalWidth * currentColumn) + (spacing.x * currentColumn) + padding.left;
            var yPos = centeringOffsetY + (finalHeight * currentRow) + (spacing.y * currentRow) + padding.top;
            if (!positionValuesAxisOriented) {
                yPos += 0.5f * finalHeight;
                yPos = parentHeight + padding.top + padding.bottom - yPos;
                xPos += 0.5f * finalWidth;
            }
            resultList.Add(new GridItemPlacement{position = new(xPos,yPos), size = new(finalWidth, finalHeight)});
        }
        return resultList;
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (rectChildren.Count == 0) {
            ResetLayoutPreferredSize();
            return;
        }

        float2 desiredSize = new (rectChildren[0].rect.width, rectChildren[0].rect.height);
        if (rectChildren[0].GetComponent<LayoutElement>() != null) desiredSize = new (rectChildren[0].GetComponent<LayoutElement>().preferredWidth, rectChildren[0].GetComponent<LayoutElement>().preferredHeight);
        var resultList = CalculateGridPlacements(rectChildren.Count, desiredSize, positionValuesAxisOriented: true);
        for (int i = 0; i < rectChildren.Count; i++){
            var item = rectChildren[i];
            var gridPos = resultList[i];
            
            SetChildAlongAxis(item, (int)Axis.Horizontal, gridPos.position.x, gridPos.size.x);
            SetChildAlongAxis(item, (int)Axis.Vertical, gridPos.position.y, gridPos.size.y);
        }
        
    }

    private void ResetLayoutPreferredSize()
    {
        if (fitLayoutSizeToContent && GetComponent<LayoutElement>() != null){
            GetComponent<LayoutElement>().preferredHeight = 0;
            GetComponent<LayoutElement>().preferredWidth = 0;
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        //throw new System.NotImplementedException();
    }

    public override void SetLayoutHorizontal()
    {
        //throw new System.NotImplementedException();
    }

    public override void SetLayoutVertical()
    {
        //throw new System.NotImplementedException();
    }
}