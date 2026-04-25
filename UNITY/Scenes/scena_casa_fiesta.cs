using UnityEngine;
using UnityEngine.SceneManagement;

public class scena_casa_fiesta : MonoBehaviour
{
    public string NombreScena = "Septima_Scena 1";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}
