using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SwipeCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referencias")]
    [SerializeField] private RectTransform content;
    [SerializeField] private Image indicator;             // Image única del indicador
    [SerializeField] private Sprite[] indicatorSprites;   // Un sprite por página, en orden

    [Header("Ajustes")]
    [SerializeField] private int startPage = 0;
    [SerializeField] private float snapDuration = 0.25f;
    [SerializeField] private float swipeThreshold = 0.2f;   // % del ancho para cambiar de página
    [SerializeField] private float flickSpeed = 600f;       // velocidad para flick rápido

    private RectTransform viewport;
    private Canvas rootCanvas;
    private int pageCount;
    private int currentPage;
    private float pageWidth;
    private float dragStartX;
    private float dragStartTime;
    private Coroutine snapRoutine;
    public event System.Action<int> PageChanged;   // avisa la página a la que se llegó
    public int PageCount => pageCount;
    private void Reset()
    {
        startPage = 0;
        snapDuration = 0.25f;
        swipeThreshold = 0.2f;
        flickSpeed = 600f;
    }

    private void Awake()
    {
        viewport = (RectTransform)transform;

        // El script mueve el Content por su cuenta: el ScrollRect se desactiva para que no interfiera
        var sr = GetComponent<ScrollRect>();
        if (sr != null) sr.enabled = false;

        var canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;
    }

    private IEnumerator Start()
    {
        // Esperar a que el layout se calcule
        yield return null;
        yield return new WaitForEndOfFrame();

        SetupPages();
        GoToPage(startPage, true);
    }

    private void SetupPages()
    {
        if (content == null) return;

        pageWidth = viewport.rect.width;
        if (pageWidth <= 0f || float.IsNaN(pageWidth)) return;

        // Content anclado a la izquierda, con pivote en el borde izquierdo
        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 0.5f);

        pageCount = content.childCount;

        foreach (RectTransform page in content)
        {
            var le = page.GetComponent<LayoutElement>();
            if (le == null) le = page.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = pageWidth;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (content == null || viewport == null || pageCount == 0) return;
        if (Mathf.Approximately(pageWidth, viewport.rect.width)) return;

        SetupPages();
        GoToPage(currentPage, true);
    }

    private float ScreenToCanvas
    {
        get
        {
            float s = (rootCanvas != null) ? rootCanvas.scaleFactor : 1f;
            return s > 0f ? 1f / s : 1f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (pageCount == 0 || pageWidth <= 0f) return;

        if (snapRoutine != null) StopCoroutine(snapRoutine);

        // Recalcular la página actual según la posición real (evita desincronización)
        currentPage = Mathf.Clamp(Mathf.RoundToInt(-content.anchoredPosition.x / pageWidth), 0, pageCount - 1);

        dragStartX = content.anchoredPosition.x;
        dragStartTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pageCount == 0 || pageWidth <= 0f) return;

        float x = content.anchoredPosition.x + eventData.delta.x * ScreenToCanvas;
        float minX = -(pageCount - 1) * pageWidth;
        float maxX = 0f;

        // Un poco de resistencia al pasarse de los extremos
        if (x > maxX) x = maxX + (x - maxX) * 0.3f;
        else if (x < minX) x = minX + (x - minX) * 0.3f;

        content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (pageCount == 0 || pageWidth <= 0f) return;

        float delta = content.anchoredPosition.x - dragStartX;
        float time = Mathf.Max(Time.unscaledTime - dragStartTime, 0.01f);
        float speed = delta / time;

        int target = currentPage;

        bool farEnough = Mathf.Abs(delta) > pageWidth * Mathf.Max(swipeThreshold, 0.01f);
        bool fastEnough = Mathf.Abs(speed) > flickSpeed && Mathf.Abs(delta) > 20f;

        if (farEnough || fastEnough)
            target += delta < 0 ? 1 : -1; // izquierda = siguiente, derecha = anterior

        GoToPage(target);
    }

    public void GoToPage(int page, bool instant = false)
    {
        if (pageCount == 0 || pageWidth <= 0f) return;

        currentPage = Mathf.Clamp(page, 0, pageCount - 1);
        float targetX = -currentPage * pageWidth;

        UpdateIndicator();
        PageChanged?.Invoke(currentPage);

        if (snapRoutine != null) StopCoroutine(snapRoutine);

        if (instant)
        {
            content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
            return;
        }
        snapRoutine = StartCoroutine(SnapTo(targetX));
    }

    private IEnumerator SnapTo(float targetX)
    {
        float startX = content.anchoredPosition.x;

        if (float.IsNaN(startX) || float.IsInfinity(startX))
        {
            content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
            yield break;
        }

        float duration = Mathf.Max(snapDuration, 0.01f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease-out cubic
            content.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, eased), content.anchoredPosition.y);
            yield return null;
        }
        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
    }

    private void UpdateIndicator()
    {
        if (indicator == null || indicatorSprites == null) return;
        if (currentPage < 0 || currentPage >= indicatorSprites.Length) return;
        if (indicatorSprites[currentPage] == null) return;

        indicator.sprite = indicatorSprites[currentPage];
    }

    // Útil para botones "Siguiente" / "Saltar"
    public void Next() => GoToPage(currentPage + 1);
    public void Previous() => GoToPage(currentPage - 1);
}