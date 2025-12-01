using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OnlineSwordButton : MonoBehaviour
{
    [Header("Referencia al manager online del mago")]
    public WizardOnlineManager onlineManager;

    [Header("Botón del juego (laberinto grande)")]
    public FloorButton floorButton;

    [Header("Botón del tutorial")]
    public TutorialFloorButton tutorialFloorButton;

    // Interno: indica si este botón estuvo activo (amarillo)
    // y está "armado" para mandar una espada en el próximo pisón
    private bool armed = false;

    private void Awake()
    {
        // Autodetectar si no lo asignas en el inspector
        if (floorButton == null)
            floorButton = GetComponent<FloorButton>();

        if (tutorialFloorButton == null)
            tutorialFloorButton = GetComponent<TutorialFloorButton>();
    }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        bool isActive = false;

        if (floorButton != null)
            isActive |= floorButton.IsActive;          // modo juego

        if (tutorialFloorButton != null)
            isActive |= tutorialFloorButton.IsActive;  // modo tutorial

        // Si en algún momento está activo, lo "armamos"
        if (isActive)
            armed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;   // solo el mago

        // 1) Si nunca estuvo activo antes, no mandamos nada
        if (!armed)
        {
            Debug.Log("🧙‍♂️ Botón pisado pero NO armado/activo recientemente, no se envía espada: " + gameObject.name);
            return;
        }

        // 2) Check manager
        if (onlineManager == null)
        {
            Debug.LogWarning("OnlineSwordButton sin referencia a WizardOnlineManager.");
            return;
        }

        // 3) Consumimos el "arma": solo una espada por activación
        armed = false;

        Debug.Log("🧙‍♂️ Botón online ARMADO pisado, enviando espada: " + gameObject.name);
        onlineManager.SendSwordToGiant();
    }
}