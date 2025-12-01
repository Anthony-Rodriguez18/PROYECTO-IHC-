using System.Text;
using UnityEngine;
using NativeWebSocket;

[System.Serializable]
public class TransformMsgOut
{
    public string type = "transform";
    public string id;
    public string role = "wizard"; // <- este es el que filtra tu NetReceiver (watchRole)
    public float t;

    // posición
    public float px, py, pz;

    // rotación en EULER (porque tu NetReceiver consume rx,ry,rz como Euler)
    public float rx, ry, rz;
}

public class NetSender : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private string serverUrl = "ws://localhost:5000";
    [SerializeField] private string myRole = "wizard";
    [SerializeField] private float sendRate = 0.05f; // 20 Hz

    private WebSocket ws;
    private float timer;
    private string myId;

    async void Start()
    {
        myId = SystemInfo.deviceUniqueIdentifier;
        ws = new WebSocket(serverUrl);

        ws.OnOpen += async () => {
            Debug.Log("[WS] Sender connected");
            string hello = $"{{\"type\":\"hello\",\"id\":\"{myId}\",\"role\":\"{myRole}\"}}";
            await ws.SendText(hello);
        };

        ws.OnError += e => Debug.LogWarning("[WS] Sender error: " + e);
        ws.OnClose += c => Debug.Log("[WS] Sender closed: " + c);

        await ws.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif

        timer += Time.deltaTime;
        if (timer >= sendRate)
        {
            timer = 0f;
            SendTransform();
        }
    }

    async void SendTransform()
    {
        if (ws == null || ws.State != WebSocketState.Open) return;

        Vector3 p = transform.position;
        Vector3 e = transform.eulerAngles;

        TransformMsgOut msg = new TransformMsgOut
        {
            id = myId,
            role = myRole,     // "wizard"
            t = Time.time,
            px = p.x,
            py = p.y,
            pz = p.z,
            rx = e.x,
            ry = e.y,
            rz = e.z     // EULER porque tu receptor espera Euler
        };

        string json = JsonUtility.ToJson(msg);
        await ws.SendText(json);
    }

    async void OnApplicationQuit()
    {
        if (ws != null) await ws.Close();
    }
}
