using UnityEngine;
using UnityEngine.SceneManagement;

public class scena_clase_casa : MonoBehaviour
{
    public string NombreScena = "octava_scena";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}
