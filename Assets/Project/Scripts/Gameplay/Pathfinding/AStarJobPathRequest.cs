namespace TRPG.Runtime
{
    public struct AStarJobPathRequest
    {
        public int StartX;
        public int StartY;
        public int TargetX;
        public int TargetY;

        public AStarJobPathRequest(int startX, int startY, int targetX, int targetY)
        {
            StartX = startX;
            StartY = startY;
            TargetX = targetX;
            TargetY = targetY;
        }
    }
}