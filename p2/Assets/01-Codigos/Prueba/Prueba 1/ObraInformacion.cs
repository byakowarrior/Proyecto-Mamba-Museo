using NUnit.Framework;
using TMPro;
using UnityEngine;
using Vuforia;

public class ObraInformacion : MonoBehaviour
{
    public AudioSource Sonido;
    public AudioClip SonidoClip;
    public Canvas canvas;
    public GameObject Boton;
    public string Titulo;
    public string Autor;
    public string Descripcion;
    public string Fecha;
    public string Tecnica;

    public void InterfazAsingarObra()
    {
        string[] informacion = { Autor, Titulo, Fecha, Tecnica };
        Eventos.InterfazAsingarObra?.Invoke(
            Sonido, informacion);
        print("Asignar sonido:" + Sonido.clip);
    }

    public void Start()
    {
        // asinamos el clip de sonido dinámicamente
        Sonido.clip = SonidoClip;   
    }

}

