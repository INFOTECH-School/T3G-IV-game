using UnityEngine;
using System.Collections;

public class InteractionFeedback : MonoBehaviour
{
    public enum FeedbackTarget { Renderer, ParticleSystem }

    [Header("Ustawienia Feedbacku")]
    [Tooltip("Wybierz czy feedback działa na Renderer czy ParticleSystem.")]
    public FeedbackTarget feedbackTarget = FeedbackTarget.Renderer;

    [Tooltip("Kolor informujący o braku możliwości interakcji.")]
    public Color errorColor = Color.red;
    
    [Tooltip("Czas w sekundach potrzebny na płynny powrót do oryginalnego koloru.")]
    public float transitionDuration = 1.0f;

    private Renderer _renderer;
    private ParticleSystem _particleSystem;
    private Color _originalColor;
    private Coroutine _feedbackCoroutine;

    private void Start()
    {
        switch (feedbackTarget)
        {
            case FeedbackTarget.Renderer:
                _renderer = GetComponent<Renderer>();
                if (_renderer && _renderer.material)
                    _originalColor = _renderer.material.color;
                break;

            case FeedbackTarget.ParticleSystem:
                _particleSystem = GetComponent<ParticleSystem>();
                if (_particleSystem)
                    _originalColor = _particleSystem.main.startColor.color;
                break;
        }
    }

    /// <summary>
    /// Wywołaj tę metodę z zewnątrz (np. ze skryptu gracza), 
    /// gdy nastąpi próba niedozwolonej interakcji.
    /// </summary>
    public void ShowErrorFeedback()
    {
        // Zabezpieczenie: jeśli animacja koloru już trwa, przerywamy ją
        if (_feedbackCoroutine != null)
        {
            StopCoroutine(_feedbackCoroutine);
        }

        // Natychmiastowa zmiana koloru na errorColor
        SetColor(errorColor);

        // Uruchomienie nowej korutyny, która płynnie przywróci kolor
        _feedbackCoroutine = StartCoroutine(RevertColorRoutine());
    }

    private IEnumerator RevertColorRoutine()
    {
        float elapsedTime = 0f;
        Color startColor = GetCurrentColor();

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            Color lerpedColor = Color.Lerp(startColor, _originalColor, elapsedTime / transitionDuration);
            SetColor(lerpedColor);
            yield return null;
        }

        // Upewnienie się, że na samym końcu kolor to dokładnie ten oryginalny
        SetColor(_originalColor);
        _feedbackCoroutine = null;
    }

    private void SetColor(Color color)
    {
        switch (feedbackTarget)
        {
            case FeedbackTarget.Renderer:
                if (_renderer && _renderer.material)
                    _renderer.material.color = color;
                break;

            case FeedbackTarget.ParticleSystem:
                if (_particleSystem)
                {
                    var main = _particleSystem.main;
                    main.startColor = color;
                }
                break;
        }
    }

    private Color GetCurrentColor()
    {
        switch (feedbackTarget)
        {
            case FeedbackTarget.Renderer:
                return (_renderer && _renderer.material) ? _renderer.material.color : _originalColor;

            case FeedbackTarget.ParticleSystem:
                return _particleSystem ? _particleSystem.main.startColor.color : _originalColor;

            default:
                return _originalColor;
        }
    }
}
