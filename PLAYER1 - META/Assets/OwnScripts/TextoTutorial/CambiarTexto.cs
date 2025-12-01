using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CambiarTexto : MonoBehaviour
{
    public TextMeshProUGUI miTexto;
    public List<GameObject> imagenes = new List<GameObject>();


    // Lista de mensajes para cambiar
    private string[] mensajes = new string[]
    {
        "¡BIENVENIDO A PC VS VR! \n Para continuar has el gesto de pellizcar sobre el cubo derecho",
        "¿DESEAS RETROCEDER? \n Haz el gesto de pellizcar sobre el cubo izquierdo",
        "Invoca tu escudo y espada gritando 'Escudo y Espada'",
        "Sujeta tu escudo con la mano izquierda y tu espada con la mano derecha",
        "Usalos para defenderte",
        "MIRA ATRAS TUYO Y HAZ PINCH PARA INICIAR"
    };
    private int indiceMensaje = 0;

    void Start()
    {
        // Asegurar índice inicial y mostrar el primer mensaje
        if (mensajes == null || mensajes.Length == 0) return;
        indiceMensaje = 0;
        if (miTexto != null) miTexto.text = mensajes[indiceMensaje];

        // Activar solo el GameObject correspondiente al primer mensaje
        ActivarImagenParaIndice(indiceMensaje);
    }

    private void ActivarImagenParaIndice(int index)
    {
        if (imagenes == null || imagenes.Count == 0) return;
        for (int i = 0; i < imagenes.Count; i++)
        {
            if (imagenes[i] != null)
                imagenes[i].SetActive(i == index);
        }
    }

    public void CambiarTextoAMensaje()
    {
        if (miTexto == null || mensajes == null || mensajes.Length == 0) return;
        miTexto.text = mensajes[indiceMensaje];
        indiceMensaje = (indiceMensaje + 1) % mensajes.Length;
        ActivarImagenParaIndice(indiceMensaje);
    }

    public void RetrocederTexto()
    {
        if (miTexto == null || mensajes == null || mensajes.Length == 0) return;
        indiceMensaje = (indiceMensaje - 1 + mensajes.Length) % mensajes.Length;
        if (miTexto != null) miTexto.text = mensajes[indiceMensaje];
        ActivarImagenParaIndice(indiceMensaje);
    }
}
