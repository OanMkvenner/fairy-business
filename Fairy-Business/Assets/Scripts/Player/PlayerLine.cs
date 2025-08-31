using UnityEngine;

namespace Player
{
    [System.Serializable]
    public struct PlayerLine
    {
        public LineIdentifier line;
        public Transform redPosition;
        public Transform neutralPosition;
        public Transform bluePosition;
    }
}