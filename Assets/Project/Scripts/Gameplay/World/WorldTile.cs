namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일의 재질입니다.
    /// </summary>
    public enum WorldTileMaterialType : byte
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
    public struct WorldTile
    {
        /// <summary>
        /// 타일을 표현하는 지형 재질입니다.
        /// </summary>
        public WorldTileMaterialType MaterialType;

        /// <summary>
        /// 완전히 비어 있는 타일인지 반환합니다.
        /// </summary>
        public bool IsEmpty => MaterialType == WorldTileMaterialType.Empty;

        /// <summary>
        /// 재질을 지정하여 타일을 생성합니다.
        /// </summary>
        public WorldTile(WorldTileMaterialType materialType)
        {
            MaterialType = materialType;
        }
    }
}
