using NativeWebSocket;  
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // AÑADIDO
using System.Collections;

public class VoiceReceiverGame : MonoBehaviour
{
    private WebSocket ws;
    public GameObject escudoPrefab;
    public GameObject rocaPrefab;
    public GameObject espadaPrefab;
    
    // Para resetear la escena
    public float retrasoReset = 1f; // Tiempo de espera antes de resetear

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
            { "escudo", new Vector3(4.289874f, 9.241675f, 19.4786f) },    // ShieldCollider
            { "roca", new Vector3(0f, 1f, 0f) },                          // Ajustar según necesites
            { "espada", new Vector3(5.82166f, 9.127943f, 19.03479f) }     // StoneSwordCollider
        };
        
        // Rotaciones específicas para cada objeto
        rotaciones = new Dictionary<string, Vector3>()
        {
            { "escudo", new Vector3(-84.482f, -179.37f, -0.627f) },       // ShieldCollider
            { "roca", new Vector3(0f, 0f, 0f) },                          // Ajustar según necesites
            { "espada", new Vector3(-15.006f, -218.006f, -1.87f) }        // StoneSwordCollider
        };
        
        // Escalas específicas para cada objeto
        escalas = new Dictionary<string, Vector3>()
        {
            { "escudo", new Vector3(0.4187258f, 0.4187258f, 0.4187257f) }, // ShieldCollider
            { "roca", new Vector3(1f, 1f, 1f) },                           // Ajustar según necesites
            { "espada", new Vector3(0.9216896f, 0.9216893f, 0.9216895f) }  // StoneSwordCollider
        };

        ws = new WebSocket("ws://192.168.137.1:5000");

        ws.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes).ToLower();
            Debug.Log("Recibido en Juego: " + msg);
            
            // Detectar comando de reset
            if (msg.Contains("reset") || msg.Contains("reinicia"))
            {
                Debug.Log("¡Comando de reset detectado! Reiniciando escena...");
                ResetearEscena();
                return; // Salir para no procesar más comandos
            }

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
        // Usar posición, rotación y escala específicas
        Vector3 pos = posiciones[comando];
        Quaternion rot = Quaternion.Euler(rotaciones[comando]);
        
        GameObject obj = Instantiate(prefab, pos, rot);
        obj.transform.localScale = escalas[comando];
        
        Debug.Log($"Objeto {comando} instanciado en posición: {pos}");
    }

    // Nueva función para resetear la escena
    public void ResetearEscena()
    {
        // Opcional: Mostrar mensaje o efecto antes del reset
        Debug.Log("Reiniciando escena en " + retrasoReset + " segundos...");
        
        // Invoca el reset después del retraso especificado
        Invoke(nameof(ReiniciarEscenaActual), retrasoReset);
    }
    
    private void ReiniciarEscenaActual()
    {
        // Obtiene el índice de la escena actual y la vuelve a cargar
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.buildIndex);
    }

    private async void OnApplicationQuit()
    {
        if (ws != null) await ws.Close();
    }
}