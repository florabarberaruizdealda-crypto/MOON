using UnityEngine;
using extOSC;

/// <summary>
/// Ponlo en cada cuadrado de decisión.
///
/// SETUP por cuadrado:
///   - Collider2D con Is Trigger activado
///   - En el Inspector: asigna moduloID (1 o 2), colorDecision, transmitter
///   - El cuadrado Moon necesita Rigidbody2D para disparar OnTriggerEnter2D
///
/// OSC que envía al ESP32:
///   /moon/decision  → int (1 o 2 — qué módulo se activó)
///   /moon/vibration → int (intensidad 0-255)
/// </summary>
public class DecisionZone : MonoBehaviour
{
    [Header("Identidad")]
    public int moduloID = 1;         // 1 o 2 — cambia en cada zona
    public Color colorDecision = Color.red;

    [Header("extOSC")]
    public OSCTransmitter transmitter;

    [Header("Vibración")]
    [Range(0, 255)]
    public int intensidadVibracion = 200;

    // Referencia al SpriteRenderer del Moon (se busca automáticamente)
    private SpriteRenderer moonSprite;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Solo reacciona si entra el cuadrado Moon
        if (!other.CompareTag("Player")) return;

        Debug.Log("[Decisión] Módulo " + moduloID + " activado");

        // 1. Cambiar color del Moon
        moonSprite = other.GetComponent<SpriteRenderer>();
        if (moonSprite != null)
            moonSprite.color = colorDecision;

        // 2. Enviar decisión al ESP32
        EnviarDecision();
        EnviarVibracion();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Opcional: parar vibración al salir
        EnviarVibracionStop();
    }

    void EnviarDecision()
    {
        var msg = new OSCMessage("/moon/decision");
        msg.AddValue(OSCValue.Int(moduloID));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/decision " + moduloID);
    }

    void EnviarVibracion()
    {
        var msg = new OSCMessage("/moon/vibration");
        msg.AddValue(OSCValue.Int(intensidadVibracion));
        transmitter.Send(msg);
        Debug.Log("[OSC] → /moon/vibration " + intensidadVibracion);
    }

    void EnviarVibracionStop()
    {
        var msg = new OSCMessage("/moon/vibration");
        msg.AddValue(OSCValue.Int(0));
        transmitter.Send(msg);
    }
}
