using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    public class TurnNofityUI : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Animator animator;

        [SerializeField] private float lifetime;

        private Coroutine Squash;


        private void Start()
        {
            StartCoroutine(SquashCo());
        }

        private IEnumerator SquashCo()
        {
            yield return new WaitForSeconds(lifetime);


        }
    }
}
