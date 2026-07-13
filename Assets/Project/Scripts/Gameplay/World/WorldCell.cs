using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드의 픽셀 하나에 대한 데이터
    /// </summary>
    public readonly struct WorldCell
    {
        public WorldMaterialType MaterialType { get; }

        public bool IsEmpty => MaterialType == WorldMaterialType.Empty;


        public WorldCell(WorldMaterialType materialType)
        {
            MaterialType = materialType;
        }
    }
}
