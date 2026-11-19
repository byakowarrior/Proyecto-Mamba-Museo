using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FigmaJsonToCanvas : EditorWindow
{
    private TextAsset jsonFile;

    [MenuItem("Tools/Figma JSON a Canvas")]
    public static void ShowWindow()
    {
        GetWindow<FigmaJsonToCanvas>("Figma a Canvas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Importador de Diseño Figma (Coordenadas Exactas)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("Archivo JSON de Figma", jsonFile, typeof(TextAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generar Canvas en la Escena", GUILayout.Height(35)))
        {
            if (jsonFile != null)
            {
                BuildCanvasFromJson(jsonFile.text);
            }
            else
            {
                Debug.LogError("Por favor, asigna un archivo .json válido.");
            }
        }
    }

    private void BuildCanvasFromJson(string jsonText)
    {
        // Como el JSON empieza con un arreglo '[', lo envolvemos para que JsonUtility lo lea sin problemas
        string wrappedJson = "{\"frames\":" + jsonText.Trim() + "}";
        FigmaFrameList frameList = JsonUtility.FromJson<FigmaFrameList>(wrappedJson);

        if (frameList == null || frameList.frames == null || frameList.frames.Count == 0)
        {
            Debug.LogError("No se pudo leer ningún Frame en el JSON.");
            return;
        }

        FigmaNode mainFrame = frameList.frames[0];
        float width = mainFrame.width > 0 ? mainFrame.width : 393f;
        float height = mainFrame.height > 0 ? mainFrame.height : 852f;

        // 1. Crear el Canvas Principal con resolución 393x852
        GameObject canvasObj = new GameObject(string.IsNullOrEmpty(mainFrame.name) ? "FigmaCanvas" : mainFrame.name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(width, height);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Fondo blanco base del Frame principal
        RectTransform rootRect = canvasObj.GetComponent<RectTransform>();
        Image bgImg = canvasObj.AddComponent<Image>();
        if (mainFrame.fillColorA > 0)
            bgImg.color = new Color(mainFrame.fillColorR, mainFrame.fillColorG, mainFrame.fillColorB, mainFrame.fillColorA);
        else
            bgImg.color = Color.white;

        // 3. Crear todos los hijos con sus posiciones exactas
        if (mainFrame.children != null)
        {
            foreach (var child in mainFrame.children)
            {
                CreateNodeRecursive(child, rootRect);
            }
        }

        Undo.RegisterCreatedObjectUndo(canvasObj, "Importar Canvas de Figma");
        Selection.activeGameObject = canvasObj;
        Debug.Log($"Canvas '{canvasObj.name}' generado con éxito ({width}x{height}) y {mainFrame.children.Count} elementos posicionados.");
    }

    private void CreateNodeRecursive(FigmaNode node, RectTransform parent)
    {
        GameObject go = new GameObject(string.IsNullOrEmpty(node.name) ? node.type : node.name);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        // Anclaje y pivote al centro (0.5, 0.5) porque el JSON usa el centro del frame como (0,0)
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // Invertir el eje Y porque en Figma Y positivo apunta hacia abajo y en Unity hacia arriba
        rect.anchoredPosition = new Vector2(node.x, -node.y);
        rect.sizeDelta = new Vector2(node.width, node.height);

        // Crear componente según tipo
        if (node.type == "TEXT")
        {
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = node.text;
            tmp.fontSize = node.textFontSize > 0 ? node.textFontSize : 16f;
            tmp.color = new Color(node.textColorR, node.textColorG, node.textColorB, node.textColorA);
            tmp.alignment = ParseTextAlignment(node.textAlignHorizontal, node.textAlignVertical);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;

            if (!string.IsNullOrEmpty(node.textFontStyle) && node.textFontStyle.ToLower().Contains("bold"))
            {
                tmp.fontStyle = FontStyles.Bold;
            }
        }
        else if (node.type == "RECTANGLE" || node.type == "ELLIPSE" || node.type == "FRAME" || node.type == "VECTOR")
        {
            if (node.fillColorA > 0)
            {
                Image img = go.AddComponent<Image>();
                img.color = new Color(node.fillColorR, node.fillColorG, node.fillColorB, node.fillColorA);
            }
        }

        // Si hubiera sub-hijos anidados
        if (node.children != null)
        {
            foreach (var child in node.children)
            {
                CreateNodeRecursive(child, rect);
            }
        }
    }

    private TextAlignmentOptions ParseTextAlignment(string horizontal, string vertical)
    {
        if (horizontal == "CENTER") return TextAlignmentOptions.Center;
        if (horizontal == "RIGHT") return TextAlignmentOptions.Right;
        return TextAlignmentOptions.TopLeft;
    }

    // --- Clases para mapear este formato de JSON ---
    [System.Serializable]
    public class FigmaFrameList
    {
        public List<FigmaNode> frames;
    }

    [System.Serializable]
    public class FigmaNode
    {
        public string name;
        public string type;
        public float x;
        public float y;
        public float width;
        public float height;
        public float opacity;

        // Colores de relleno (Formas / Vectores)
        public float fillColorR;
        public float fillColorG;
        public float fillColorB;
        public float fillColorA;

        // Propiedades de Texto
        public string text;
        public float textFontSize;
        public string textFontFamily;
        public string textFontStyle;
        public float textColorR;
        public float textColorG;
        public float textColorB;
        public float textColorA;
        public string textAlignHorizontal;
        public string textAlignVertical;

        public List<FigmaNode> children;
    }
}
