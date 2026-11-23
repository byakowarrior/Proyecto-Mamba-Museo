using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarkerAudioTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct MarkerAudioMapping
    {
        public string markerName;
        public AudioClip audioClip;
    }

    [Header("Configuración de Marcadores")]
    [SerializeField] private List<MarkerAudioMapping> markerAudios = new List<MarkerAudioMapping>();
    [SerializeField] private string targetSceneName = "Prueba";
    [SerializeField] private bool loadSceneAutomatically = true;

    private Dictionary<string, AudioClip> audioLookup;
    private bool isTransitioning = false;

    private void Awake()
    {
        audioLookup = new Dictionary<string, AudioClip>();
        foreach (var entry in markerAudios)
        {
            if (!string.IsNullOrEmpty(entry.markerName) && entry.audioClip != null)
            {
                audioLookup[entry.markerName] = entry.audioClip;
            }
        }
    }

    // Llama a este método desde el evento OnTargetFound (Vuforia, AR Foundation, etc.)
    // pasando el nombre del marcador detectado.
    public void OnMarkerDetected(string detectedMarkerName)
    {
        if (isTransitioning) return;

        if (audioLookup.TryGetValue(detectedMarkerName, out AudioClip clip))
        {
            AudioTransferData.SelectedAudio = clip;
            AudioTransferData.TargetName = detectedMarkerName;

            if (loadSceneAutomatically)
            {
                LoadTargetScene();
            }
        }
        else
        {
            Debug.LogWarning($"[MarkerAudioTrigger] No hay audio asignado para el marcador: '{detectedMarkerName}'");
        }
    }

    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName)) return;

        isTransitioning = true;
        SceneManager.LoadScene(targetSceneName);
    }
}