using TMPro;
using UnityEngine;

public class TextosObraUI : MonoBehaviour
{
    [Header("Contenedor de textos (opcional)")]
    [SerializeField] private Transform contenedorTextos;

    [Header("Textos en escena (sin Descripción)")]
    [SerializeField] private TMP_Text textoAutor;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoFecha;
    [SerializeField] private TMP_Text textoTecnica;

    private void Awake()
    {
        AsegurarReferencias();
    }

    private void AsegurarReferencias()
    {
        if (contenedorTextos == null)
        {
            contenedorTextos = transform;
        }

        if (textoAutor == null) textoAutor = BuscarTMP("Autor - 2L");
        if (textoTitulo == null) textoTitulo = BuscarTMP("Titulo - 2L");
        if (textoFecha == null) textoFecha = BuscarTMP("Fecha - 1L");
        if (textoTecnica == null) textoTecnica = BuscarTMP("Técnica - 3L");
    }

    private TMP_Text BuscarTMP(string nombreObjeto)
    {
        if (contenedorTextos == null) return null;

        Transform hijo = contenedorTextos.Find(nombreObjeto);
        return hijo != null ? hijo.GetComponent<TMP_Text>() : null;
    }

    public void Rellenar(ObraInformacion obra)
    {
        if (obra == null) return;

        AsegurarReferencias();

        if (textoAutor != null) textoAutor.text = obra.Autor;
        if (textoTitulo != null) textoTitulo.text = obra.Titulo;
        if (textoFecha != null) textoFecha.text = obra.Fecha;
        if (textoTecnica != null) textoTecnica.text = obra.Tecnica;
    }

    public void Limpiar()
    {
        AsegurarReferencias();

        if (textoAutor != null) textoAutor.text = string.Empty;
        if (textoTitulo != null) textoTitulo.text = string.Empty;
        if (textoFecha != null) textoFecha.text = string.Empty;
        if (textoTecnica != null) textoTecnica.text = string.Empty;
    }
}