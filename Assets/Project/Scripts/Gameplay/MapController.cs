using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<BckgData> bckgData;

        private BckgScroller bckgScroller;


        private void Start()
        {
            BckgData data = ResourceManager.GetResource(bckgData);
            bckgScroller = new BckgScroller(data, transform, WorldManager.CamController.Cam);
        }

        private void Update()
        {
            bckgScroller?.Update(Time.deltaTime);
        }

        /// <summary>
        /// 1. 배경을 변경
        /// 2. 타일 그라운드를 변경
        /// </summary>
        private void ChangeMap()
        {

        }
    }
}
