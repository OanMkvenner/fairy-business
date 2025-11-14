using UnityEngine;

namespace Utility
{
    public class CanvasReferencer : MonobheaviourSingletonCustom<CanvasReferencer>
    {
        public Canvas Canvas { get; private set; }
        
        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
        }
    }
}