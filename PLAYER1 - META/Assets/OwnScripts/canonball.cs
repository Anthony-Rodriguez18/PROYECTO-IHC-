using UnityEngine;

public class DestructorBala : MonoBehaviour
{
    [Header("Configuración")]
    public bool destruirAlImpacto = true;
    public float retraso = 0f; // Retraso en segundos antes de destruirse
    
    [Header("Efectos (opcional)")]
    public GameObject efectoExplosion; // Prefab de efecto visual (opcional)
    public AudioClip sonidoImpacto;    // Sonido al impactar (opcional)

    private void Start()
    {
        // Asignar tag "Bala" automáticamente al crearse
        gameObject.tag = "Bala";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (destruirAlImpacto)
        {
            // Reproducir efecto (si existe)
            if (efectoExplosion != null)
            {
                Instantiate(efectoExplosion, transform.position, Quaternion.identity);
            }
            
            // Reproducir sonido (si existe)
            if (sonidoImpacto != null)
            {
                AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position);
            }
            
            // Destruir bala (con o sin retraso)
            if (retraso > 0)
                Destroy(gameObject, retraso);
            else
                Destroy(gameObject);
        }
    }
}