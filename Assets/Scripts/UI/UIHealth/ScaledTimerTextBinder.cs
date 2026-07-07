using TMPro;
using UnityEngine;

public class ScaledTimerTextBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private bool startOnEnable = true;
    [SerializeField] private bool resetOnEnable = true;

    private float elapsedSeconds;
    private bool running;

    private void Reset()
    {
        AutoBind();
    }

    private void Awake()
    {
        AutoBind();
    }

    private void OnEnable()
    {
        AutoBind();

        if (resetOnEnable)
        {
            ResetTimer();
        }

        running = startOnEnable;
        RefreshView();
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        RefreshView();
    }

    public void StartTimer()
    {
        running = true;
    }

    public void PauseTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        elapsedSeconds = 0f;
        RefreshView();
    }

    private void RefreshView()
    {
        if (timeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void AutoBind()
    {
        if (timeText == null)
        {
            timeText = GetComponent<TMP_Text>();
        }
    }
}
