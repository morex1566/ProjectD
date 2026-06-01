using UnityEngine;

namespace TRPG.Runtime
{
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        public static readonly Vector3 TileSize = Vector3.one;

        public static class BackgroundColor
        {
            public static readonly string Sky = "#1E202A";

            public static readonly string Stone = "#1E1E1E";
        }
    }
}
