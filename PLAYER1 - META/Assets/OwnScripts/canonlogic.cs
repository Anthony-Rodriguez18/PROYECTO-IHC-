using UnityEngine;

public class Canon : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject objetivo;          // El objetivo al que disparamos
    public GameObject prefabBala;        // Prefab con Rigidbody
    public AudioClip sonidoDisparo;      // NUEVO: Sonido al disparar

    [Header("Configuración del disparo")]
    public float fuerzaInicial = 10f;    // Potencia base del disparo
    [Range(0f, 1f)]
    public float precision = 1f;         // 1 = perfecto, 0 = muy impreciso
    public float alturaExtra = 1.5f;     // Curvatura de la parábola
    public float intervaloDisparo = 5f;  // Cada cuántos segundos dispara
    [Range(0f, 1f)]
    public float volumenDisparo = 1f;    // NUEVO: Volumen del sonido

    [Header("Opcional")]
    public Transform puntoDisparo;       // Lugar desde donde sale la bala

    // NUEVO: Variable para el componente de audio
    private AudioSource audioSource;

    private void Start()
    {
        // NUEVO: Busca o crea componente de audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoDisparo != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Repite la función "Disparar" cada X segundos
        InvokeRepeating(nameof(Disparar), 1f, intervaloDisparo);
    }

    public void Disparar()
    {
        if (objetivo == null || prefabBala == null)
        {
            Debug.LogWarning("Falta asignar el objetivo o el prefab de la bala.");
            return;
        }

        // NUEVO: Reproducir sonido de disparo
        if (sonidoDisparo != null)
        {
            if (audioSource != null)
            {
                // Usar el componente AudioSource si existe
                audioSource.PlayOneShot(sonidoDisparo, volumenDisparo);
            }
            else
            {
                // Reproducir en posición sin necesidad de AudioSource
                AudioSource.PlayClipAtPoint(sonidoDisparo, transform.position, volumenDisparo);
            }
        }

        // Crear la instancia de la bala
        Vector3 spawnPos = puntoDisparo != null ? puntoDisparo.position : transform.position;
        GameObject bala = Instantiate(prefabBala, spawnPos, Quaternion.identity);

        Rigidbody rb = bala.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("El prefab de la bala necesita un Rigidbody.");
            return;
        }

        // Calcular dirección con parábola
        Vector3 direccion = CalcularDireccionParabolica(spawnPos, objetivo.transform.position, alturaExtra);

        // Añadir imprecisión (según "precision")
        Vector3 desviacion = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * (1f - precision);

        direccion += desviacion;

        // Aplicar fuerza inicial
        rb.linearVelocity = direccion * fuerzaInicial;

        // Destruir bala automáticamente tras 10 segundos (para evitar acumulación)
        Destroy(bala, 10f);
    }

    private Vector3 CalcularDireccionParabolica(Vector3 origen, Vector3 destino, float altura)
    {
        Vector3 dir = destino - origen;
        float distancia = dir.magnitude;

        // Separar componentes horizontales y verticales
        Vector3 dirHorizontal = new Vector3(dir.x, 0, dir.z).normalized;

        // Calcular ángulo con elevación (parábola)
        float alturaObjetivo = dir.y;
        float angulo = Mathf.Atan2(altura + alturaObjetivo, distancia);

        Vector3 direccionFinal = (dirHorizontal * Mathf.Cos(angulo)) + (Vector3.up * Mathf.Sin(angulo));

        return direccionFinal.normalized;
    }
}