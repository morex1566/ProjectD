using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    

    public class MapController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private MapGenerator mapGenerator;

        private void Awake()
        {
            mapGenerator = GetComponent<MapGenerator>();
        }

        private void Start()
        {

        }
    }
}
