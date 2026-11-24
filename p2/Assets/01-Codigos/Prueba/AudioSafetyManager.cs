using UnityEngine;
using System.Collections;

public class AudioSafetyManager : MonoBehaviour
{
    private const int MUSIC_STREAM = 3;
    private const int OUTPUT_DEVICES = 2;

    [SerializeField, Range(0, 100)] private int maxVolumePercent = 30;
    [SerializeField] private float pollInterval = 0.15f;

    private AndroidJavaObject audioManager;

    private bool headphonesConnected;
    private int maxVolume;
    private int allowedVolume;

    // Para loguear solo cuando algo cambia
    private int lastLoggedVolume = -1;
    private bool lastLoggedHeadphones;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeAndroidAudio();
        StartCoroutine(AudioSafetyLoop());
#endif
    }

    private void InitializeAndroidAudio()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio");
        }

        maxVolume = audioManager.Call<int>("getStreamMaxVolume", MUSIC_STREAM);
        allowedVolume = Mathf.FloorToInt(maxVolume * (maxVolumePercent / 100f));
    }

    private IEnumerator AudioSafetyLoop()
    {
        var wait = new WaitForSecondsRealtime(pollInterval);

        while (true)
        {
            CheckHeadphones();
            ApplyVolumeLimit();
            yield return wait;
        }
    }

    private void CheckHeadphones()
    {
        AndroidJavaObject[] devices =
            audioManager.Call<AndroidJavaObject[]>("getDevices", OUTPUT_DEVICES);

        bool found = false;

        foreach (AndroidJavaObject device in devices)
        {
            int type = device.Call<int>("getType");

            // 3 = wired headset, 4 = wired headphones
            // 7 = Bluetooth SCO, 8 = Bluetooth A2DP
            // 22 = USB headset, 23 = hearing aid, 26 = BLE headset
            if (type == 3 || type == 4 || type == 7 || type == 8 ||
                type == 22 || type == 23 || type == 26)
            {
                found = true;
            }

            device.Dispose();
        }

        headphonesConnected = found;
    }

    private void ApplyVolumeLimit()
    {
        int currentVolume = audioManager.Call<int>("getStreamVolume", MUSIC_STREAM);

        if (!headphonesConnected && currentVolume > allowedVolume)
        {
            audioManager.Call("setStreamVolume", MUSIC_STREAM, allowedVolume, 0);
            currentVolume = allowedVolume;
            Debug.Log($"Volumen limitado a {allowedVolume}/{maxVolume}");
        }

        if (currentVolume != lastLoggedVolume || headphonesConnected != lastLoggedHeadphones)
        {
            lastLoggedVolume = currentVolume;
            lastLoggedHeadphones = headphonesConnected;

            Debug.Log(
                $"Audífonos: {headphonesConnected} | " +
                $"Volumen: {currentVolume}/{maxVolume} | " +
                $"Límite: {allowedVolume}");
        }
    }

    private void OnDestroy()
    {
        if (audioManager != null)
        {
            audioManager.Dispose();
            audioManager = null;
        }
    }
}