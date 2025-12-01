// Assets/Scripts/Network/NetTransformSender.cs
using System;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class NetTransformSender : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private string serverUrl = "ws://localhost:5000"; // <-- IP del server
    [SerializeField] private string id = "wizard1";
    [SerializeField] private string role = "wizard";

    [Header("Target to send")]
    [SerializeField] private Transform target;          // si null, usa this.transform
    [SerializeField] private float sendHz = 20f;        // tasa de envío
    [SerializeField] private bool sendRotation = true;

    private WebSocket ws;
    private float _nextSend;
    private static readonly CultureInfo CI = CultureInfo.InvariantCulture;

    private async void Start()
    {
        if (target == null) target = transform;

        ws = new WebSocket(serverUrl);
        ws.OnOpen += () => Debug.Log("[WS] Sender connected");
        ws.OnError += (e) => Debug.LogWarning("[WS] Sender error: " + e);
        ws.OnClose += (c) => Debug.Log("[WS] Sender closed: " + c);

        await ws.Connect();

        // saludo opcional (útil para logs del server)
        await SafeSend($"{{\"type\":\"hello\",\"id\":\"{id}\",\"role\":\"{role}\"}}");
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
        if (ws == null || ws.State != WebSocketState.Open) return;

        if (Time.time >= _nextSend)
        {
            _nextSend = Time.time + (1f / Mathf.Max(1f, sendHz));
            Vector3 p = target.position;
            Vector3 r = sendRotation ? target.eulerAngles : Vector3.zero;
            double t = Time.timeAsDouble;

            // JSON manual, evitando problemas de coma decimal
            string msg = string.Format(CI,
                "{{\"type\":\"transform\",\"id\":\"{0}\",\"role\":\"{1}\",\"t\":{2}," +
                "\"px\":{3},\"py\":{4},\"pz\":{5},\"rx\":{6},\"ry\":{7},\"rz\":{8}}}",
                id, role, t, p.x, p.y, p.z, r.x, r.y, r.z);

            _ = SafeSend(msg);
        }
    }

    private async Task SafeSend(string s)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            await ws.Send(bytes);
        }
    }

    private async void OnApplicationQuit()
    {
        if (ws != null) await ws.Close();
    }
}
