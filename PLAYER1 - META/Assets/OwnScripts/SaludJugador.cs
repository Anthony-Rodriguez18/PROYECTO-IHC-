using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena

public class SaludJugador : MonoBehaviour
{
    // VARIABLES
    public int vidaActual = 5; 
    public int vidaMaxima = 5;
    
    // Nueva variable pública: ¡ARRÁSTRALE el Canvas de Fin del Juego aquí!
    public GameObject mensajeFinJuego; 

    // ... (La función RecibirDano queda igual) ...
    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano; 
        Debug.Log("¡Impacto! Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log("El jugador ha muerto. Fin del juego.");
        
        // 1. Mostrar el mensaje de Fin del Juego
        if (mensajeFinJuego != null)
        {
            mensajeFinJuego.SetActive(true); // Activa el Canvas
        }

        // 2. Detener el juego (o reiniciar la escena)
        // Opción A: Detener el tiempo para que todo se congele
        // Time.timeScale = 0f; 
        
        // Opción B: Reiniciar la escena después de 3 segundos
        Invoke("ReiniciarJuego", 3f);
    }
    
    void ReiniciarJuego()
    {
        // Carga la escena actual de nuevo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}