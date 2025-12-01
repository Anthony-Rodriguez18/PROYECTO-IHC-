using System;
using System.Text;
using NativeWebSocket;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetEntitySync : MonoBehaviour
{
    [Header("Red")]
    public string serverUrl = "ws://192.168.137.1:5000";
    public bool isDriver = false;            // true = mueve y envía; false = solo recibe
    [Tooltip("Identificador lógico del objeto. Debe coincidir en A y B.")]
    public string entityId = "cube_1";
    public float sendHz = 10f;

    [Header("Objetivo a mover / leer")]
    [Tooltip("Objeto que se moverá localmente (driver) o se actualizará (viewer). Si está vacío, usa este GameObject.")]
    public Transform target;

    [Header("Movimiento (driver)")]
    public float speed = 3f;
    public float rotateDegreesPerSec = 90f;

    // Estado remoto (viewer)
    private Vector3 targetPos;
    private Quaternion targetRot;

    // WS
    private WebSocket ws;
    private float sendTimer;

    // Input System (sin .inputactions, todo por código)
    private InputAction moveAction;       // Vector2 WASD / stick
    private InputAction rotLeftAction;    // Q / LB
    private InputAction rotRightAction;   // E / RB

    [Serializable]
    private class StateMsg
    {
        public string t;   // "state"
        public string id;  // entityId
        public float x, y, z;
        public float rx, ry, rz, rw;
    }

    private void Awake()
    {
        if (target == null) target = this.transform;
        targetPos = target.position;
        targetRot = target.rotation;

        // ==== Input Actions ====
        moveAction = new InputAction(name: "Move", type: InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddBinding("<Gamepad>/leftStick");

        rotLeftAction = new InputAction("RotateLeft", InputActionType.Button);
        rotLeftAction.AddBinding("<Keyboard>/q");
        rotLeftAction.AddBinding("<Gamepad>/leftShoulder");

        rotRightAction = new InputAction("RotateRight", InputActionType.Button);
        rotRightAction.AddBinding("<Keyboard>/e");
        rotRightAction.AddBinding("<Gamepad>/rightShoulder");
    }

    private async void Start()
    {
        Application.runInBackground = true;

        ws = new WebSocket(serverUrl);

        ws.OnOpen += () => Debug.Log("[WS] abierto");
        ws.OnError += (e) => Debug.LogError("[WS] error: " + e);
        ws.OnClose += (c) => Debug.Log("[WS] cerrado: " + c);

        ws.OnMessage += (bytes) =>
        {
            var msg = Encoding.UTF8.GetString(bytes);
            try
            {
                var data = JsonUtility.FromJson<StateMsg>(msg);
                if (data.t == "state" && data.id == entityId && !isDriver)
                {
                    targetPos = new Vector3(data.x, data.y, data.z);
                    targetRot = new Quaternion(data.rx, data.ry, data.rz, data.rw);
                }
            }
            catch { /* ignorar lo que no sea nuestro JSON */ }
        };

        try
        {
            await ws.Connect();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[WS] No se pudo conectar: " + e.Message);
        }
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        rotLeftAction?.Enable();
        rotRightAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        rotLeftAction?.Disable();
        rotRightAction?.Disable();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif

        if (isDriver)
        {
            // Input System nuevo
            Vector2 input = moveAction.ReadValue<Vector2>(); // (-1..1, -1..1)

            // Mover en XZ
            Vector3 dir = new Vector3(input.x, 0f, input.y).normalized;
            target.position += dir * speed * Time.deltaTime;

            // Rotar con Q/E (o LB/RB)
            float rot = 0f;
            if (rotLeftAction.IsPressed()) rot -= rotateDegreesPerSec * Time.deltaTime;
            if (rotRightAction.IsPressed()) rot += rotateDegreesPerSec * Time.deltaTime;
            if (Mathf.Abs(rot) > 0.0001f)
                target.Rotate(0f, rot, 0f, Space.World);

            // Enviar estado a una cadencia fija
            sendTimer += Time.deltaTime;
            if (sendTimer >= 1f / sendHz)
            {
                sendTimer = 0f;
                SendState(target.position, target.rotation);
            }
        }
        else
        {
            // Suavizado para el receptor
            target.position = Vector3.Lerp(target.position, targetPos, 0.25f);
            target.rotation = Quaternion.Slerp(target.rotation, targetRot, 0.25f);
        }
    }

    private async void SendState(Vector3 p, Quaternion q)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;

        var msg = new StateMsg
        {
            t = "state",
            id = entityId,
            x = p.x,
            y = p.y,
            z = p.z,
            rx = q.x,
            ry = q.y,
            rz = q.z,
            rw = q.w
        };

        string json = JsonUtility.ToJson(msg);
        try
        {
            await ws.SendText(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[WS] send falló: " + e.Message);
        }
    }

    private async void OnApplicationQuit()
    {
        if (ws != null && ws.State == WebSocketState.Open)
            await ws.Close();
    }
}