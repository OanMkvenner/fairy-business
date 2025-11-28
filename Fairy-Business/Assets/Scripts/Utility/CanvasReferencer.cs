using UnityEngine;

namespace Utility
{
    public class CanvasReferencer : MonobehaviourSingletonCustom<CanvasReferencer>
    {
        public Canvas Canvas { get; private set; }
        
        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
        }
    }
}