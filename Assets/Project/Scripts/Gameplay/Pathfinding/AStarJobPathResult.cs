namespace TRPG.Runtime
{
    public struct AStarJobPathResult
    {
        public int Length;
        public int Status;

        public bool IsSuccess => Status == AStarJobPathStatus.Success;
    }

    public static class AStarJobPathStatus
    {
        public const int NoPath = 0;
        public const int Success = 1;
        public const int InvalidRequest = -1;
        public const int PathTooLong = -2;
    }
}