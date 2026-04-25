using UnityEngine;
using extOSC;

public class ReceptorESP : MonoBehaviour
{
    public OSCReceiver receiver;

    void Start()
    {
        receiver.Bind("/knob", MensajePotenciometro);
        receiver.Bind("/trigger", MensajeBoton);
    }

    void MensajePotenciometro(OSCMessage mensaje)
    {
        int valor = mensaje.Values[0].IntValue;
        Debug.Log("Potenciómetro: " + valor);
    }

    void MensajeBoton(OSCMessage mensaje)
    {
        int valor = mensaje.Values[0].IntValue;
        Debug.Log("Botón pulsado: " + valor);
    }
}