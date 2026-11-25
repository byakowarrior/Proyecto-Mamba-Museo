using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TutorialTimer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SwipeCarousel carousel;
    [SerializeField] private GameObject timerRoot;   // objeto que contiene el temporizador
    [SerializeField] private TMP_Text timerText;     // texto con el número

    [Header("Ajustes")]
    [SerializeField] private float seconds = 5f;
    [SerializeField] private string nextScene = "NombreDeTuEscena";

    private bool started;

    private void Awake()
    {
        if (timerRoot != null) timerRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (carousel != null) carousel.PageChanged += OnPageChanged;
    }

    private void OnDisable()
    {
        if (carousel != null) carousel.PageChanged -= OnPageChanged;
    }

    private void OnPageChanged(int page)
    {
        if (started) return; // una vez iniciada, la cuenta ya no se cancela ni se reinicia

        if (page == carousel.PageCount - 1)
        {
            started = true;
            StartCoroutine(Countdown());
        }
    }

    private IEnumerator Countdown()
    {
        if (timerRoot != null) timerRoot.SetActive(true);

        float remaining = seconds;
        while (remaining > 0f)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(remaining).ToString();

            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (timerText != null) timerText.text = "0";
        SceneManager.LoadScene(nextScene);
    }
}