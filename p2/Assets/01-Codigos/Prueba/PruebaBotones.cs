using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Botones")]
    public Button retrocederButton;
    public Button pausaButton;
    public Button adelantarButton;

    [Header("Configuración")]
    public float segundos = 5f;

    private bool pausado = false;

    void Start()
    {
        // Asignar funciones a los botones
        if (retrocederButton != null)
            retrocederButton.onClick.AddListener(Retroceder);

        if (adelantarButton != null)
            adelantarButton.onClick.AddListener(Adelantar);

        if (pausaButton != null)
            pausaButton.onClick.AddListener(PausarReanudar);
    }

    // Recibe el nuevo AudioSource desde ObraControlador sin reproducirlo automáticamente
    public void AsignarNuevoAudio(AudioSource nuevoAudio)
    {
        // Si ya había un audio asignado antes, lo detenemos y reiniciamos su tiempo
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }

        audioSource = nuevoAudio;
        pausado = false;
    }

    public void ReproducirSonido()
    {
        if (audioSource != null)
        {
            audioSource.Play();
            pausado = false;
        }
    }

    public void Retroceder()
    {
        if (audioSource == null) return;

        audioSource.time = Mathf.Max(
            0f, audioSource.time - segundos
        );
    }

    public void Adelantar()
    {
        if (audioSource == null) return;

        audioSource.time = Mathf.Min(
            audioSource.clip.length,
            audioSource.time + segundos
        );
    }

    public void PausarReanudar()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
        {
            // Si está sonando, lo pausa
            audioSource.Pause();
            pausado = true;
        }
        else if (pausado)
        {
            // Si estaba pausado, lo reanuda donde quedó
            audioSource.UnPause();
            pausado = false;
        }
        else
        {
            // Si aún no había empezado a sonar (o terminó), lo inicia desde cero
            audioSource.Play();
            pausado = false;
        }
    }

    void OnDestroy()
    {
        if (retrocederButton != null)
            retrocederButton.onClick.RemoveListener(Retroceder);

        if (adelantarButton != null)
            adelantarButton.onClick.RemoveListener(Adelantar);

        if (pausaButton != null)
            pausaButton.onClick.RemoveListener(PausarReanudar);
    }
}
