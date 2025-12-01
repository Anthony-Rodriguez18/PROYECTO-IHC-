using TMPro;
using UnityEngine;

public class VidaUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoVida;
    public GameObject imagenGameOver;
    
    [Header("Referencias de Salud")]
    public PlayerHealth playerHealth;
    
    private int vidaActual;

    void Start()
    {
        // Desactivar imagen de Game Over al inicio
        if (imagenGameOver != null)
            imagenGameOver.SetActive(false);
        
        // ASEGURAR que el texto esté activo y visible
        if (textoVida != null)
        {
            textoVida.gameObject.SetActive(true);
            textoVida.enabled = true;
        }
            
        // Obtener vida inicial
        if (playerHealth != null)
        {
            vidaActual = playerHealth.vidaActual;
            ActualizarTextoVida();
            Debug.Log("Vida inicial: " + vidaActual);
        }
        else
        {
            Debug.LogError("PlayerHealth no está asignado!");
        }
    }

    void Update()
    {
        // Actualizar continuamente la vida
        if (playerHealth != null && playerHealth.vidaActual != vidaActual)
        {
            vidaActual = playerHealth.vidaActual;
            ActualizarTextoVida();
            
            // Verificar game over
            if (vidaActual <= 0)
            {
                MostrarGameOver();
            }
        }
    }
    
    void ActualizarTextoVida()
    {
        if (textoVida != null)
        {
            textoVida.text = "Vida: " + vidaActual;
            Debug.Log("Texto actualizado: " + textoVida.text);
            
            // Opcional: Cambiar color según vida
            if (vidaActual <= 3)
                textoVida.color = Color.red;
            else if (vidaActual <= 5)
                textoVida.color = Color.yellow;
            else
                textoVida.color = Color.white;
        }
        else
        {
            Debug.LogError("textoVida no está asignado!");
        }
    }
    
    void MostrarGameOver()
    {
        // CAMBIAR el texto en lugar de ocultarlo
        if (textoVida != null)
        {
            textoVida.text = "Haz pinch en el botón de atrás para reiniciar";
            textoVida.color = Color.white; // Color para el mensaje
            textoVida.fontSize = 20; // Opcional: ajustar tamaño de fuente
        }
        
        // Activar la imagen de Game Over
        if (imagenGameOver != null)
        {
            imagenGameOver.SetActive(true);
            Debug.Log("Game Over - Imagen activada");
        }
    }
}