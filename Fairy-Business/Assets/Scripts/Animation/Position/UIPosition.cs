using System;
using UnityEngine;

namespace Animation.Position
{
    public enum Pivot
    {
        Middle,
        BottomLeft,
        BottomRight,
        TopRight,
    }

    [Serializable]
    public class UIPosition
    {
        [SerializeField] private Pivot startingPivot;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;

        private UIPosition(Pivot startingPivot, float xOffset, float yOffset)
        {
            this.startingPivot = startingPivot;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }

        public Vector2 GetUIPosition(Canvas canvas)
        {
            return startingPivot switch
            {
                Pivot.Middle     => CalculateMiddleScreenPosition(canvas),
                Pivot.BottomLeft => CalculateBottomLeftScreenPosition(canvas),
                Pivot.TopRight   => CalculateTopRightScreenPosition(canvas),
                Pivot.BottomRight => CalculateBottomRightScreenPosition(canvas),
                _                => Vector2.zero
            };
        }

        #region OnScreenPositions

        private Vector2 CalculateMiddleScreenPosition(Canvas canvas)
        {
            Vector2 screenCenter = new Vector2((Screen.width / 2f) + xOffset, (Screen.height / 2f) + yOffset);
            return ScreenPointToLocalPoint(canvas, screenCenter);
        }

        private Vector2 CalculateBottomLeftScreenPosition(Canvas canvas)
        {
            Vector2 screenPosition = new Vector2(xOffset, yOffset);
            return ScreenPointToLocalPoint(canvas, screenPosition);
        }

        private Vector2 CalculateBottomRightScreenPosition(Canvas canvas)
        {
            Vector2 screenPosition = new Vector2(Screen.width + xOffset, yOffset);
            return ScreenPointToLocalPoint(canvas, screenPosition);
        }
        
        private Vector2 CalculateTopRightScreenPosition(Canvas canvas)
        {
            Vector2 screenPosition = new Vector2(Screen.width + xOffset, Screen.height + yOffset);
            return ScreenPointToLocalPoint(canvas, screenPosition);
        }

        #endregion

        private Vector2 ScreenPointToLocalPoint(Canvas canvas, Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 canvasPos
            );
            return canvasPos;
        }
    }
}