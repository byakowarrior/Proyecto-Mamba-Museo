using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;

public class ControlDeSonidos : MonoBehaviour
{

    public bool Reproduciendo; // ok
    public float AvanzarReproducirSalto = 5f; // salto de 5 segundos - para avazanar o retroceder
    public AudioSource sonido; // ok
    public float tiempo_actual; // ok
    public Coroutine corutina;
    public Image ImagenProgreso;
    public float progreso = 0;

    public TextMeshProUGUI autor_tmp;
    public TextMeshProUGUI titulo_tmp;
    public TextMeshProUGUI fecha_tmp;
    public TextMeshProUGUI tecnica_tmp;

    private void LateUpdate()
    {
        if (sonido != null)
        {
            if (sonido.time > 0)
            {
                float tiempoActual = sonido.time;
                float duracion = sonido.clip.length;
                progreso = tiempoActual / duracion;
                ImagenProgreso.fillAmount = progreso;
                Eventos.AudioProgreso = progreso;
            }
            else if (progreso == 1)
            {
                Pausa();
                Eventos.AudioProgreso = 0;
            }
            else if (progreso == 0)
            {
                ImagenProgreso.fillAmount = 0;
            }
        }
    }


    [ContextMenu("Pausa")]

    public void Presionado()
    {
        corutina = StartCoroutine(Corutina_MatenerPresionado());
    }

    public void Liberado()
    {
        if (corutina != null)
        {
            StopCoroutine(corutina);
        }
    }

    private void Awake()
    {
        tiempo_actual = 0f;
    }

    IEnumerator Corutina_MatenerPresionado()
    {
        yield return new WaitForSeconds(2);
        Reiniciar();
    }
    public void Reiniciar()
    {
        tiempo_actual = 0;
        sonido.time = tiempo_actual;
        sonido.Stop();
    }
    public void Detener()
    {
        tiempo_actual = 0;
        sonido.time = tiempo_actual;
        sonido.Stop();
        ImagenProgreso.fillAmount = 0;
    }
    public void Pausa()
    {
        if(sonido.isPlaying == true)
        {
            tiempo_actual = sonido.time; 
            sonido.Stop();
        }
    }

    public void Reproducir()
    {
        if(sonido.isPlaying == false)
        {
            sonido.time = tiempo_actual;
            sonido.Play();
        }
    }

    public void Retroceder()
    {
        tiempo_actual -= AvanzarReproducirSalto;
        if (tiempo_actual < 0)
        {
            tiempo_actual = 0;
        }
        
        if (sonido.isPlaying)
        {
            sonido.Stop();
            sonido.time = tiempo_actual;
            sonido.Play();
        }
        //
        ImagenProgreso.fillAmount = tiempo_actual / sonido.clip.length;
    }

    public void Avanzar()
    {
        tiempo_actual += AvanzarReproducirSalto;
        
        if (sonido.isPlaying)
        {
            sonido.Stop();
            sonido.time = tiempo_actual;
            sonido.Play();
        }
        //
        ImagenProgreso.fillAmount = tiempo_actual / sonido.clip.length;
    }
    private void interfazAsingarObra(AudioSource _sonido, string[] _informacion)
    {
        // asignar el sonido recibido
        sonido = _sonido;

        // asignar informacion
        autor_tmp.text = _informacion[0];
        titulo_tmp.text = _informacion[1];
        fecha_tmp.text = _informacion[2];
        tecnica_tmp.text = _informacion[3];
    }

    private void OnEnable()
    {
        Eventos.InterfazAsingarObra += interfazAsingarObra;
    }

    private void OnDisable()
    {
        tiempo_actual = 0;
        Eventos.InterfazAsingarObra -= interfazAsingarObra;
    }

}
