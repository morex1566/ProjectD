using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldManager 초기화에 필요한 맵 데이터와 프리팹 참조를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_WorldManagerSettings", menuName = "Scriptable Objects/Settings/WorldManager")]
    public partial class WorldManagerSettingsData : ScriptableObject
    {
        public ObjectSelector Selector;

        public WorldCameraController CamController;
    }
}
