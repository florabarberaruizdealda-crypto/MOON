using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Slider barraEmocional;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        barraEmocional.value = 50;
    }

    public void CambiarBarra(float cantidad)
    {
        barraEmocional.value += cantidad;
    }
}
