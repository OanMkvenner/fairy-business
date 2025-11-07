using System;
using UnityEngine;

namespace Animation.Position
{
    public enum Side
    {
        Top,
        Bottom,
        Left,
        Right
    }
    
    [Serializable]
    public class UIOffScreenPosition
    {
        [SerializeField] private Side side;

        public Vector2 GetOffScreenPosition(Canvas canvas)
        {
            return side switch
            {
                Side.Bottom => GetBottomOffScreenPosition(canvas),
                Side.Top => GetTopOffScreenPosition(canvas),
                Side.Left => GetLeftOffScreenPosition(canvas),
                Side.Right => GetRightOffScreenPosition(canvas),
                _ => Vector2.zero
            };
        }

        private Vector2 GetBottomOffScreenPosition(Canvas canvas)
        {
            Vector2 offPosition = new Vector2(Screen.width / 2, -Screen.height);
            return ScreenPointToLocalPoint(canvas, offPosition);
        }

        private Vector2 GetTopOffScreenPosition(Canvas canvas)
        {
            Vector2 offPosition = new Vector2(Screen.width / 2, Screen.height * 2);
            return ScreenPointToLocalPoint(canvas, offPosition);
        }

        private Vector2 GetLeftOffScreenPosition(Canvas canvas)
        {
            Vector2 offPosition = new Vector2(-Screen.width, Screen.height / 2);
            return ScreenPointToLocalPoint(canvas, offPosition);
        }

        private Vector2 GetRightOffScreenPosition(Canvas canvas)
        {
            Vector2 offPosition = new Vector2(Screen.width * 2, Screen.height / 2);
            return ScreenPointToLocalPoint(canvas, offPosition);
        }
        
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