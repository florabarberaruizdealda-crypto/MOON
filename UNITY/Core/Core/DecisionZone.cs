using UnityEngine;

/// <summary>
/// DecisionZone — Zona de decisión física en escena.
///
/// ARQUITECTURA REAL:
///   El sensor Hall del ESP32 detecta el imán del módulo y envía
///   /moon/decision (1 o 2) a Unity. Unity procesa la emoción
///   y devuelve /moon/decision al ESP32 para el feedback inmediato.
///
/// Este script ya NO dispara la decisión por colisión —
/// la decisión viene del hardware. Solo define el deltaEmocion
/// asociado a cada módulo y lo aplica cuando OSCManager lo recibe.
///
/// SETUP EN ESCENA:
///   - Un DecisionZone por módulo físico
///   - moduloID: 1 o 2 (módulo 3 no disponible en demo)
///   - deltaEmocion: negativo (ansiedad) o positivo (euforia)
///   - Llamar RegisterZone() en Start para que OSCManager sepa qué delta aplicar
/// </summary>
public class DecisionZone : MonoBehaviour
{
    [Header("Identidad")]
    public int moduloID = 1;

    [Header("Impacto emocional (±30)")]
    public float deltaEmocion = -30f;  // Módulo 1: ansiedad (-30) / Módulo 2: euforia (+30)

    void OnEnable()
    {
        OSCManager.Instance.OnDecision += OnDecisionReceived;
    }

    void OnDisable()
    {
        if (OSCManager.Instance != null)
            OSCManager.Instance.OnDecision -= OnDecisionReceived;
    }

    void OnDecisionReceived(int modulo)
    {
        if (modulo != moduloID) return;

        // 1. Acumular emoción según este módulo
        EmotionManager.Instance.AddEmotion(deltaEmocion);

        // 2. Devolver decisión al ESP32 para feedback inmediato (flash + haptic)
        OSCManager.Instance.SendDecision(moduloID);

        // 3. Enviar emoción actualizada al ESP32 para actualizar estado continuo
        OSCManager.Instance.SendEmotion(EmotionManager.Instance.GetNormalized());

        Debug.Log($"[DecisionZone] Módulo {moduloID} | Δ{deltaEmocion:+0;-0} → {EmotionManager.Instance.GetValor():F1} (Estado {EmotionManager.Instance.GetEstado()})");
    }

    // Módulo 3 — no disponible en demo
    // void OnDecision3() { ... }
}
