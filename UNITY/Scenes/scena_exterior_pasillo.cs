using UnityEngine;
using UnityEngine.SceneManagement;


public class scena_exterior_pasillo : MonoBehaviour
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

