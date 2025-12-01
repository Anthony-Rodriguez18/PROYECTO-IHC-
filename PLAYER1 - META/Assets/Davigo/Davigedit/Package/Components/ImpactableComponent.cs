using System;
using UnityEngine;

public class ImpactableComponent : MonoBehaviour
{
    private void Awake()
    {
        gameObject.AddComponent(Type.GetType("Impactable"));
        Destroy(this);
    }
}
