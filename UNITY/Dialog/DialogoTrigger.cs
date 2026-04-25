using UnityEngine;
using TMPro;
using System.Collections;

public class DialogoTrigger : MonoBehaviour
{
    public TMP_Text textoDialogo;
    public TMP_Text textoOpcionA;
    public TMP_Text textoOpcionB;
    public TMP_Text textoSeleccion;

    private bool yaActivado = false;

    void Start()
    {
        textoDialogo.gameObject.SetActive(false);
        textoOpcionA.gameObject.SetActive(false);
        textoOpcionB.gameObject.SetActive(false);
        textoSeleccion.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !yaActivado)
        {
            yaActivado = true;
            StartCoroutine(SecuenciaDialogo());
        }
    }

    IEnumerator SecuenciaDialogo()
    {
        textoDialogo.gameObject.SetActive(true);

        textoDialogo.text = "¡No me escuchas nunca!";
        yield return new WaitForSeconds(3);

        textoDialogo.text = "¡Estoy harta de esto!";
        yield return new WaitForSeconds(3);

        textoDialogo.gameObject.SetActive(false);
        textoOpcionA.text = "A: Ignorarles";
        textoOpcionB.text = "B: Intervenir";
        textoOpcionA.gameObject.SetActive(true);
        textoOpcionB.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!textoOpcionA.gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            textoOpcionA.gameObject.SetActive(false);
            textoOpcionB.gameObject.SetActive(false);
            textoSeleccion.text = "Has seleccionado la opción A";
            textoSeleccion.gameObject.SetActive(true);
            GameManager.Instance.CambiarBarra(-25);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            textoOpcionA.gameObject.SetActive(false);
            textoOpcionB.gameObject.SetActive(false);
            textoSeleccion.text = "Has seleccionado la opción B";
            textoSeleccion.gameObject.SetActive(true);
            GameManager.Instance.CambiarBarra(25);
        }
    }
}