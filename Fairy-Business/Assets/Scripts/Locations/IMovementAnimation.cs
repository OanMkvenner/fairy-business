namespace Locations
{
    public interface IMovementAnimation
    {
        public void MoveY(float y, float duration);
        
        public void MoveX(float x, float duration);
        
        public void Rotate(float angle);
    }
}