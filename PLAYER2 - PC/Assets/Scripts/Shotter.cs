using UnityEngine;

public class CanonLineal : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo;          // PlayerH (su transform)
    public GameObject prefabBala;       // Prefab con Rigidbody + BalaLineal
    public Transform puntoDisparo;      // Boca del cañón

    [Header("Disparo")]
    public float velocidadBala = 20f;
    public float dispersionGrados = 3f;  // 0 = perfecto, >0 = algo de error

    [Header("Auto disparo (opcional)")]
    public bool autoDisparo = false;
    public float intervaloDisparo = 2f;

    private void Start()
    {
        if (autoDisparo && objetivo != null)
        {
            InvokeRepeating(nameof(Disparar), 1f, intervaloDisparo);
        }
    }

    public void Disparar()
    {
        if (objetivo == null || prefabBala == null)
        {
            Debug.LogWarning("CanonLineal: falta objetivo o prefabBala.");
            return;
        }

        // 1) Tomamos la ÚLTIMA posición conocida del jugador
        Vector3 targetPos = objetivo.position;

        // 2) Calculamos dirección desde el cañón hacia esa posición
        Vector3 spawnPos = puntoDisparo != null ? puntoDisparo.position : transform.position;
        Vector3 dir = (targetPos - spawnPos);

        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;      // por si está encima

        dir.Normalize();

        // 3) Añadimos un poco de dispersión para que no sea perfecto
        if (dispersionGrados > 0f)
        {
            dir = ApplySpread(dir, dispersionGrados);
        }

        // 4) Instanciar bala
        GameObject balaGO = Instantiate(prefabBala, spawnPos, Quaternion.LookRotation(dir));
        Rigidbody rb = balaGO.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("El prefab de bala necesita un Rigidbody.");
            return;
        }

        // Sin gravedad al inicio → trayectoria recta
        rb.useGravity = false;
        rb.linearVelocity = dir * velocidadBala;
    }

    /// <summary>
    /// Aplica una dispersión en grados sobre la dirección original
    /// </summary>
    private Vector3 ApplySpread(Vector3 dir, float maxDegrees)
    {
        // Eje aleatorio alrededor del cual girar
        Vector3 randomAxis = Random.onUnitSphere;
        float angle = Random.Range(-maxDegrees, maxDegrees);
        Quaternion q = Quaternion.AngleAxis(angle, randomAxis);
        return (q * dir).normalized;
    }
}
