using NativeWebSocket;  
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class VoiceReceiverTutorial : MonoBehaviour
{
    private WebSocket ws;
    public GameObject escudoPrefab;
    public GameObject rocaPrefab;
    public GameObject espadaPrefab;

    private Dictionary<string, GameObject> comandos;
    private Dictionary<string, Vector3> posiciones;
    private Dictionary<string, Vector3> rotaciones;
    private Dictionary<string, Vector3> escalas;

    async void Start()
    {
        comandos = new Dictionary<string, GameObject>()
        {
            { "escudo", escudoPrefab },
            { "roca", rocaPrefab },
            { "espada", espadaPrefab }
        };

        // Posiciones específicas para cada objeto
        posiciones = new Dictionary<string, Vector3>()
        {
            { "escudo", new Vector3(28.822f, 9.189f, 19.974f) },
            { "roca", new Vector3(25f, 9f, 18f) },
            { "espada", new Vector3(30.4740009f, 9.12794304f, 19.8920002f) }
        };
        
        escalas = new Dictionary<string, Vector3>()
        {
            { "escudo", new Vector3(0.411803693f, 0.411803693f, 0.411803693f) },
            { "roca", new Vector3(1f, 1f, 1f) },
            { "espada", new Vector3(0.9216892f, 0.9216892f, 0.9216892f) }
        };

        rotaciones = new Dictionary<string, Vector3>()
        {
            { "escudo", new Vector3(275.517761f, 176.145065f, 359.373444f) },
            { "roca", new Vector3(0f, 0f, 0f) },
            { "espada", new Vector3(350.043762f, 114.132828f, 3.25513291f) }
        };

        ws = new WebSocket("ws://192.168.137.1:5000");

        ws.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes).ToLower();
            Debug.Log("Recibido en Tutorial: " + msg);

            foreach (var par in comandos)
            {
                if (msg.Contains(par.Key))
                {
                    Spawn(par.Value, par.Key);
                }
            }
        };

        await ws.Connect();
    }

    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        ws.DispatchMessageQueue();
    #endif
    }

    void Spawn(GameObject prefab, string comando)
    {
        Vector3 pos = posiciones[comando];
        Quaternion rot = Quaternion.Euler(rotaciones[comando]);
        
        GameObject obj = Instantiate(prefab, pos, rot);
        obj.transform.localScale = escalas[comando];
    }

    private async void OnApplicationQuit()
    {
        if (ws != null) await ws.Close();
    }
}