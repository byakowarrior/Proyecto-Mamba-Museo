using UnityEngine;
using UnityEngine.EventSystems;

public class ObraControlador : MonoBehaviour
{
    private AudioController audioController;

    void Awake()
    {
      
        audioController = FindFirstObjectByType<AudioController>();
    }

  
    public void AsignarAudioSource()
    {
        
        GameObject botonPresionado = EventSystem.current.currentSelectedGameObject;

        if (botonPresionado != null && audioController != null)
        {
           
            AudioSource audioDelBoton = botonPresionado.GetComponent<AudioSource>();

            if (audioDelBoton != null)
            {
                audioController.AsignarNuevoAudio(audioDelBoton);
            }
            else
            {
                Debug.LogWarning("El botón '" + botonPresionado.name + "' no tiene un componente AudioSource.");
            }
        }
    }
}
