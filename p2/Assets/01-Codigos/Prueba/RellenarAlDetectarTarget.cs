using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObraInformacion))]
public class RellenarAlDetectarTarget : MonoBehaviour
{
    [SerializeField] private TextosObraUI textosUI;

    private ObserverBehaviour observer;
    private ObraInformacion obraInfo;

    private void Awake()
    {
        obraInfo = GetComponent<ObraInformacion>();
        observer = GetComponent<ObserverBehaviour>();
        if (observer == null)
        {
            observer = GetComponentInParent<ObserverBehaviour>();
        }

        if (textosUI == null)
        {
            textosUI = FindFirstObjectByType<TextosObraUI>(FindObjectsInactive.Include);
        }
    }

    private void OnEnable()
    {
        if (observer != null)
        {
            observer.OnTargetStatusChanged += ManejarEstadoTarget;

            // Si ya estaba detectado al activarse el script, rellena de inmediato
            if (EsEstadoDetectado(observer.TargetStatus.Status))
            {
                RellenarAhora();
            }
        }
    }

    private void OnDisable()
    {
        if (observer != null)
        {
            observer.OnTargetStatusChanged -= ManejarEstadoTarget;
        }
    }

    private void ManejarEstadoTarget(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (EsEstadoDetectado(status.Status))
        {
            RellenarAhora();
        }
    }

    private bool EsEstadoDetectado(Status estado)
    {
        return estado == Status.TRACKED || estado == Status.EXTENDED_TRACKED;
    }

    public void RellenarAhora()
    {
        if (textosUI == null)
        {
            textosUI = FindFirstObjectByType<TextosObraUI>(FindObjectsInactive.Include);
        }

        if (textosUI != null && obraInfo != null)
        {
            textosUI.Rellenar(obraInfo);
        }
    }
}