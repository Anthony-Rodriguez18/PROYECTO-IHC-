using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;
    public float duration = 5f;

    private Coroutine currentRoutine;

    public void Show(string msg)
    {
        messageText.text = msg;

        panel.SetActive(true);

        // Reiniciar el temporizador si ya había uno
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(duration);
        panel.SetActive(false);
        currentRoutine = null;
    }
}