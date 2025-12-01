using NativeWebSocket;
using System;
using UnityEngine;

public class WizardOnlineManager : MonoBehaviour
{
    private WebSocket ws;
    private bool helloSent = false;

    [Header("Config")]
    public string serverUrl = "ws://192.168.137.1:5000";

    [Header("UI")]
    public NotificationUI notificationUI;   // ← referencia al script de la notificación

    [System.Serializable]
    public class HelloMessage
    {
        public string type;
        public string role;
        public string id;
    }

    [System.Serializable]
    public class SpawnItemMessage
    {
        public string type;
        public string item;
        public string target;
        public string from;
        public string role;
        public string id;
        public double t;
    }

    async void Start()
    {
        ws = new WebSocket(serverUrl);

        ws.OnOpen += () =>
        {
            Debug.Log("🧙‍♂️ Mago conectado al server WebSocket");
        };

        ws.OnError += (e) =>
        {
            Debug.LogError("WS error (mago): " + e);
        };

        ws.OnClose += (e) =>
        {
            Debug.LogWarning("WS cerrado (mago): " + e);
            helloSent = false;
        };

        ws.OnMessage += (bytes) =>
        {
            string msg = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("🧙‍♂️ Mago recibió: " + msg);

            HandleServerMessage(msg);   // ← NUEVO
        };

        await ws.Connect();
    }

    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
    #endif

        // Enviar HELLO una sola vez cuando esté conectado
        if (ws != null && ws.State == WebSocketState.Open && !helloSent)
        {
            var hello = new HelloMessage
            {
                type = "hello",
                role = "mago",
                id   = "mago_pc"
            };

            string json = JsonUtility.ToJson(hello);
            ws.SendText(json);
            helloSent = true;

            Debug.Log("🧙‍♂️ HELLO enviado desde mago: " + json);
        }
    }

    /// <summary>
    /// Llamado por los botones Completer / Completer (1).
    /// Envía una ESPADA al gigante (Meta).
    /// </summary>
    public async void SendSwordToGiant()
    {
        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("WS no está abierto (mago), no se puede enviar espada.");
            return;
        }

        var payload = new SpawnItemMessage
        {
            type   = "spawn_item",
            item   = "espada",
            target = "meta",      // llega solo al cliente con role="meta"
            from   = "mago",
            role   = "mago",
            id     = "mago_pc",
            t      = Time.time
        };

        string json = JsonUtility.ToJson(payload);
        await ws.SendText(json);
        Debug.Log("🧙‍♂️ Mago envió ESPADA al gigante: " + json);
    }

    // ---------------- MENSAJES DEL SERVER ----------------

    private void HandleServerMessage(string msg)
    {
        // Intentamos parsear como SpawnItemMessage
        try
        {
            var data = JsonUtility.FromJson<SpawnItemMessage>(msg);

            // Si no tiene type, es probablemente otro tipo de mensaje (hello, etc.)
            if (string.IsNullOrEmpty(data.type))
                return;

            // Queremos: mensajes que vienen de voz, piden espada y van hacia el mago
            if (data.type == "spawn_item" &&
                data.item == "espada" &&
                data.target == "mago")
            {
                Debug.Log("📩 Notificación: tu compañero (voz) pidió una ESPADA");

                if (notificationUI != null)
                {
                    notificationUI.Show("Tu compañero necesita espada ⚔️");
                }
                else
                {
                    Debug.LogWarning("NotificationUI no asignado en WizardOnlineManager.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo parsear mensaje del server: " + e.Message);
        }
    }

    private async void OnApplicationQuit()
    {
        if (ws != null)
            await ws.Close();
    }
}