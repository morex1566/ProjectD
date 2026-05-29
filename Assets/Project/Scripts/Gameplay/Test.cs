using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<MapData> mapDataRef;

        public void RequestLoadMap()
        {
            MapData mapData = ResourceManager.GetResource(mapDataRef);
            WorldManager.LoadMapData(mapData);
        }

        public void RequestSpawnPlayer()
        {

        }

        public void RequestSpawnMonster()
        {

        }
    }
}
