using UnityEngine;

public class ControladorCanon : MonoBehaviour
{
    // ... Tus variables (objetivoJugador, proyectilPrefab, etc.) ...
    public Transform objetivoJugador; 
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;
    public float fuerzaDisparo = 500f;
    public float tiempoEntreDisparos = 2f;

    private float siguienteDisparo;

    void Update()
    {
        // 1. APUNTAR al jugador (¡Ahora en 3D!)
        if (objetivoJugador != null)
        {
            // Calcula la dirección completa (3D) hacia el jugador
            Vector3 direccion = objetivoJugador.position - transform.position;
            
            // ELIMINA O COMENTA ESTA LÍNEA para permitir el movimiento vertical
            // direccion.y = 0; // ANTES: Esto limitaba la rotación a solo el eje Y (horizontal)

            // Calcula la rotación necesaria para mirar en esa dirección
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            
            // Rota el cañón suavemente hacia el objetivo
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
        }

        // 2. DISPARAR automáticamente (Esta parte no cambia)
        if (Time.time > siguienteDisparo)
        {
            Disparar();
            siguienteDisparo = Time.time + tiempoEntreDisparos;
        }
    }

    void Disparar()
    {
        // ... (El código de Disparar no necesita cambios)
        GameObject nuevaBala = Instantiate(proyectilPrefab, puntoDisparo.position, puntoDisparo.rotation);
        Rigidbody rb = nuevaBala.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(puntoDisparo.forward * fuerzaDisparo);
        }
    }
}