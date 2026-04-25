using UnityEngine;
using UnityEngine.SceneManagement;

public class scena_1_2 : MonoBehaviour
{
    public string NombreScena = "Segunda_scena";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}

