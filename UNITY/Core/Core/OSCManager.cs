using System;
using UnityEngine;
using extOSC;

/// <summary>
/// OSCManager — Singleton centralizado de comunicación OSC con el ESP32.
///
/// 
/// El resto de scripts NO tocan OSC directamente — solo se suscriben a los
/// eventos de este manager o llaman a sus métodos Send.
/// </summary>
public class OSCManager : MonoBehaviour
{
    public static OSCManager Instance { get; private set; }

    [Header("extOSC — Componentes")]
    public OSCReceiver    receiver;     // LocalPort: 9999  (ESP32 → Unity)
    public OSCTransmitter transmitter;  // RemotePort: 8888 (Unity → ESP32)

    // ─────────────────────────────────────────────
    // EVENTOS — otros scripts se suscriben aquí
    // ─────────────────────────────────────────────

    /// /moon/move → int  0=parar | 1=derecha | 2=izquierda | 3=arriba | 4=abajo
    public event Action<int>   OnMove;

    /// /moon/decision → int  1-3 (módulo activado)
    public event Action<int>   OnDecision;

    /// /moon/emotion → float 0.0-1.0
    public event Action<float> OnEmotion;

    // ─────────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─────────────────────────────────────────────
    // BIND DE RECEPTORES-SUSCRIPCIÓN A MENSAJES DEL ESP
    // ─────────────────────────────────────────────

    void Start()
    {
        receiver.Bind("/moon/move",     OnMoveReceived);
        receiver.Bind("/moon/decision", OnDecisionReceived);
        receiver.Bind("/moon/emotion",  OnEmotionReceived);

        Debug.Log("[OSCManager] Escuchando en puerto " + receiver.LocalPort);
    }

    // ─────────────────────────────────────────────
    // CALLBACKS INTERNOS-FUNCIONES QUE TAMBIÉN AFECTAN A UNITY
    // ─────────────────────────────────────────────

    void OnMoveReceived(OSCMessage message)
    {
        int dir = message.Values[0].IntValue;
        Debug.Log("[OSC] /moon/move → " + dir);
        OnMove?.Invoke(dir);
    }

    void OnDecisionReceived(OSCMessage message)
    {
        int modulo = message.Values[0].IntValue;
        Debug.Log("[OSC] /moon/decision → módulo " + modulo);
        OnDecision?.Invoke(modulo);
    }

    void OnEmotionReceived(OSCMessage message)
    {
        float valor = message.Values[0].FloatValue;
        Debug.Log("[OSC] /moon/emotion → " + valor.ToString("F3"));
        OnEmotion?.Invoke(valor);
    }

    // ─────────────────────────────────────────────
    // MÉTODOS DE ENVÍO — Unity → ESP32 (DEFINICIÓN DE LOS TIPOS DE VALORES)
    // ─────────────────────────────────────────────

 //EMOCIÓN=FLOAT
    /// Envía el valor emocional normalizado (0.0-1.0) al ESP32
    public void SendEmotion(float valor)
    {
        var msg = new OSCMessage("/moon/emotion");
        msg.AddValue(OSCValue.Float(valor));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/emotion " + valor.ToString("F3"));
    }
//DRV=GESTIONADO CON INT=EFECTO , VÍA LA LIBRERIÍA DE ADAFRUIT
    /// Activa un efecto háptico en el DRV2605 (0-127)
    public void SendVibration(int efecto)
    {
        var msg = new OSCMessage("/moon/vibration");
        msg.AddValue(OSCValue.Int(efecto));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/vibration " + efecto);
    }

    /// DEFINE EL TIPO DE LEDS Y DE QUE FORMA VA A EJECUTAR EN ENVIO (RGB -(valores 0-255 por canal)
    public void SendLEDs(int r, int g, int b)
    {
        var msg = new OSCMessage("/moon/leds");
        msg.AddValue(OSCValue.Int(r));
        msg.AddValue(OSCValue.Int(g));
        msg.AddValue(OSCValue.Int(b));
        transmitter.Send(msg);
        Debug.Log($"[OSC] → /moon/leds ({r},{g},{b})");
    }

    //ENVÍO DOBLE: DECISIÓN TOMADA POR EL ESP -> RECIBIDA POR UNITY = trigger emocional (unity+esp)/// 
    public void SendDecision(int modulo)
    {
        var msg = new OSCMessage("/moon/decision");
        msg.AddValue(OSCValue.Int(modulo));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/decision " + modulo);
        OnDecision?.Invoke(modulo);
    }

    /// CONFIRMACION VISUAL SIMPLE DE LA ELECCIÓN DE LA DECISIÓN
    public void SendConfirm(int modulo)
    {
        var msg = new OSCMessage("/moon/confirm");
        msg.AddValue(OSCValue.Int(modulo));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/confirm " + modulo);
    }
}
