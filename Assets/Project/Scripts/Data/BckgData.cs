using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Bckg", menuName = "Scriptable Objects/Bckg")]
    public class BckgData : ScriptableObject
    {
        public List<Sprite> BckgSprites;

        [SerializeField] [Min(0f)] private float baseMoveSpeedPerSec = 1.0f;

        [SerializeField] [Min(1f)] private float speedMultiplier = 2.5f;

        [SerializeField] [Range(0.01f, 1f)] private float cameraHeightRatio = 0.5f;

        [SerializeField] private int baseSortingOrder = -100;

        public float BaseMoveSpeedPerSec => baseMoveSpeedPerSec;

        public float SpeedMultiplier => speedMultiplier;

        public float CameraHeightRatio => cameraHeightRatio;

        public int BaseSortingOrder => baseSortingOrder;
    }
}
