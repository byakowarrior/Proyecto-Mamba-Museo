using System.Collections;
using UnityEngine;

public class AndroidHeadphoneNotifier : MonoBehaviour
{
    private const int OUTPUT_DEVICES = 2;

    [Header("Canvas a Controlar")]
    [Tooltip("Canvas o panel que se activa cuando NO hay audífonos y se desactiva cuando SÍ hay")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Aviso Nativo de Android (Toast)")]
    [SerializeField] private string toastMessage = "Conecta audífonos (cable o Bluetooth) para continuar.";
    [SerializeField] private bool showToastLong = true;
    [SerializeField] private float checkInterval = 2.0f;
    [SerializeField] private bool showOnStart = true;

    [Header("Bloqueo y Omisión")]
    [SerializeField] private bool pauseTimeWhileMissing = false;
    [SerializeField] private bool allowSkipByCode = false;

#if UNITY_EDITOR
    [Header("Pruebas en Editor")]
    [SerializeField] private bool allowSkipWithF1InEditor = true;
#endif

    private AndroidJavaObject audioManager;
    private AndroidJavaObject currentActivity;
    private bool hasHeadphones;
    private bool skipped;
    private float savedTimeScale = 1f;
    private bool timePaused;

    private void Awake()
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeAndroid();
        hasHeadphones = AreHeadphonesConnected();

        UpdateCanvasState(hasHeadphones);

        if (!hasHeadphones && !skipped)
        {
            if (showOnStart)
            {
                ShowNativeToast(toastMessage);
            }
            SetPaused(pauseTimeWhileMissing);
        }

        StartCoroutine(MonitorLoop());
#else
        // En el Editor simulamos que no hay audífonos para probar el Canvas
        hasHeadphones = false;
        UpdateCanvasState(hasHeadphones);

        if (showOnStart && !skipped)
        {
            Debug.Log($"[Editor Sim] Toast Android: {toastMessage}");
            SetPaused(pauseTimeWhileMissing);
        }
#endif
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (allowSkipWithF1InEditor && (timePaused || (targetCanvas != null && targetCanvas.gameObject.activeSelf)) && Input.GetKeyDown(KeyCode.F1))
        {
            SkipNotice(force: true);
        }
    }
#endif

    public void SkipNotice(bool force = false)
    {
        if (!allowSkipByCode && !force) return;
        skipped = true;
        UpdateCanvasState(true);
        SetPaused(false);
    }

    private void UpdateCanvasState(bool headphonesConnected)
    {
        if (targetCanvas == null) return;

        bool shouldShowCanvas = !headphonesConnected && !skipped;
        if (targetCanvas.gameObject.activeSelf != shouldShowCanvas)
        {
            targetCanvas.gameObject.SetActive(shouldShowCanvas);
        }
    }

    private IEnumerator MonitorLoop()
    {
        var wait = new WaitForSecondsRealtime(checkInterval);

        while (true)
        {
            bool connectedNow = AreHeadphonesConnected();

            UpdateCanvasState(connectedNow);

            if (connectedNow && !hasHeadphones)
            {
                skipped = false;
                UpdateCanvasState(true);
                SetPaused(false);
            }
            else if (!connectedNow && hasHeadphones)
            {
                skipped = false;
                UpdateCanvasState(false);
                if (!skipped)
                {
                    ShowNativeToast(toastMessage);
                    SetPaused(pauseTimeWhileMissing);
                }
            }
            else if (!connectedNow && !skipped && pauseTimeWhileMissing && !timePaused)
            {
                SetPaused(true);
            }

            hasHeadphones = connectedNow;
            yield return wait;
        }
    }

    private void InitializeAndroid()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            audioManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "audio");
        }
    }

    private bool AreHeadphonesConnected()
    {
        if (audioManager == null) return false;

        AndroidJavaObject[] devices = audioManager.Call<AndroidJavaObject[]>("getDevices", OUTPUT_DEVICES);
        if (devices == null) return false;

        bool found = false;
        for (int i = 0; i < devices.Length; i++)
        {
            AndroidJavaObject device = devices[i];
            if (device == null) continue;

            int type = device.Call<int>("getType");
            if (type == 3 || type == 4 || type == 7 || type == 8 ||
                type == 22 || type == 23 || type == 26 || type == 27)
            {
                found = true;
            }
            device.Dispose();
        }
        return found;
    }

    public void ShowNativeToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (currentActivity == null) InitializeAndroid();
        if (currentActivity == null) return;

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            using (var toastClass = new AndroidJavaClass("android.widget.Toast"))
            using (var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                int duration = showToastLong
                    ? toastClass.GetStatic<int>("LENGTH_LONG")
                    : toastClass.GetStatic<int>("LENGTH_SHORT");

                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", context, message, duration);
                toast.Call("show");
            }
        }));
#else
        Debug.Log($"[Toast]: {message}");
#endif
    }

    private void SetPaused(bool pause)
    {
        if (pause && !timePaused)
        {
            savedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            timePaused = true;
        }
        else if (!pause && timePaused)
        {
            Time.timeScale = savedTimeScale;
            timePaused = false;
        }
    }

    private void OnDestroy()
    {
        SetPaused(false);
        if (audioManager != null) audioManager.Dispose();
        if (currentActivity != null) currentActivity.Dispose();
    }
}