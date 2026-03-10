using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    private Coroutine _lerpCoroutine;

    public bool IsPaused { get; private set; } = false;
    private float _targetTimeScale = 1f;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Immediate time scale set (pause/play)
    /// </summary>
    public void SetTimeScale(float timeScale)
    {
        _targetTimeScale = timeScale;

        // Only apply if not paused
        if (!IsPaused)
        {
            Time.timeScale = _targetTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    /// <summary>
    /// Smoothly lerp time scale (e.g., for slow motion)
    /// </summary>
    public void LerpTimeScale(float to, float duration)
    {
        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _lerpCoroutine = StartCoroutine(LerpRoutine(to, duration));
    }

    private IEnumerator LerpRoutine(float to, float duration)
    {
        float start = Time.timeScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _targetTimeScale = Mathf.Lerp(start, to, t / duration);

            // Only apply if not paused
            if (!IsPaused)
            {
                Time.timeScale = _targetTimeScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }

            yield return null;
        }

        // Final value
        if (!IsPaused)
        {
            Time.timeScale = to;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    /// <summary>
    /// Pause or unpause game
    /// </summary>
    public void SetPause(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : _targetTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public float GetTimeScale()
    {
        return Time.timeScale;
    }
}