using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class BotonIniciar : MonoBehaviour
{
    [SerializeField] private string escenaDestino = "NombreDeTuEscena";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Iniciar);
    }

    private void Iniciar()
    {
        SceneManager.LoadScene(escenaDestino);
    }
}