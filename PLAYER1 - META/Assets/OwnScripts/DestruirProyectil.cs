using UnityEngine;

public class DestruirProyectil : MonoBehaviour
{
    public float tiempoDeVida = 5f; 
    public int danoInfligido = 1; // 🎯 Cada proyectil quita 1 punto de vida.

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }
    
    // Esta función se llama al chocar con otro objeto
    void OnCollisionEnter(Collision collision)
    {
        // 1. Intentamos obtener el script SaludJugador en el objeto chocado o sus padres.
        // Usamos GetComponentInParent porque el Collider del jugador podría estar en un hijo 
        // o en el objeto raíz (OVRCameraRig).
        SaludJugador salud = collision.gameObject.GetComponentInParent<SaludJugador>();

        // 2. Si encontramos el script (es decir, chocamos con el jugador)
        if (salud != null)
        {
            // Llamamos a la función de daño
            salud.RecibirDano(danoInfligido);
        }

        // 3. El proyectil se destruye, haya o no golpeado al jugador.
        Destroy(gameObject);
    }
}