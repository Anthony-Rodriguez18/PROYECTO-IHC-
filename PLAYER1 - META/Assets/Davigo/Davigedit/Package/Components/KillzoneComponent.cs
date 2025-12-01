using System;
using UnityEngine;

public class KillzoneComponent : MonoBehaviour
{
    private void Awake()
    {        
        gameObject.AddComponent(Type.GetType("Killzone"));
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        Destroy(this);
    }
}
