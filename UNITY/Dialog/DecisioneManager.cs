using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisioneManager : MonoBehaviour
{
    private TextMeshProUGUI textoOpcionA;
    public TextMeshProUGUI textoOpcionB;
    public TextMeshProUGUI textoSeleccion;

    private string seleccionActual = "";

    public TextMeshProUGUI TextoOpcionA { get => textoOpcionA; set => textoOpcionA = value; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            seleccionActual = "A";
            textoSeleccion.text = "Elegiste: A";
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            seleccionActual = "B";
            textoSeleccion.text = "Elegiste: B";
        }
    }
}