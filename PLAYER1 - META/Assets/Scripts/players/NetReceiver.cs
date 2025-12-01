// Assets/Scripts/players/NetReceiver.cs
using System;
using System.Text;
using NativeWebSocket;
using UnityEngine;

[Serializable]
public class TransformMsg
{
    public string type, id, role;
    public double t;
    public float px, py, pz;
    public float rx, ry, rz;
}

public class NetReceiver : MonoBehaviour  // <--- coincide con el nombre del archivo
{
    [Header("Connection")]
    [SerializeField] private string serverUrl = "ws://192.168.137.1:5000";
    [SerializeField] private string myRole = "meta";

    [Header("Apply to")]
    [SerializeField] private Transform target;          // objeto que se mueve
    [SerializeField] private string watchRole = "wizard";
    [SerializeField] private bool applyRotation = true;
    [SerializeField] private float smoothPos = 15f;
    [SerializeField] private float smoothRot = 15f;

    private WebSocket ws;
    private Vector3 goalPos, goalEuler;
    private bool initialized;

    private async void Start()
    {
        if (target == null) target = transform;
        goalPos = target.position;
        goalEuler = target.eulerAngles;

        ws = new WebSocket(serverUrl);

        ws.OnOpen += async () => {
            Debug.Log("[WS] Receiver connected");
            await ws.SendText($"{{\"type\":\"hello\",\"id\":\"{SystemInfo.deviceUniqueIdentifier}\",\"role\":\"{myRole}\"}}");
        };
        ws.OnError += e => Debug.LogWarning("[WS] Receiver error: " + e);
        ws.OnClose += c => Debug.Log("[WS] Receiver closed: " + c);

        ws.OnMessage += bytes => {
            var json = Encoding.UTF8.GetString(bytes);
            try
            {
                var msg = JsonUtility.FromJson<TransformMsg>(json);
                if (msg != null && msg.type == "transform" && msg.role == watchRole)
                {
                    goalPos = new Vector3(msg.px, msg.py, msg.pz);
                    goalEuler = new Vector3(msg.rx, msg.ry, msg.rz);
                    initialized = true;
                }
            }
            catch { /* ignorar */ }
        };

        await ws.Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
        if (!initialized) return;

        target.position = Vector3.Lerp(target.position, goalPos, Time.deltaTime * smoothPos);
        if (applyRotation)
        {
            var q = Quaternion.Euler(goalEuler);
            target.rotation = Quaternion.Slerp(target.rotation, q, Time.deltaTime * smoothRot);
        }
    }

    private async void OnApplicationQuit()
    {
        if (ws != null) await ws.Close();
    }
}
