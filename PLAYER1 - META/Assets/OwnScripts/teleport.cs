using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class TeleportController : MonoBehaviour
{
    [Header("Teleport References")]
    public Transform playerRig;                // El rig de Meta Building Blocks (XR Origin)
    public Transform destinationPoint;         // GameObject con posición/rotación destino

    [Header("Game Objects")]
    public GameObject tutorialGameObject;      // GameObject del tutorial (se desactivará)
    public GameObject gameGameObject;          // GameObject del juego (se activará)
    
    [Header("Objetos Adicionales para Activar")]
    public GameObject objetoAdicional1;        // Primer objeto adicional para activar
    public GameObject objetoAdicional2;        // Segundo objeto adicional para activar
    public GameObject objetoAdicional3;        // Tercer objeto adicional para activar
    public float retrasoActivacion = 10f;      // NUEVO: Tiempo de espera en segundos

    [Header("Options")]
    public bool teleportRotation = true;       // Si true, también copia la rotación
    public bool teleportOnStart = false;       // Si true, teleporta automáticamente al inicio

    void Start()
    {
        // Si queremos teleportar al iniciar la escena
        if (teleportOnStart)
        {
            Teleport();
        }
    }

    // Método principal que se llamará desde el botón/trigger
    public void Teleport()
    {
        // 1. Teleportar el jugador a la posición destino
        if (playerRig != null && destinationPoint != null)
        {
            // Teleportar posición
            playerRig.position = destinationPoint.position;
            
            // Teleportar rotación (opcional)
            if (teleportRotation)
            {
                playerRig.rotation = destinationPoint.rotation;
            }
            
            Debug.Log("Jugador teleportado a: " + destinationPoint.position);
        }
        else
        {
            Debug.LogError("Error: Falta asignar playerRig o destinationPoint");
        }

        // 2. Cambiar GameObjects activos (inmediato)
        if (tutorialGameObject != null)
        {
            tutorialGameObject.SetActive(false);
            Debug.Log("Tutorial desactivado");
        }

        if (gameGameObject != null)
        {
            gameGameObject.SetActive(true);
            Debug.Log("Juego activado");
        }
        
        // 3. NUEVO: Activar los objetos adicionales con retraso
        StartCoroutine(ActivarObjetosConRetraso());
    }

    // NUEVO: Corrutina para activar objetos después del retraso
    private IEnumerator ActivarObjetosConRetraso()
    {
        Debug.Log($"Objetos adicionales se activarán en {retrasoActivacion} segundos...");
        
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(retrasoActivacion);
        
        // Activar objetos adicionales
        if (objetoAdicional1 != null)
        {
            objetoAdicional1.SetActive(true);
            Debug.Log("Objeto Adicional 1 activado");
        }
        
        if (objetoAdicional2 != null)
        {
            objetoAdicional2.SetActive(true);
            Debug.Log("Objeto Adicional 2 activado");
        }
        
        if (objetoAdicional3 != null)
        {
            objetoAdicional3.SetActive(true);
            Debug.Log("Objeto Adicional 3 activado");
        }
        
        Debug.Log("Todos los objetos adicionales han sido activados");
    }
}