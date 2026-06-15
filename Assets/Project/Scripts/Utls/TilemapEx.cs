using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    public static class TilemapEx
    {
        /// <summary>
        /// 타일맵의 특정 셀에서 스프라이트를 가져옵니다.
        /// </summary>
        public static bool TryGetSprite(Tilemap tilemap, Vector3Int cellPos, out Sprite sprite)
        {
            sprite = null;

            // 타일맵이 없으면 실패합니다.
            if (tilemap == null)
            {
                return false;
            }

            // 해당 셀에 표시되는 스프라이트를 가져옵니다.
            sprite = tilemap.GetSprite(cellPos);

            return sprite != null;
        }

        /// <summary>
        /// 스프라이트가 사용하는 원본 텍스처를 가져옵니다.
        /// </summary>
        public static bool TryGetTexture(Tilemap tilemap, Vector3Int cellPos, out Texture2D texture)
        {
            texture = null;

            // 해당 셀의 스프라이트를 가져옵니다.
            if (TryGetSprite(tilemap, cellPos, out Sprite sprite) == false)
            {
                return false;
            }

            // 스프라이트가 참조하는 텍스처를 가져옵니다.
            texture = sprite.texture;

            return texture != null;
        }
    }
}
