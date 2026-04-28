using UnityEngine;

/// <summary>
/// EmotionManager — Métrica emocional acumulativa 0–100.
///
/// FUENTES DE CAMBIO:
///   Decisión:  ±30   → AddEmotion(±30)
///   Objeto:    ±5–10 → AddEmotion(±delta)
///   NPC:       ±5    → AddEmotion(±5)
///
/// RESET: solo al pasar Loop 1 → Loop 2 → ResetEmotion()
///
/// 5 ESTADOS (intervalos de 20):
///   1 →  0–20   Extreme Anxiety
///   2 → 20–40   Moderate Anxiety
///   3 → 40–60   Neutral
///   4 → 60–80   Moderate Euphoria
///   5 → 80–100  Extreme Euphoria
/// </summary>
public class EmotionManager : MonoBehaviour
{
    public static EmotionManager Instance { get; private set; }

    [Header("Valor inicial (0-100)")]
    [Range(0, 100)]
    public float valorInicial = 50f;  // Empieza en Neutral

    private float valor = 50f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        valor = valorInicial;
    }

    // ─────────────────────────────────────────────
    // MODIFICAR
    // ─────────────────────────────────────────────

    /// Suma o resta. Positivo = más euforia, negativo = más ansiedad.
    public void AddEmotion(float delta)
    {
        valor = Mathf.Clamp(valor + delta, 0f, 100f);
        Debug.Log($"[EmotionManager] {delta:+0;-0} → {valor:F1} (Estado {GetEstado()})");
        GameManager.Instance?.OnEmotionChanged(valor);
    }

    /// Reset total — solo al entrar en Loop 2
    public void ResetEmotion()
    {
        valor = valorInicial;
        Debug.Log("[EmotionManager] Reset → " + valor);
        GameManager.Instance?.OnEmotionChanged(valor);
    }

    // ─────────────────────────────────────────────
    // CONSULTAR
    // ─────────────────────────────────────────────

    /// Valor crudo 0–100
    public float GetValor() => valor;

    /// Normalizado 0.0–1.0 para OSC al ESP32
    public float GetNormalized() => valor / 100f;

    /// Estado emocional 1–5
    public int GetEstado()
    {
        if (valor <= 20f) return 1;
        if (valor <= 40f) return 2;
        if (valor <= 60f) return 3;
        if (valor <= 80f) return 4;
        return 5;
    }

    /// Color Unity del estado actual
    public Color GetColor()
    {
        switch (GetEstado())
        {
            case 1: return new Color(0.9f, 0.1f, 0.1f);  // Rojo       — Extreme Anxiety
            case 2: return new Color(0.9f, 0.5f, 0.1f);  // Naranja    — Moderate Anxiety
            case 3: return new Color(0.9f, 0.9f, 0.9f);  // Blanco     — Neutral
            case 4: return new Color(0.2f, 0.4f, 0.9f);  // Azul       — Moderate Euphoria
            case 5: return new Color(0.6f, 0.1f, 0.9f);  // Neon       — Extreme Euphoria
            default: return Color.white;
        }
    }
}
