using System.Collections;
using System.Diagnostics;
using UnityEngine;
using TMPro; // <-- necesario para TextMeshPro


public class PlayerRespawn : MonoBehaviour
{
    [Header("Suelo que mata")]
    public string floorTag = "MainFloor";

    
    [Header("Camara")]
    public Vector3 firstRespawnCamPosition;

    [Header("Respawn")]
    public float respawnDelay = 3f;
    public Transform respawnPoint;

    [Header("Efecto visual")]
    public Color hitColor = Color.red;

    [Header("Texto de Tutorial")]
    public TextMeshProUGUI tutorialText;
    public GameObject tutorialPanel;


    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private SkinnedMeshRenderer meshRenderer;
    private Color originalColor;

    private bool isRespawning = false;
    private bool firstRespawnDone = false;
    private Transform mainCameraTransform;

    void Start()
    {

        if (respawnPoint != null)
        {
            initialPosition = new Vector3(-3.5f,0f,20.5f);
            initialRotation = respawnPoint.rotation;
        }
        else
        {
            initialPosition = new Vector3(-3.5f,0f,20.5f);
            initialRotation = transform.rotation;
        }

        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;

        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;

       ShowTutorial(
            "<align=center><size=36><b>Bienvenido al tutorial del jugador 2</b></size></align>\n\n" +
            "Para moverte usa <color=#00D1FF><b>W A S D</b></color>.\n\n" +
            "Pisa el botón <color=#FFD500><b>amarillo pequeño</b></color> para activar el más grande.\n\n" +
            "Eso enviará una <color=#FF4A4A><b>espada</b></color> al gigante.\n\n" +
            "Para comenzar la partida, <b>muere por el cañón</b>.",
            12f
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isRespawning)
            return;

        if (collision.collider.CompareTag(floorTag))
            StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Llamado desde trampas, balas, etc. para "matar" al jugador.
    /// </summary>
    public void KillPlayer()
    {
        if (isRespawning)
            return;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        PlayerMovement controller = GetComponent<PlayerMovement>();
        if (controller != null)
            controller.enabled = false;

        if (meshRenderer != null)
            meshRenderer.material.color = hitColor;

        yield return new WaitForSeconds(respawnDelay);

        // Teletransportar jugador
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        // ⭐ Aplicar efecto solo en el primer respawn
        if (!firstRespawnDone && mainCameraTransform != null)
        {
            mainCameraTransform.localPosition = firstRespawnCamPosition;
            firstRespawnDone = true;
        }

        if (meshRenderer != null)
            meshRenderer.material.color = originalColor;

        if (controller != null)
            controller.enabled = true;

        isRespawning = false;
    }

    public void ShowTutorial(string message, float duration = 10f)
{
    if (tutorialText == null)
        return;

    tutorialText.text = message;
    tutorialText.gameObject.SetActive(true);
    tutorialPanel.SetActive(true);

    // Quitar el texto luego de X segundos
    StartCoroutine(HideTutorialAfter(duration));
}

    private IEnumerator HideTutorialAfter(float time)
    {
        yield return new WaitForSeconds(time);
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);
            tutorialPanel.SetActive(false);
    }



}
