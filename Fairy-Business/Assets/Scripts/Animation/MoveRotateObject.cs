using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class MoveRotateObject : MonoBehaviour
    {
        public Tween MoveY(float y, float duration)
        {
            return GetComponent<RectTransform>().DOMoveY(y, duration);
        }

        public Tween MoveX(float x, float duration)
        {
            return GetComponent<RectTransform>().DOLocalMoveX(x, duration);
        }

        public Tween Rotate(float angle, float duration)
        {
            return GetComponent<RectTransform>().DORotate(new Vector3(0, 0, angle), duration);
        }
    }
}