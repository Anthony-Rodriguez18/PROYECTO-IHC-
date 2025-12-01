using UnityEngine;

namespace Davigo.Davigedit
{
    [RequireComponent(typeof(Rigidbody))]
    public partial class DavigoRigidbodyComponent : DavigeditComponent
    {
        [Tooltip("Multiplies the amount of force applied to the warrior when struck by this rigidbody.")]
        public float ForceMultiplier = 1;
    }
}
