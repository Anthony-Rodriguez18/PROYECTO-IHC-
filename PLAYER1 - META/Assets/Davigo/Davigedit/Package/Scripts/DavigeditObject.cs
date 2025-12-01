using UnityEngine;

namespace Davigo.Davigedit
{
    [RequireComponent(typeof(IdentifiableObject))]
    public abstract class DavigeditObject : MonoBehaviour
    {
        public IdentifiableObject Replacement { set; protected get; }

        public virtual void Process() { }
    }
}