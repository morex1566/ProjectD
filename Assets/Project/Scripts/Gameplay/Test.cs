using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<MapData> mapDataRef;

        private MapData mapData = null;

        public void RequestLoadMap()
        {
            var tiles =  WorldManager.Tiles;
            foreach (var tile in tiles)
            {
                WorldManager.Despawn(tile.Value.CellPos);
            }

            var creatures = WorldManager.Creatures;
            foreach (var creature in creatures)
            {
                WorldManager.Despawn(creature.Value.GetInstanceID());
            }

            mapData = ResourceManager.GetResource(mapDataRef);
            WorldManager.SpawnTiles(mapData);
            WorldManager.SpawnMonsters(mapData);
            WorldManager.SpawnPlayer(Vector3Int.zero);
        }
    }
}
