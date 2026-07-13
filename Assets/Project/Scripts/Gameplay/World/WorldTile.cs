namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일의 논리적인 종류입니다.
    /// </summary>
    public enum WorldTileType : byte
    {
        Empty = 0,
        Soil,
        Stone,
        Sand,
        Water,
    }

    /// <summary>
    /// 월드의 논리적인 타일 한 칸입니다.
    /// </summary>
    public readonly struct WorldTile
    {
        public WorldTileType Type { get; }

        public bool IsEmpty => Type == WorldTileType.Empty;


        public WorldTile(WorldTileType type)
        {
            Type = type;
        }
    }
}