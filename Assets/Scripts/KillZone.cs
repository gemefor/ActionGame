using UnityEngine;

namespace PinballSmasher
{
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // У тела может быть несколько коллайдеров, поэтому ищем компонент на Rigidbody.
            Rigidbody body = other.attachedRigidbody;
            if (body == null) return;

            if (body.TryGetComponent(out Robot robot))
            {
                robot.Kill();
            }
            else if (body.TryGetComponent(out PlayerBall _))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
                body.gameObject.SetActive(false);
            }
        }
    }
}
