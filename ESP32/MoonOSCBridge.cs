using UnityEngine;
using extOSC;

/// <summary>
/// CAPA 1 - Prueba de comunicación ESP32 <-> Unity via OSC
///
/// SETUP EN LA ESCENA:
///   1. Crea un GameObject vacío → llámalo "OSCManager"
///      - Añade componente OSCReceiver  → LocalPort: 9999
///      - Añade componente OSCTransmitter → RemoteHost: [IP del ESP32], RemotePort: 8888
///   2. Crea un GameObject con SpriteRenderer (tu cuadrado)
///      - Añade este script
///      - En el Inspector, arrastra el OSCManager a los campos Receiver y Transmitter
///
/// MENSAJES QUE RECIBE DESDE EL ESP32:
///   /moon/move     → int  (1=derecha, 2=izquierda, 3=arriba, 4=abajo)
///   /moon/decision → int  (1, 2 o 3 — qué módulo se ha activado)
///   /moon/emotion  → float (0.0 a 1.0 — estado emocional)
///
/// MENSAJES QUE ENVÍA UNITY AL ESP32:
///   /moon/vibration → int (intensidad 0-255)
///   /moon/leds      → int int int (R G B, valores 0-255)
///   /moon/confirm   → int (1 = decisión recibida)
/// </summary>
public class MoonOSCBridge : MonoBehaviour
{
    [Header("extOSC — Conexión")]
    public OSCReceiver receiver;       // Escucha mensajes del ESP32  (puerto 9999)
    public OSCTransmitter transmitter; // Envía mensajes al ESP32     (puerto 8888)

    [Header("Movimiento")]
    public float velocidadMovimiento = 2f;

    [Header("Escala emocional")]
    public float escalaMinima = 0.5f;
    public float escalaMaxima = 3f;

    // ─────────────────────────────────────────────
    // UNITY — ARRANQUE
    // ─────────────────────────────────────────────

    void Start()
    {
        // Bindear cada address OSC a su función receptora
        receiver.Bind("/moon/move",     OnMoveReceived);
        receiver.Bind("/moon/decision", OnDecisionReceived);
        receiver.Bind("/moon/emotion",  OnEmotionReceived);
    }

    // ─────────────────────────────────────────────
    // RECEPTORES — llegan del ESP32
    // ─────────────────────────────────────────────

    /// <summary>
    /// El ESP32 envía: OSCMessage("/moon/move").add(int dirección)
    /// 1 = derecha, 2 = izquierda, 3 = arriba, 4 = abajo
    /// </summary>
    void OnMoveReceived(OSCMessage message)
    {
        int direccion = message.Values[0].IntValue;
        Debug.Log("[OSC] /moon/move → " + direccion);

        Vector3 delta = Vector3.zero;
        switch (direccion)
        {
            case 1: delta = Vector3.right; break;
            case 2: delta = Vector3.left;  break;
            case 3: delta = Vector3.up;    break;
            case 4: delta = Vector3.down;  break;
        }

        transform.position += delta * velocidadMovimiento * Time.deltaTime;
    }

    /// <summary>
    /// El ESP32 envía: OSCMessage("/moon/decision").add(int modulo)
    /// 1, 2 o 3 según en qué módulo de decisión se posa la pirámide.
    /// El cuadrado cambia de color y Unity confirma al ESP32.
    /// </summary>
    void OnDecisionReceived(OSCMessage message)
    {
        int modulo = message.Values[0].IntValue;
        Debug.Log("[OSC] /moon/decision → módulo " + modulo);

        Color color = Color.white;
        switch (modulo)
        {
            case 1: color = new Color(0.9f, 0.3f, 0.3f); break; // rojo
            case 2: color = new Color(0.3f, 0.5f, 0.9f); break; // azul
            case 3: color = new Color(0.3f, 0.9f, 0.5f); break; // verde
        }
        GetComponent<SpriteRenderer>().color = color;

        // Confirmación al ESP32 — feedback al controlador
        EnviarConfirmacion(modulo);
    }

    /// <summary>
    /// El ESP32 envía: OSCMessage("/moon/emotion").add(float valor)
    /// Valor normalizado entre 0.0 y 1.0.
    /// El cuadrado crece o encoge según ese valor.
    /// </summary>
    void OnEmotionReceived(OSCMessage message)
    {
        float valor = message.Values[0].FloatValue;
        Debug.Log("[OSC] /moon/emotion → " + valor.ToString("F3"));

        float escala = Mathf.Lerp(escalaMinima, escalaMaxima, valor);
        transform.localScale = new Vector3(escala, escala, 1f);
    }

    // ─────────────────────────────────────────────
    // EMISORES — Unity envía al ESP32
    // ─────────────────────────────────────────────

    /// <summary>
    /// Activa la vibración del motor LRA.
    /// intensidad: 0 (off) a 255 (máximo)
    /// Llamar desde cualquier script del juego: GetComponent<MoonOSCBridge>().EnviarVibracion(200);
    /// </summary>
    public void EnviarVibracion(int intensidad)
    {
        var msg = new OSCMessage("/moon/vibration");
        msg.AddValue(OSCValue.Int(intensidad));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/vibration " + intensidad);
    }

    /// <summary>
    /// Cambia el color de los LEDs WS2812B.
    /// r, g, b: valores 0-255
    /// </summary>
    public void EnviarColorLEDs(int r, int g, int b)
    {
        var msg = new OSCMessage("/moon/leds");
        msg.AddValue(OSCValue.Int(r));
        msg.AddValue(OSCValue.Int(g));
        msg.AddValue(OSCValue.Int(b));
        transmitter.Send(msg);
        Debug.Log($"[OSC] → /moon/leds ({r},{g},{b})");
    }

    /// <summary>
    /// Confirma al ESP32 que la decisión del módulo fue recibida.
    /// </summary>
    void EnviarConfirmacion(int modulo)
    {
        var msg = new OSCMessage("/moon/confirm");
        msg.AddValue(OSCValue.Int(modulo));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/confirm " + modulo);
    }
}
