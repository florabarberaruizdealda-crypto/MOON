using UnityEngine;
using UnityEngine.SceneManagement;

public class scena_3_4 : MonoBehaviour
{
    public string NombreScena = "Quinta_scena";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}
