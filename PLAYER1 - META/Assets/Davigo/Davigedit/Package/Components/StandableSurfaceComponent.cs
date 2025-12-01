using UnityEngine;

public class StandableSurfaceComponent : MonoBehaviour
{
    [SerializeField, Tooltip("Overrides the Warrior's default maximum angle of surfaces they can stand on. " +
        "When this number is very high, they will be able to stand on very steep slopes. When it is zero, " +
        "they will not be able to stand on this surface at all.")]
    float maximumGroundAngle = 80f;

    public float MaximumGroundAngle => maximumGroundAngle;
}
