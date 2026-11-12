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

    [HorizontalGroup("spacing", DisableAutomaticLabelWidth = true)]
    public float2 spacing;
    [HorizontalGroup("spacing", PaddingLeft = 0.3f, MinWidth = 45f)]
    [LabelText("%")]
    [LabelWidth(12)]
    [PropertyRange(0f, 1.0f)]
    public float percentageBasedSpacing;

    [Header("Flexible Grid")]
    public FitType fitType = FitType.FLEXIBLEFILL;

    [ShowIf("@fitType == FitType.FIXEDROWS")]
    public int rows = 1;
    [ShowIf("@fitType == FitType.FIXEDCOLUMNS")]
    public int columns = 1;

    public bool stretchFill = true;
    [HideIf("stretchFill")]
    public bool centered = true;
    [ShowIf("stretchFill")]
    public bool OverstretchLastRowOrColumn = true;
    
    [Tooltip("Use Scale Instead: Instead of changing the targets actual element width and height, uses the elements "+
        "scaling factor to fit it in while keeping its original width and height.")]
    public bool useScaleInstead = false;
    

    [HorizontalGroup("fitLayoutSizeToContent", LabelWidth = 120)]
    [Tooltip("Fit Layout Size To Content: Will using the preferredWidth (or minWidth) and preferredHeight (or minHeight) of the first objects LayoutElement component inside, " +
             "spread the elements inside the grid, then updates the min Size of its own LayoutElement. " +
             "Can be used via a parent LayoutGroup to resize the Grid to fit its content perfectly!")]
    public bool fitLayoutSizeToContent = false;
    [ShowIf("fitLayoutSizeToContent")]
    [HorizontalGroup("fitLayoutSizeToContent", LabelWidth = 60)]
    [LabelText("Grid Ratio w/h")]
    [MinValue(0.0001f)]
    [Tooltip("Desired Grid Ratio: The target width to height Ratio of how the Elements should be spread inside the grid." +
             " The elemens sizes will NOT be touched, but the grid will try to spread them in a way that allows the grid " +
             "itself to roughly reach this ratio.")]
    public float desiredGridRatio = 1.0f;

    [HorizontalGroup("ContentAspectRatio", LabelWidth = 120)]
    [ShowIf("@!fitLayoutSizeToContent")]
    [Tooltip("Set Content Aspect Ratio: Set a manual aspect ratio that is to be kept for each item.")]
    public bool setContentAspectRatio = false;
    [ShowIf("@!fitLayoutSizeToContent && setContentAspectRatio")]
    [HorizontalGroup("ContentAspectRatio", LabelWidth = 60)]
    [LabelText("Ratio w/h")]
    [Tooltip("Content Aspect Ratio Override: The Aspect ratio that is to be kept for each item.")]
    [MinValue(0.0001f)]
    public float contentAspectRatioOverride = 1.0f;

    public Axis startAxis = Axis.Horizontal;


    public float2 GetCellWidthAndHeight(){
        return cellWidthHeight;
    }

    float2 cellWidthHeight = new float2(1,1);

    public struct GridItemPlacement
    {
        public float2 position;
        public float2 size;
    }

    public List<GridItemPlacement> CalculateGridPlacements(int amountOfItems, float2? contentSize = null, bool positionValuesAxisOriented = false){
        List<GridItemPlacement> resultList = new();
        if (amountOfItems == 0) return resultList;

        PaddingFloats paddingFinal = padding;
        float parentWidth = rectTransform.rect.width - paddingFinal.left - paddingFinal.right;
        float parentHeight = rectTransform.rect.height - paddingFinal.top - paddingFinal.bottom;

        // This spaces fields out in a way, where each content item has enough space to fulfill its height-width ratio properly within the field
        // value > 0 == higherThanWide, value < 0 => wider than high
        float contentAspectRatio = 1.0f;
        if (setContentAspectRatio && !fitLayoutSizeToContent) {
            contentAspectRatio = contentAspectRatioOverride;
        }
        else
        {
            if (contentSize.HasValue){
                if (contentSize.Value.y != 0 && contentSize.Value.x != 0){
                    contentAspectRatio = contentSize.Value.x / contentSize.Value.y;
                } else {
                    // Error only needed for debugging purposes
                    //Debug.LogError($"invalid contentsize values: {contentSize.Value}", this);
                }
            }
        }

        // find optimal columnCount
        var desiredRowColumnRatio = parentHeight / parentWidth;
        var spacingModifierHorizontal = spacing.x / parentWidth;
        var spacingModifierVertical = spacing.y / parentHeight;

        bool layoutFittingPossible = false;
        LayoutElement layoutElement = null;
        if (fitLayoutSizeToContent){
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null) {
                Debug.LogError("fitLayoutSizeToContent requires LayoutElement on the Gameobject (and any layout as parent that uses its minWidth/Height values)");
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
        

        /*if (!stretchFill)*/ desiredRowColumnRatio *= contentAspectRatio;
        
        // when percentageBasedSpacing is used, rations change slightly because the added spacing has 1:1 ratio while the shrinking objects have the original ratio
        desiredRowColumnRatio *= ((1 - percentageBasedSpacing) + percentageBasedSpacing / contentAspectRatio); 
        
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

                    // paddingRatioModifier changes ratio by fake-reducing the height and width of the parent (for the matter of this calculation)
                    // So once multiple rows/columns are introduced, each additional row/column introduces one more padding onto the mix, reducing
                    // the available size by a percentage depending on the relative space this padding takes
                    var spacingRatioModVertical = (1f - spacingModifierVertical * (checkRow - 1f));
                    var spacingRatioModHorizontal = (1f - spacingModifierHorizontal * (checkColumn - 1f));
                    if (spacingRatioModVertical > 0 && spacingRatioModHorizontal > 0)
                    {
                        var columnRatioDistance = Mathf.Abs(currentRowColumnRatio - desiredRowColumnRatio * spacingRatioModVertical / spacingRatioModHorizontal);
                        if (lastCheckedDistance > columnRatioDistance)
                        {
                            columnCount = checkColumn;
                            rowCount = checkRow;
                            lastCheckedDistance = columnRatioDistance;
                        }
                    };
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
                    // see "paddingRatioModifier" above
                    var spacingRatioModVertical = (1f - spacingModifierVertical * (checkRow - 1f));
                    var spacingRatioModHorizontal = (1f - spacingModifierHorizontal * (checkColumn - 1f));
                    if (spacingRatioModVertical > 0 && spacingRatioModHorizontal > 0)
                    {
                        var columnRatioDistance =
                            Mathf.Abs(currentRowColumnRatio - desiredRowColumnRatio * spacingRatioModVertical / spacingRatioModHorizontal);
                        if (lastCheckedDistance > columnRatioDistance)
                        {
                            columnCount = checkColumn;
                            rowCount = checkRow;
                            lastCheckedDistance = columnRatioDistance;
                        }
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
        // recalculate using percentageBasedSpacing (if percentageBasedSpacing > 0)
        var percentSpaceAbsolute = Mathf.Min(cellWidth * percentageBasedSpacing, cellHeight * percentageBasedSpacing);
        var spacingFinal = spacing + percentSpaceAbsolute;
        paddingFinal += percentSpaceAbsolute / 2.0f;
        parentWidth -= percentSpaceAbsolute;
        parentHeight -= percentSpaceAbsolute;
        cellWidth = ((parentWidth + spacingFinal.x) / (float)columnCount) - spacingFinal.x;
        cellHeight = ((parentHeight + spacingFinal.y) / (float)rowCount) - spacingFinal.y;
        if (layoutFittingPossible){
            cellWidth = contentSize.Value.x;
            cellHeight = contentSize.Value.y;
            percentSpaceAbsolute = Mathf.Min(cellWidth * percentageBasedSpacing, cellHeight * percentageBasedSpacing);
            spacingFinal = percentSpaceAbsolute;
            paddingFinal += percentSpaceAbsolute / 2.0f;
            cellWidth -= cellWidth * percentageBasedSpacing;
            cellHeight -= cellHeight * percentageBasedSpacing;
            spacingFinal = spacing + new float2(Mathf.Min(spacingFinal.x, spacingFinal.y));
            parentWidth = columnCount * (cellWidth + spacingFinal.x) - spacingFinal.x;
            parentHeight = rowCount * (cellHeight + spacingFinal.y) - spacingFinal.y;
            layoutElement.preferredWidth = parentWidth + paddingFinal.left + paddingFinal.right;
            layoutElement.preferredHeight = parentHeight + paddingFinal.top + paddingFinal.bottom;
            layoutElement.minWidth = parentWidth + paddingFinal.left + paddingFinal.right;
            layoutElement.minHeight = parentHeight + paddingFinal.top + paddingFinal.bottom;
        }

        if (!stretchFill){
            // try to find the smallest side while trying to keep the desired height-width ratio (usually a 1:1 square)
            if (cellWidth / contentAspectRatio > cellHeight) cellWidth = cellHeight * contentAspectRatio;
            else cellHeight = cellWidth / contentAspectRatio;
        }

        cellWidthHeight = new float2(cellWidth, cellHeight);

        float lastRowOrColumnWidth = cellWidth;
        float lastRowOrColumnHeight = cellHeight;
        bool lastRowIsNotFull = amountOfItems % columnCount != 0;
        bool lastColumnIsNotFull = amountOfItems % rowCount != 0;
        if (startAxis == Axis.Horizontal){
            if (stretchFill && OverstretchLastRowOrColumn && lastRowIsNotFull){
                lastRowOrColumnWidth = ((parentWidth + spacingFinal.x) / (float)(amountOfItems % columnCount)) - spacingFinal.x;
            }
        }
        if (startAxis == Axis.Vertical){
            if (stretchFill && OverstretchLastRowOrColumn && lastColumnIsNotFull){
                lastRowOrColumnHeight = ((parentHeight + spacingFinal.y) / (float)(amountOfItems % rowCount)) - spacingFinal.y;
            }
        }
        float centeringOffsetX = 0;
        float centeringOffsetY = 0;
        float centeringLastRowOffsetX = 0;
        float centeringLastColOffsetY = 0;
        if (!stretchFill && centered){
            centeringOffsetX = centeringLastRowOffsetX = (parentWidth + spacingFinal.x - columnCount * (cellWidth + spacingFinal.x)) / 2.0f;
            centeringOffsetY = centeringLastColOffsetY = (parentHeight + spacingFinal.y - rowCount * (cellHeight + spacingFinal.y)) / 2.0f;
            if (startAxis == Axis.Horizontal && lastRowIsNotFull){
                centeringLastRowOffsetX = (parentWidth + spacingFinal.x - (amountOfItems % columnCount) * (cellWidth + spacingFinal.x)) / 2.0f;
            }
            if (startAxis == Axis.Vertical && lastColumnIsNotFull){
                centeringLastColOffsetY = (parentHeight + spacingFinal.y - (amountOfItems % rowCount) * (cellHeight + spacingFinal.y)) / 2.0f;
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

            var xPos = centeringOffsetX + (finalWidth * currentColumn) + (spacingFinal.x * currentColumn) + paddingFinal.left;
            var yPos = centeringOffsetY + (finalHeight * currentRow) + (spacingFinal.y * currentRow) + paddingFinal.top;
            if (!positionValuesAxisOriented) {
                yPos += 0.5f * finalHeight;
                yPos = parentHeight + paddingFinal.top + paddingFinal.bottom - yPos;
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
            ResetLayoutMinSize();
            return;
        }

        float2 desiredSize = new (rectChildren[0].rect.width, rectChildren[0].rect.height);
        if (rectChildren[0].GetComponent<LayoutElement>() != null) {
            desiredSize = new (rectChildren[0].GetComponent<LayoutElement>().minWidth, rectChildren[0].GetComponent<LayoutElement>().minHeight);
            if (desiredSize.x == 0 || desiredSize.y == 0) desiredSize = new (rectChildren[0].GetComponent<LayoutElement>().preferredWidth, rectChildren[0].GetComponent<LayoutElement>().preferredHeight);
        }
        
        var resultList = CalculateGridPlacements(rectChildren.Count, desiredSize, positionValuesAxisOriented: true);
        for (int i = 0; i < rectChildren.Count; i++){
            var item = rectChildren[i];
            var gridPos = resultList[i];
            
            float2 finalSize = new float2(gridPos.size.x, gridPos.size.y);
            float horizontalScale = 1.0f;
            float verticalScale = 1.0f;
            if (useScaleInstead)
            {
                if (item.rect.width == 0 || item.rect.height == 0) continue;
                horizontalScale = finalSize.x / item.rect.width;
                verticalScale = finalSize.y / item.rect.height;
                finalSize.x = item.rect.width;
                finalSize.y = item.rect.height;
                
                item.localScale = new(horizontalScale, verticalScale, item.localScale.z);
                SetChildAlongAxisWithScale(item, (int)Axis.Horizontal, gridPos.position.x, horizontalScale);
                SetChildAlongAxisWithScale(item, (int)Axis.Vertical, gridPos.position.y, verticalScale);
                m_Tracker.Add(this, item, DrivenTransformProperties.ScaleX | DrivenTransformProperties.ScaleY);
            }
            else
            {
                SetChildAlongAxis(item, (int)Axis.Horizontal, gridPos.position.x, finalSize.x);
                SetChildAlongAxis(item, (int)Axis.Vertical, gridPos.position.y, finalSize.y);
            }
        }
        
    }

    private void ResetLayoutMinSize()
    {
        if (fitLayoutSizeToContent && GetComponent<LayoutElement>() != null){
            GetComponent<LayoutElement>().minHeight = 0;
            GetComponent<LayoutElement>().minWidth = 0;
            GetComponent<LayoutElement>().preferredWidth = 0;
            GetComponent<LayoutElement>().preferredHeight = 0;
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