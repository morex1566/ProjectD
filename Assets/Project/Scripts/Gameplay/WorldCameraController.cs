using UnityEngine;


namespace TRPG.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class WorldCameraController : MonoBehaviour
    {
        [ReadOnly] public Camera Cam;



        private void Awake()
        {
            Cam = GetComponent<Camera>();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            
        }
    }
}