using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int vidaMaxima = 10;
    public int vidaActual;
    
    [Header("Game Over")]
    // Reemplazamos el arreglo por cañones específicos
    public GameObject canon1;
    public GameObject canon2;
    public GameObject canon3;
    public GameObject panelGameOver;
    
    [Header("Efectos")]
    public AudioClip sonidoGolpe;
    public AudioClip sonidoGameOver;
    public GameObject efectoDano;
    public float tiempoInvulnerabilidad = 0.5f;
    
    private bool estaInvulnerable = false;
    private AudioSource audioSource;
    
    void Start()
    {
        vidaActual = vidaMaxima;
        
        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoGolpe != null || sonidoGameOver != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // Sonido 2D para el jugador
        }
        
        // Ocultar panel de Game Over si existe
        if (panelGameOver != null)
            panelGameOver.SetActive(false);
            
        Debug.Log("Vida del jugador iniciada: " + vidaActual);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bala") && !estaInvulnerable)
        {
            RecibirDano(1);
            Debug.Log("¡Impacto de bala detectado! Vida restante: " + vidaActual);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bala") && !estaInvulnerable)
        {
            RecibirDano(1);
            Debug.Log("¡Impacto de bala (trigger) detectado! Vida restante: " + vidaActual);
        }
    }
    
    public void RecibirDano(int cantidad)
    {
        if (estaInvulnerable) return;
        
        vidaActual -= cantidad;
        
        // Reproducir sonido de golpe
        if (audioSource != null && sonidoGolpe != null)
        {
            audioSource.PlayOneShot(sonidoGolpe);
        }
        
        // Mostrar efecto de daño
        if (efectoDano != null)
        {
            Instantiate(efectoDano, transform.position, Quaternion.identity);
        }
        
        // Hacer invulnerable brevemente
        StartCoroutine(PeriodoInvulnerabilidad());
        
        // Comprobar Game Over
        if (vidaActual <= 0)
        {
            GameOver();
        }
    }
    
    IEnumerator PeriodoInvulnerabilidad()
    {
        estaInvulnerable = true;
        yield return new WaitForSeconds(tiempoInvulnerabilidad);
        estaInvulnerable = false;
    }
    
    void GameOver()
    {
        // Reproducir sonido de game over
        if (audioSource != null && sonidoGameOver != null)
        {
            audioSource.PlayOneShot(sonidoGameOver);
        }
        
        // Desactivar cada cañón individualmente
        if (canon1 != null) 
        {
            canon1.SetActive(false);
            Debug.Log("Cañón 1 desactivado");
        }
        
        if (canon2 != null) 
        {
            canon2.SetActive(false);
            Debug.Log("Cañón 2 desactivado");
        }
        
        if (canon3 != null) 
        {
            canon3.SetActive(false);
            Debug.Log("Cañón 3 desactivado");
        }
        
        // Mostrar panel de Game Over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
        
        Debug.Log("¡GAME OVER! Vida reducida a 0");
    }
    
    // Método opcional para interfaces o botones
    public void ResetearVida()
    {
        vidaActual = vidaMaxima;
        
        // Reactivar cañones individualmente
        if (canon1 != null) canon1.SetActive(true);
        if (canon2 != null) canon2.SetActive(true);
        if (canon3 != null) canon3.SetActive(true);
        
        // Ocultar panel Game Over
        if (panelGameOver != null)
            panelGameOver.SetActive(false);
            
        Debug.Log("Vida restaurada y cañones reactivados");
    }
}