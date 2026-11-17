
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CargadorAssetBundle : MonoBehaviour
{
    [Header("URL del AssetBundle")]
    [SerializeField]
    private string assetBundleURL;

    // Ejecutar manualmente para descargar e instanciar
    [ContextMenu("DescargarAssetBundle")]
    public void DescargarAssetBundle()
    {
        StartCoroutine(Descargar());
    }

    private IEnumerator Descargar()
    {
        using (UnityWebRequest request =
            UnityWebRequestAssetBundle.GetAssetBundle(assetBundleURL))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al descargar: " + request.error);
                yield break;
            }

            AssetBundle bundle =
                DownloadHandlerAssetBundle.GetContent(request);

            if (bundle == null)
            {
                Debug.LogError("No se pudo cargar el AssetBundle.");
                yield break;
            }

            // Obtener todos los GameObjects del AssetBundle
            GameObject[] objetos =
                bundle.LoadAllAssets<GameObject>();

            if (objetos.Length == 0)
            {
                Debug.LogError("El AssetBundle no contiene GameObjects.");
                bundle.Unload(false);
                yield break;
            }

            // Instanciar el primer GameObject encontrado
            GameObject nuevoObjeto = Instantiate(
                objetos[0],
                Vector3.zero,
                Quaternion.identity
            );

            // Asignar nombre
            nuevoObjeto.name = "nuevo objeto";

            Debug.Log("AssetBundle instanciado correctamente.");

            // Liberar el contenedor
            bundle.Unload(false);
        }
    }
}
