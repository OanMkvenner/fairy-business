using System;
using UnityEngine;

namespace Animation.Position
{
    public enum Side
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    [Serializable]
    public class UIOffScreenPosition
    {
        [SerializeField] private Side side;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;

        public Vector2 GetOffScreenPosition(Canvas canvas)
        {
            return side switch
            {
                Side.Bottom      => GetBottomOffScreenPosition(canvas),
                Side.Top         => GetTopOffScreenPosition(canvas),
                Side.Left        => GetLeftOffScreenPosition(canvas),
                Side.Right       => GetRightOffScreenPosition(canvas),
                Side.TopLeft     => GetTopLeftOffScreenPosition(canvas),
                Side.TopRight    => GetTopRightOffScreenPosition(canvas),
                Side.BottomLeft  => GetBottomLeftOffScreenPosition(canvas),
                Side.BottomRight => GetBottomRightOffScreenPosition(canvas),
                _                => Vector2.zero
            };
        }

        // Hilfsfunktion: Canvasgröße im lokalen Raum bestimmen
        private static Vector2 GetCanvasSize(Canvas canvas)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            return canvasRect.rect.size;
        }

        // --- Seitenpositionen ---

        private Vector2 GetBottomOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(0 + xOffset, -canvasSize.y / 2 - yOffset);
        }

        private Vector2 GetTopOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(0 + xOffset, canvasSize.y / 2 + yOffset);
        }

        private Vector2 GetLeftOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(-canvasSize.x / 2 - xOffset, 0 + yOffset);
        }

        private Vector2 GetRightOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(canvasSize.x / 2 + xOffset, 0 + yOffset);
        }

        // --- Diagonalen ---

        private Vector2 GetTopLeftOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(-canvasSize.x / 2 - xOffset, canvasSize.y / 2 + yOffset);
        }

        private Vector2 GetTopRightOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(canvasSize.x / 2 + xOffset, canvasSize.y / 2 + yOffset);
        }

        private Vector2 GetBottomLeftOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(-canvasSize.x / 2 - xOffset, -canvasSize.y / 2 - yOffset);
        }

        private Vector2 GetBottomRightOffScreenPosition(Canvas canvas)
        {
            Vector2 canvasSize = GetCanvasSize(canvas);
            return new Vector2(canvasSize.x / 2 + xOffset, -canvasSize.y / 2 - yOffset);
        }
    }
}
