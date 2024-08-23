using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    private static NotificationManager _instance;
    public static NotificationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotificationManager>();
                if (_instance == null)
                {
                    Debug.LogError("NotificationManager instance not found in the scene!");
                }
            }
            return _instance;
        }
    }

    // Variables specific to NotificationManager
    [SerializeField] private Image screenOverlay;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private int flashCount = 3;

    private Coroutine currentFlashCoroutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.Log($"{nameof(NotificationManager)} instance already exists, destroying duplicate!");
            Destroy(gameObject);
        }
    }

    public void ShowScreenOverlay(Color color)
    {
        if (screenOverlay == null)
        {
            Debug.LogWarning("Screen overlay image is not assigned in NotificationManager!");
            return;
        }

        // Stop any existing coroutine
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }

        currentFlashCoroutine = StartCoroutine(FlashScreenOverlay(color));
    }

    private IEnumerator FlashScreenOverlay(Color color)
    {
        Color transparentColor = new Color(color.r, color.g, color.b, 0);
        screenOverlay.color = transparentColor;
        screenOverlay.gameObject.SetActive(true);

        for (int i = 0; i < flashCount; i++)
        {
            // Fade in
            yield return StartCoroutine(FadeOverlay(transparentColor, color, fadeDuration));
            // Fade out
            yield return StartCoroutine(FadeOverlay(color, transparentColor, fadeDuration));
        }

        screenOverlay.gameObject.SetActive(false);
    }

    private IEnumerator FadeOverlay(Color startColor, Color endColor, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            screenOverlay.color = Color.Lerp(startColor, endColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        screenOverlay.color = endColor;
    }
}