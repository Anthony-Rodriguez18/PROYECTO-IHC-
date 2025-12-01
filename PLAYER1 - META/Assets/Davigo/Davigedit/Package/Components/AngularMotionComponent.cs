using UnityEngine;

namespace Davigo.Davigedit
{
    [RequireComponent(typeof(Rigidbody))]
    public class AngularMotionComponent : MonoBehaviour
    {
        public Vector3 AngularVelocity;
        public float Acceleration = 1;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = 1000;
        }

        private void FixedUpdate()
        {
            rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, AngularVelocity * Mathf.Deg2Rad, Acceleration * Mathf.Deg2Rad * Time.fixedDeltaTime);
        }
    }
}