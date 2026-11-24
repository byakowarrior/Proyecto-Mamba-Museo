using UnityEngine;
using System.Collections;
using System;

public class ControlDeSonidos : MonoBehaviour
{

    public bool Reproduciendo; // ok
    public float AvanzarReproducirSalto = 5f; // salto de 5 segundos - para avazanar o retroceder
    public AudioSource sonido; // ok
    public float tiempo_actual; // ok
    public Coroutine corutina;

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

    private void interfazAsingarObra(AudioSource _sonido)
    {
        sonido = _sonido;
        print("---");
    }
}
