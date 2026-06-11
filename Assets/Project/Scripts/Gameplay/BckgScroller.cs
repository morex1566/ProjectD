using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 하나의 스프라이트를 3개 타일로 이어붙여 사이드 스크롤하는 레이어입니다.
    /// </summary>
    public class BckgLayer
    {
        public const int TileCountPerLayer = 3;

        public readonly List<Transform> Tiles = new();

        public readonly Vector3 StartPosition;

        public readonly float MoveSpeed;

        public readonly float Width;

        public BckgLayer(
            Sprite bckgSprite,
            Transform parent,
            Vector3 startPosition,
            Vector3 scale,
            float moveSpeed,
            int sortingOrder)
        {
            StartPosition = startPosition;
            MoveSpeed = moveSpeed;
            Width = bckgSprite.bounds.size.x * scale.x;

            // [원본][복제][복제] 형태로 오른쪽에 이어붙임
            for (int i = 0; i < TileCountPerLayer; i++)
            {
                GameObject tile = new($"Bckg_{bckgSprite.name}_{i}");
                tile.transform.SetParent(parent, false);
                tile.transform.localPosition = StartPosition + Vector3.right * (Width * i);
                tile.transform.localScale = scale;

                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = bckgSprite;
                renderer.sortingOrder = sortingOrder;

                Tiles.Add(tile.transform);
            }
        }
    }

    [Serializable]
    public class BckgScroller
    {
        private readonly List<BckgLayer> layers = new();

        private readonly float baseMoveSpeedPerSec;

        private readonly float speedMultiplier;

        private readonly float cameraHeightRatio;

        private readonly int baseSortingOrder;

        private readonly Camera targetCamera;

        public BckgScroller(BckgData data, Transform parent, Camera targetCamera)
        {
            if (data == null)
            {
                Debug.LogWarning($"{nameof(BckgScroller)} init failed. {nameof(BckgData)} is null.");
                return;
            }

            if (parent == null)
            {
                Debug.LogWarning($"{nameof(BckgScroller)} init failed. Parent is null.");
                return;
            }

            baseMoveSpeedPerSec = data.BaseMoveSpeedPerSec;
            speedMultiplier = data.SpeedMultiplier;
            cameraHeightRatio = data.CameraHeightRatio;
            baseSortingOrder = data.BaseSortingOrder;
            this.targetCamera = targetCamera;

            InstantiateBckgs(data.BckgSprites, parent);
        }

        /// <summary>
        /// 배경 데이터에 들어있는 각 스프라이트를 레이어로 변환합니다.
        /// 각 레이어는 SpriteRenderer 3개로 구성됩니다.
        /// </summary>
        private void InstantiateBckgs(IReadOnlyList<Sprite> bckgSprites, Transform parent)
        {
            if (bckgSprites == null)
            {
                return;
            }

            for (int i = 0; i < bckgSprites.Count; i++)
            {
                Sprite bckgSprite = bckgSprites[i];

                if (bckgSprite == null)
                {
                    continue;
                }

                // 먼저 들어온 스프라이트는 뒤 레이어라고 보고 느리게 이동합니다.
                float moveSpeed = baseMoveSpeedPerSec * Mathf.Pow(speedMultiplier, layers.Count);
                Vector3 scale = CalculateCameraHalfHeightScale(bckgSprite);
                Vector3 startPosition = CalculateTopAlignedStartPosition(bckgSprite, scale, parent);

                // 레이어 생성
                BckgLayer layer = new(bckgSprite, parent, startPosition, scale, moveSpeed, baseSortingOrder + layers.Count);
                layers.Add(layer);
            }
        }

        private Vector3 CalculateCameraHalfHeightScale(Sprite bckgSprite)
        {
            if (targetCamera == null || bckgSprite.bounds.size.y <= 0f)
            {
                return Vector3.one;
            }

            float targetHeight = GetCameraWorldHeight() * cameraHeightRatio;
            float scale = targetHeight / bckgSprite.bounds.size.y;

            return new Vector3(scale, scale, 1f);
        }

        private Vector3 CalculateTopAlignedStartPosition(Sprite bckgSprite, Vector3 scale, Transform parent)
        {
            if (targetCamera == null)
            {
                return Vector3.zero;
            }

            float cameraTopY = targetCamera.transform.position.y + GetCameraWorldHeight() * 0.5f;
            float spriteHalfHeight = bckgSprite.bounds.size.y * scale.y * 0.5f;
            Vector3 worldPosition = new(targetCamera.transform.position.x, cameraTopY - spriteHalfHeight, parent.position.z);

            return parent.InverseTransformPoint(worldPosition);
        }

        private float GetCameraWorldHeight()
        {
            if (targetCamera.orthographic)
            {
                return targetCamera.orthographicSize * 2f;
            }

            float distance = Mathf.Abs(targetCamera.transform.position.z);
            return 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        public void Update(float deltaTime)
        {
            foreach (BckgLayer layer in layers)
            {
                MoveLayer(layer, deltaTime);
            }
        }

        private void MoveLayer(BckgLayer layer, float deltaTime)
        {
            float moveDistance = layer.MoveSpeed * deltaTime;

            // 레이어에 속한 타일 3개를 전부 왼쪽으로 이동
            for (int i = 0; i < layer.Tiles.Count; i++)
            {
                Transform tile = layer.Tiles[i];

                if (tile == null)
                {
                    continue;
                }

                tile.localPosition += Vector3.left * moveDistance;
            }

            // 왼쪽으로 완전히 빠진 타일을 오른쪽 끝으로 다시 보냄
            for (int i = 0; i < layer.Tiles.Count; i++)
            {
                Transform tile = layer.Tiles[i];

                if (tile == null)
                {
                    continue;
                }

                float leftLimitX = layer.StartPosition.x - layer.Width;
                if (tile.localPosition.x <= leftLimitX)
                {
                    float rightMostX = GetRightMostX(layer);

                    tile.localPosition = new Vector3(rightMostX + layer.Width, tile.localPosition.y, tile.localPosition.z);
                }
            }
        }

        private float GetRightMostX(BckgLayer layer)
        {
            float rightMostX = float.MinValue;

            for (int i = 0; i < layer.Tiles.Count; i++)
            {
                Transform tile = layer.Tiles[i];

                if (tile == null) continue;

                if (tile.localPosition.x > rightMostX)
                {
                    rightMostX = tile.localPosition.x;
                }
            }

            return rightMostX;
        }
    }
}
