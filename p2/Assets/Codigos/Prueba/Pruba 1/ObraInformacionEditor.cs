using UnityEngine;
using UnityEditor;
using Vuforia;

[CustomEditor(typeof(ObraInformacion))]
public class ObraInformacionEditor : Editor
{
    // =========================================================================
    // PANEL DE CONTROL: Ajusta estos valores en píxeles a tu gusto
    // =========================================================================
    private float anchoCajas = 400f;          // Ancho de todas las cajas de texto

    // Altos individuales para cada campo
    private float altoTitulo = 45f;
    private float altoAutor = 45f;
    private float altoDescripcion = 100f;
    private float altoFecha = 35f;
    private float altoTecnica = 60f;

    // Separaciones (en píxeles)
    private float espacioAntesDeEtiqueta = 12f; // Espacio arriba del título del campo
    private float espacioDespuesDeEtiqueta = 2f; // Espacio entre el título y su caja
    private float espacioDespuesDeCaja = 5f;     // Espacio debajo de la caja
    // =========================================================================

    private SerializedProperty sonidoProp;
    private SerializedProperty canvasProp;
    private SerializedProperty botonProp;
    private SerializedProperty tituloProp;
    private SerializedProperty autorProp;
    private SerializedProperty descripcionProp;
    private SerializedProperty fechaProp;
    private SerializedProperty tecnicaProp;

