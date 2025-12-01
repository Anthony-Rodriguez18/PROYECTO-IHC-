// Assets/Scripts/FloorButton.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialFloorButton : MonoBehaviour
{
    [Header("Visual")]
    public Renderer rend;                  // Renderer del mesh del botón
    public Color offColor = Color.black;   // Color apagado
    public Color activeColor = Color.yellow; // Color GOLD / activo

    [HideInInspector]
    public TutorialButtonSequenceManager manager;  // Lo setea el manager

    private bool isActive = false;
    public bool IsActive => isActive;

    private void Reset()
    {
        // Auto-configurar cosas útiles cuando se añada el script
        rend = GetComponent<Renderer>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;              // Botón tipo trigger
    }

    public void Init(TutorialButtonSequenceManager mgr)
    {
        manager = mgr;
        UpdateVisual();
    }

    public void SetActive(bool value)
    {
        isActive = value;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (rend != null)
        {
            rend.material.color = isActive ? activeColor : offColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (manager != null)
        {
            manager.OnButtonPressed(this);
        }
    }
}
