
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Obra
{
    public string asset_bundle_url;
    public Ficha ficha;
}

[Serializable]
public class Ficha
{
    public string autor;
    public string titulo;
    public string contenido;
}

[Serializable]
public class ListaObras
{
    public List<Obra> obras;
}

public class GestorObras : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField]
    private string googleDriveURL =
        "https://drive.google.com/file/d/ID_DEL_ARCHIVO/view";

    [Header("Datos")]
    public ListaObras datos = new ListaObras();

    [Header("Estado")]
    public bool cargando;
    public bool cargado;

    //private void Start() 
    //{
    //    StartCoroutine(DescargarJSON());
    //}

    [ContextMenu("Cargar JSON")]
    public void CargarJSON()
    {
        StartCoroutine(DescargarJSON());
    }

    private IEnumerator DescargarJSON()
    {
        cargando = true;
        cargado = false;

        string fileID = ObtenerIDGoogleDrive(googleDriveURL);

        if (string.IsNullOrEmpty(fileID))
        {
            Debug.LogError("No se pudo obtener el ID de Google Drive.");
            cargando = false;
            yield break;
        }

        string urlDescarga =
            "https://drive.google.com/uc?export=download&id=" + fileID;

        using (UnityWebRequest request = UnityWebRequest.Get(urlDescarga))
        {
            request.timeout = 30;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("JSON descargado correctamente.");

                string json = request.downloadHandler.text;

                // Comprobar que la respuesta sea JSON
                json = json.Trim();

                if (json.StartsWith("["))
                {
                    // Convertir array raíz en objeto compatible
                    json = "{\"obras\":" + json + "}";
                }

                if (!json.StartsWith("{"))
                {
                    Debug.LogError("La respuesta no contiene un JSON válido.");
                    cargando = false;
                    yield break;
                }

                CargarJSON(json);
            }
            else
            {
                Debug.LogError(
                    "Error descargando JSON: " + request.error
                );
            }
        }

        cargando = false;
    }

    private string ObtenerIDGoogleDrive(string url)
    {
        try
        {
            Uri uri = new Uri(url);

            string[] parametros =
                uri.Query.TrimStart('?').Split('&');

            foreach (string parametro in parametros)
            {
                string[] partes = parametro.Split('=');

                if (partes.Length == 2 && partes[0] == "id")
                {
                    return Uri.UnescapeDataString(partes[1]);
                }
            }

            string[] segmentos = uri.AbsolutePath.Split('/');

            for (int i = 0; i < segmentos.Length - 1; i++)
            {
                if (segmentos[i] == "d")
                {
                    return segmentos[i + 1];
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error leyendo URL: " + e.Message);
        }

        return null;
    }

    private void CargarJSON(string json)
    {
        try
        {
            ListaObras resultado =
                JsonUtility.FromJson<ListaObras>(json);

            if (resultado == null || resultado.obras == null)
            {
                Debug.LogError("No se encontraron obras.");
                return;
            }

            // ASIGNAR AUTOMATICAMENTE LOS DATOS AL INSPECTOR
            datos = resultado;

            cargado = true;

            Debug.Log("================================");
            Debug.Log("OBRAS CARGADAS CORRECTAMENTE");
            Debug.Log("Total: " + datos.obras.Count);
            Debug.Log("================================");

            foreach (Obra obra in datos.obras)
            {
                if (obra == null || obra.ficha == null)
                    continue;

                Debug.Log("Autor: " + obra.ficha.autor);
                Debug.Log("Título: " + obra.ficha.titulo);
                Debug.Log("Contenido: " + obra.ficha.contenido);
                Debug.Log("Bundle: " + obra.asset_bundle_url);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error procesando JSON: " + e.Message);
        }
    }

    public Obra ObtenerObra(int indice)
    {
        if (datos == null || datos.obras == null)
            return null;

        if (indice < 0 || indice >= datos.obras.Count)
            return null;

        return datos.obras[indice];
    }
}
