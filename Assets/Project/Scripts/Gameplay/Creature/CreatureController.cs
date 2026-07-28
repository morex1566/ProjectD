using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriter;

        [SerializeField, ReadOnly] private Animator animator;

        [SerializeField] private BoxCollider2D hitBox;


        private void OnValidate()
        {
            CacheComponents();
            SetLayers();
        }

        private void Awake()
        {
            CacheComponents();
            SetLayers();
        }

        private void CacheComponents()
        {
            spriter = gameObject.GetComponentInHierarchy<SpriteRenderer>();
            animator = gameObject.GetComponentInHierarchy<Animator>();
        }

        private void SetLayers()
        {
            int layer = LayerMask.NameToLayer(UnityConstant.Layers.Creature);
            gameObject.layer = layer;
        }
    }
}
