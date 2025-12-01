using System;
using System.Text;
using NativeWebSocket;
using UnityEngine;

public class WsCubeReceiver : MonoBehaviour
{
    [Header("Red")]
    public string serverUrl = "ws://192.168.137.1:5000"; // IP donde corre server.py
    public string entityId  = "cube_1";

    [Header("Objeto a actualizar")]
    public Transform target;           // arrastra aquí el Cube
    [Range(0f,1f)] public float posLerp = 0.25f;
    [Range(0f,1f)] public float rotLerp = 0.25f;

    private WebSocket ws;
    private Vector3 tPos;
    private Quaternion tRot;

    [Serializable]
    private class StateMsg {
        public string t;
        public string id;
        public float x,y,z;
        public float rx,ry,rz,rw;
    }

    private async void Start()
    {
        if (target == null) target = this.transform;
        tPos = target.position;
        tRot = target.rotation;

        ws = new WebSocket(serverUrl);

        ws.OnOpen  += () => Debug.Log("[WS] abierto (viewer)");
        ws.OnError += (e) => Debug.LogError("[WS] error: " + e);
        ws.OnClose += (c) => Debug.Log("[WS] cerrado: " + c);

        ws.OnMessage += (bytes) =>
        {
            var msg = Encoding.UTF8.GetString(bytes);
            try {
                var data = JsonUtility.FromJson<StateMsg>(msg);
                if (data.t == "state" && data.id == entityId)
                {
                    tPos = new Vector3(data.x, data.y, data.z);
                    tRot = new Quaternion(data.rx, data.ry, data.rz, data.rw);
                }
            } catch { /* ignora basura */ }
        };

        try { await ws.Connect(); }
        catch (Exception e) { Debug.LogWarning("[WS] no conecta: " + e.Message); }
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
        if (target == null) return;

        target.position = Vector3.Lerp(target.position, tPos, posLerp);
        target.rotation = Quaternion.Slerp(target.rotation, tRot, rotLerp);
    }

    private async void OnApplicationQuit()
    {
        if (ws != null && ws.State == WebSocketState.Open)
            await ws.Close();
    }
}