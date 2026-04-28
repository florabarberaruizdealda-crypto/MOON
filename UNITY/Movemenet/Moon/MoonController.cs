using UnityEngine;

/// <summary>
/// MoonController — Movimiento y reacción visual del Moon.
/// Se suscribe a OSCManager en lugar de bindear OSC directamente.
/// </summary>
public class MoonController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;

    [Header("Escala emocional")]
    public float escalaMinima = 0.5f;
    public float escalaMaxima = 3f;

    private Vector2 direccionActual = Vector2.zero;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        OSCManager.Instance.OnMove     += OnMove;
        OSCManager.Instance.OnDecision += OnDecision;
    }

    void OnDestroy()
    {
        if (OSCManager.Instance == null) return;
        OSCManager.Instance.OnMove     -= OnMove;
        OSCManager.Instance.OnDecision -= OnDecision;
    }

    void Update()
    {
        if (direccionActual != Vector2.zero)
            transform.position += (Vector3)direccionActual * velocidad * Time.deltaTime;
    }

    // ─────────────────────────────────────────────
    // HANDLERS
    // ─────────────────────────────────────────────

    void OnMove(int dir)
    {
        switch (dir)
        {
            case 0: direccionActual = Vector2.zero;  break;
            case 1: direccionActual = Vector2.right; break;
            case 2: direccionActual = Vector2.left;  break;
            case 3: direccionActual = Vector2.up;    break;
            case 4: direccionActual = Vector2.down;  break;
        }
    }

    //void OnDecision(int modulo)
    //{
    //  // Actualizar color y escala según estado emocional actual
    // if (sr != null) sr.color = EmotionManager.Instance.GetColor();
      //  float escala = Mathf.Lerp(escalaMinima, escalaMaxima, EmotionManager.Instance.GetNormalized());
        //transform.localScale = new Vector3(escala, escala, 1f);

        // Debug.Log("[MoonController] Decisión " + modulo + " → estado " + EmotionManager.Instance.GetEstado());
    }
}
