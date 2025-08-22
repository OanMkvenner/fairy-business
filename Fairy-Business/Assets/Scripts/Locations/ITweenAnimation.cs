using DG.Tweening;
using UnityEngine;

namespace Locations
{
    public interface ITweenAnimation
    {
        public Tween MoveY(float y, float duration);
        
        public Tween MoveX(float x, float duration);
        
        public Tween Rotate(float angle, float duration);

        public void SetPosition(Vector3 position);
    }
}