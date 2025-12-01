using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour
{
    [Header("Configuración")]
    public float retraso = 0f; // Retraso antes del reset (opcional)
    
    // Método para llamar desde un botón o evento
    public void ResetearEscena()
    {
        if (retraso <= 0)
        {
            // Reset inmediato
            ReiniciarEscenaActual();
        }
        else
        {
            // Reset con retraso
            Invoke(nameof(ReiniciarEscenaActual), retraso);
        }
        
        Debug.Log("Reiniciando escena...");
    }
    
    private void ReiniciarEscenaActual()
    {
        // Obtiene el índice de la escena actual y la vuelve a cargar
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.buildIndex);
    }
}