    private void OnEnable()
    {
        sonidoProp = serializedObject.FindProperty("Sonido");
        canvasProp = serializedObject.FindProperty("canvas");
        botonProp = serializedObject.FindProperty("Boton");
        tituloProp = serializedObject.FindProperty("Titulo");
        autorProp = serializedObject.FindProperty("Autor");
        descripcionProp = serializedObject.FindProperty("Descripcion");
        fechaProp = serializedObject.FindProperty("Fecha");
        tecnicaProp = serializedObject.FindProperty("Tecnica");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. Dibuja los campos de referencia (AudioSource y Canvas)
        EditorGUILayout.PropertyField(sonidoProp);
        EditorGUILayout.PropertyField(canvasProp);
        EditorGUILayout.PropertyField(botonProp);

        // 2. Dibuja cada caja de texto con sus medidas personalizadas
        DibujarCampoTexto("Título", tituloProp, altoTitulo);
        DibujarCampoTexto("Autor", autorProp, altoAutor);
        DibujarCampoTexto("Descripción", descripcionProp, altoDescripcion);
        DibujarCampoTexto("Fecha", fechaProp, altoFecha);
        DibujarCampoTexto("Técnica", tecnicaProp, altoTecnica);

        serializedObject.ApplyModifiedProperties();

        // =====================================================================
        // SECCIÓN DE BOTONES DE ESCENA Y HERRAMIENTAS
        // =====================================================================
        GUILayout.Space(15f);
        EditorGUILayout.LabelField("Herramientas de Vista y Edición", EditorStyles.boldLabel);
        GUILayout.Space(5f);

        // Estilo personalizado para alinear el texto a la izquierda con un margen interior
        GUIStyle estiloBotonIzquierda = new GUIStyle(GUI.skin.button);
        estiloBotonIzquierda.alignment = TextAnchor.MiddleLeft;
        estiloBotonIzquierda.padding.left = 15; // Margen en píxeles para que el texto no toque el borde

        // Botón 1: Enfocar marcador
        if (GUILayout.Button("01 - Enfocar marcador", estiloBotonIzquierda, GUILayout.Height(50f)))
        {
            EnfocarObjeto();
        }

        GUILayout.Space(5f);

        // Botón 2: Alinear vista de cámara con la de la escena
        if (GUILayout.Button("02 - Alinear vista de cámara con la escena", estiloBotonIzquierda, GUILayout.Height(50f)))
        {
            AlinearCamaraConEscena();
        }

        GUILayout.Space(5f);

        // Botón 3: Cambiar Physical Width de Vuforia a 1
        if (GUILayout.Button("03 - Establecer Physical Width a 1m", estiloBotonIzquierda, GUILayout.Height(50f)))
        {
            EstablecerAnchoTarget(1f);
        }

        GUILayout.Space(5f);

        // Botón 4: Rotar Canvas -90 en X, elevar 0.025 en Y y ajustar tamaño al ImageTarget
        if (GUILayout.Button("04 - Ajustar Canvas al Image Target", estiloBotonIzquierda, GUILayout.Height(50f)))
        {
            AjustarCanvasATarget();
        }

        GUILayout.Space(5f);

        // Botón 5: Duplicar objeto y desplazar en X
        if (GUILayout.Button("05 - Duplicar Obra", estiloBotonIzquierda, GUILayout.Height(50f)))
        {
            DuplicarObra();
        }

        GUILayout.Space(5f);

        // Fila de 2 columnas al 50% cada una del ancho total de la fila
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Escala Botón -", GUILayout.Height(50f), GUILayout.ExpandWidth(true)))
        {
            EscalarBoton(0.9f);
        }

        if (GUILayout.Button("Escala Botón +", GUILayout.Height(50f), GUILayout.ExpandWidth(true)))
        {
            EscalarBoton(1.1f);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DibujarCampoTexto(string etiqueta, SerializedProperty propiedad, float altoCaja)
    {
        GUILayout.Space(espacioAntesDeEtiqueta);
        EditorGUILayout.LabelField(etiqueta, EditorStyles.boldLabel);
        GUILayout.Space(espacioDespuesDeEtiqueta);

        GUIStyle estiloCaja = new GUIStyle(EditorStyles.textArea);
        estiloCaja.wordWrap = true;

        propiedad.stringValue = EditorGUILayout.TextArea(
            propiedad.stringValue,
            estiloCaja,
            GUILayout.Height(altoCaja)
        );

        GUILayout.Space(espacioDespuesDeCaja);
    }

    // --- LÓGICA DE LOS BOTONES ---

    private void EnfocarObjeto()
    {
        ObraInformacion script = (ObraInformacion)target;
        Selection.activeGameObject = script.gameObject;

        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }

    private void AlinearCamaraConEscena()
    {
        SceneView vistaEscena = SceneView.lastActiveSceneView;
        if (vistaEscena == null)
        {
            Debug.LogWarning("No hay ninguna ventana de Escena activa.");
            return;
        }

        ObraInformacion script = (ObraInformacion)target;

        Camera camaraObjetivo = script.GetComponent<Camera>();
        if (camaraObjetivo == null) camaraObjetivo = script.GetComponentInChildren<Camera>();
        if (camaraObjetivo == null) camaraObjetivo = Camera.main;

        if (camaraObjetivo != null)
        {
            Undo.RecordObject(camaraObjetivo.transform, "Alinear cámara con escena");

            camaraObjetivo.transform.position = vistaEscena.camera.transform.position;
            camaraObjetivo.transform.rotation = vistaEscena.camera.transform.rotation;
        }
        else
        {
            Debug.LogWarning("No se encontró una cámara en este objeto ni una 'Main Camera' en la escena.");
        }
    }

    private void EstablecerAnchoTarget(float nuevoAncho)
    {
        ObraInformacion script = (ObraInformacion)target;
        ImageTargetBehaviour imageTarget = script.GetComponentInParent<ImageTargetBehaviour>();

        if (imageTarget != null)
        {
            Undo.RecordObject(imageTarget, "Cambiar Physical Width a " + nuevoAncho);

            SerializedObject serializedTarget = new SerializedObject(imageTarget);
            serializedTarget.Update();

            SerializedProperty widthProp = serializedTarget.FindProperty("mWidth");
            SerializedProperty heightProp = serializedTarget.FindProperty("mHeight");

            if (widthProp != null)
            {
                float anchoActual = widthProp.floatValue;
                if (heightProp != null && anchoActual > 0f)
                {
                    float proporcion = heightProp.floatValue / anchoActual;
                    heightProp.floatValue = nuevoAncho * proporcion;
                }

                widthProp.floatValue = nuevoAncho;
                serializedTarget.ApplyModifiedProperties();
            }

            imageTarget.SetWidth(nuevoAncho);

            EditorUtility.SetDirty(imageTarget);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(imageTarget.gameObject.scene);

            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();

            Repaint();
        }
        else
        {
            Debug.LogWarning("No se encontró ningún componente ImageTargetBehaviour en este objeto ni en sus padres.");
        }
    }

    private void AjustarCanvasATarget()
    {
        ObraInformacion script = (ObraInformacion)target;
        Canvas canvasObjetivo = canvasProp.objectReferenceValue as Canvas;

        if (canvasObjetivo == null)
        {
            Debug.LogWarning("Asigna primero el Canvas en el campo 'Canvas' del Inspector.");
            return;
        }

        ImageTargetBehaviour imageTarget = script.GetComponentInParent<ImageTargetBehaviour>();
        if (imageTarget == null)
        {
            Debug.LogWarning("No se encontró ningún componente ImageTargetBehaviour en este objeto ni en sus padres.");
            return;
        }

        Vector2 tamañoTarget = ObtenerTamañoTarget(imageTarget);

        RectTransform rectCanvas = canvasObjetivo.GetComponent<RectTransform>();
        if (rectCanvas != null)
        {
            Undo.RecordObject(rectCanvas, "Ajustar Canvas al Image Target");

            // 1. Girar -90 grados en X (respetando Y y Z actuales)
            Vector3 rotacionActual = rectCanvas.localEulerAngles;
            rectCanvas.localEulerAngles = new Vector3(-90f, rotacionActual.y, rotacionActual.z);

            // 2. Separar 0.025 en el eje Y local
            rectCanvas.localPosition = new Vector3(rectCanvas.localPosition.x, 0.025f, rectCanvas.localPosition.z);

            // 3. Ajustar Width y Height compensando la escala del Canvas
            float escalaX = Mathf.Abs(rectCanvas.localScale.x) > 0.00001f ? Mathf.Abs(rectCanvas.localScale.x) : 1f;
            float escalaY = Mathf.Abs(rectCanvas.localScale.y) > 0.00001f ? Mathf.Abs(rectCanvas.localScale.y) : 1f;

            rectCanvas.sizeDelta = new Vector2(
                tamañoTarget.x / escalaX,
                tamañoTarget.y / escalaY
            );

            EditorUtility.SetDirty(rectCanvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rectCanvas.gameObject.scene);
            SceneView.RepaintAll();
        }
    }

    private void DuplicarObra()
    {
        ObraInformacion script = (ObraInformacion)target;
        ImageTargetBehaviour imageTarget = script.GetComponentInParent<ImageTargetBehaviour>();

        float anchoTarget = imageTarget != null ? ObtenerTamañoTarget(imageTarget).x : 0f;
        float desplazamientoX = anchoTarget + 0.1f;
        GameObject duplicado = Instantiate(
        script.gameObject,
        script.transform.position + new Vector3(desplazamientoX, 0f, 0f),
        script.transform.rotation,
        script.transform.parent
        );
        duplicado.name = script.gameObject.name;
        GameObjectUtility.EnsureUniqueNameForSibling(duplicado);
        Undo.RegisterCreatedObjectUndo(duplicado, "Duplicar Obra");
        Selection.activeGameObject = duplicado;
        EditorGUIUtility.PingObject(duplicado);
    }
    // Método auxiliar para leer siempre el tamaño real en modo edición
    private Vector2 ObtenerTamañoTarget(ImageTargetBehaviour imageTarget)
    {
        Vector2 tamaño = imageTarget.GetSize();
        if (tamaño.x <= 0f || tamaño.y <= 0f)
        {
            SerializedObject serializedTarget = new SerializedObject(imageTarget);
            SerializedProperty widthProp = serializedTarget.FindProperty("mWidth");
            SerializedProperty heightProp = serializedTarget.FindProperty("mHeight");
            if (widthProp != null && heightProp != null)
            {
                tamaño = new Vector2(widthProp.floatValue, heightProp.floatValue);
            }
        }
        return tamaño;
    }

    // Escalar Botón
    private void EscalarBoton(float factor)
    {
        GameObject objetoBoton = botonProp.objectReferenceValue as GameObject;

        if (objetoBoton != null)
        {
            Undo.RecordObject(objetoBoton.transform, "Escalar Botón");
            objetoBoton.transform.localScale *= factor;
            EditorUtility.SetDirty(objetoBoton.transform);
            SceneView.RepaintAll();
        }
        else
        {
            Debug.LogWarning("Asigna primero el GameObject en el campo 'Boton' del Inspector.");
        }
    }
}