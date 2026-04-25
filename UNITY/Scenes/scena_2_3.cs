using UnityEngine;
using UnityEngine.SceneManagement;

public class scena_2_3 : MonoBehaviour
{
    public string NombreScena = "Tercera_scena";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}
