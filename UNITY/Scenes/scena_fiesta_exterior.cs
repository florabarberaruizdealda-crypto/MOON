using UnityEngine;
using UnityEngine.SceneManagement;

public class scena_fiesta_exterior : MonoBehaviour
{
    public string NombreScena = "Novena_scena";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}
