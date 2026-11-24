using UnityEngine;

public class ControladorAnimacionBoton : MonoBehaviour
{
    [Header("Referencia al objeto que tiene el Animator")]
    public Animator animatorObjetivo;

    // Método para el Botón 1 (ej. Abrir / Encender)
    public void ActivarEstado()
    {
        if (animatorObjetivo != null)
        {
            animatorObjetivo.SetBool("estaActivo", true);
        }
    }

    // Método para el Botón 2 (ej. Cerrar / Apagar)
    public void DesactivarEstado()
    {
        if (animatorObjetivo != null)
        {
            animatorObjetivo.SetBool("estaActivo", false);
        }
    }
}