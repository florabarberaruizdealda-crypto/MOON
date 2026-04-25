using UnityEngine;
using UnityEngine.SceneManagement;

public class Scena_exterior_cafe : MonoBehaviour
{
    public string NombreScena = "Sexta_scena";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NombreScena);
        }
    }
}
