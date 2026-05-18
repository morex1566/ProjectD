using UnityEngine;

namespace TRPG.Runtime
{
    public class Player : MonoBehaviourSingleton<Player>
    {
        public static void Init()
        {
            GetInstance();
        }
    }
}
