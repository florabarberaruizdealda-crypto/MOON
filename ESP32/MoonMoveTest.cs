using UnityEngine;
using extOSC;

/// <summary>
/// TEST — Movimiento del cuadrado via OSC desde el ESP32.
///
/// SETUP:
///   - Añade este script al GameObject "Square"
///   - Arrastra el OSCReceiver al campo Receiver en el Inspector
///   - El OSCReceiver debe tener LocalPort: 9999
///
/// MENSAJES QUE ESCUCHA:
///   /moon/move → int
///     0 = sin toque   (dedo levantado → el cuadrado se para)
///     1 = derecha
///     2 = izquierda
///     3 = arriba
///     4 = abajo
///
/// El movimiento es continuo mientras el pin está tocado:
/// el ESP envía el mismo valor cada 20ms y el cuadrado
/// se mueve cada frame en Update().
/// </summary>
public class MoonMoveTest : MonoBehaviour
{
    [Header("extOSC")]
    public OSCReceiver receiver;

    [Header("Movimiento")]
    public float speed = 5f;

    // Dirección recibida por OSC — se actualiza cada mensaje
    private Vector2 direccionActual = Vector2.zero;

    void Start()
    {
        receiver.Bind("/moon/move", OnMove);
    }

    // Callback OSC — llega cada vez que el ESP envía un mensaje
    void OnMove(OSCMessage message)
    {
        int dir = message.Values[0].IntValue;

        switch (dir)
        {
            case 0: direccionActual = Vector2.zero;  break; // sin toque
            case 1: direccionActual = Vector2.right; break; // derecha
            case 2: direccionActual = Vector2.left;  break; // izquierda
            case 3: direccionActual = Vector2.up;    break; // arriba
            case 4: direccionActual = Vector2.down;  break; // abajo
        }

        Debug.Log("[OSC] /moon/move → " + dir);
    }

    // Se ejecuta cada frame — mueve el cuadrado mientras haya dirección activa
    void Update()
    {
        if (direccionActual != Vector2.zero)
        {
            transform.position += (Vector3)direccionActual * speed * Time.deltaTime;
        }
    }
}
