// Assets/Scripts/ButtonSequenceManager.cs
using UnityEngine;

public class TutorialButtonSequenceManager : MonoBehaviour
{
    [Header("Botones de secuencia (en orden)")]
    public TutorialFloorButton[] sequenceButtons;      // floorbutton(2), floorbutton(5), etc.

    [Header("Botón de completar")]
    public TutorialFloorButton completerButton;        // el botón "Completer"

    private int currentIndex = 0;

    private void Start()
    {
        // Inicializar referencias desde manager -> botones
        foreach (var b in sequenceButtons)
        {
            if (b != null)
            {
                b.Init(this);
                b.SetActive(false);           // apagados de inicio
            }
        }

        if (completerButton != null)
        {
            completerButton.Init(this);
            completerButton.SetActive(false); // apagado de inicio
        }

        // Activar el primer botón de la secuencia
        if (sequenceButtons.Length > 0 && sequenceButtons[0] != null)
        {
            currentIndex = 0;
            sequenceButtons[0].SetActive(true);   // primer GOLD
        }
    }

    public void OnButtonPressed(TutorialFloorButton button)
    {
        if (button == null) return;

        // 1) Si es un botón de la secuencia
        bool isSequenceButton = System.Array.IndexOf(sequenceButtons, button) >= 0;

        if (isSequenceButton && button == sequenceButtons[currentIndex])
        {
            // Solo reacciona si es el botón ACTUAL y está activo
            if (!button.IsActive) return;

            // Apagar el botón actual
            button.SetActive(false);

            // Encender el Completer
            if (completerButton != null)
            {
                completerButton.SetActive(true);
            }

            return;
        }

        // 2) Si es el botón Completer
        if (button == completerButton)
        {
            // Solo si estaba activo
            if (!completerButton.IsActive) return;

            Debug.Log("Se completo el tutorial");

            // Apagar el Completer
            completerButton.SetActive(false);

            if (sequenceButtons.Length == 0)
                return;

            // Avanzar al siguiente botón (cíclicamente)
            currentIndex = (currentIndex + 1) % sequenceButtons.Length;

            // Encender el siguiente botón
            var nextButton = sequenceButtons[currentIndex];
            if (nextButton != null)
            {
                nextButton.SetActive(true);
            }

            return;
        }

        // 3) Cualquier otro botón no hace nada por ahora
    }
}
