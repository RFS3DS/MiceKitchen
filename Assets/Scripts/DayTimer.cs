using UnityEngine;
using UnityEngine.UI;

// ============================================================
// ТАЙМЕР ДНЯ.
//  • Запускается автоматически, когда день начинается (кнопка в кабинете).
//  • Сбрасывается в 00:00 в начале каждого дня.
//  • Показывает время в ТЕКСТОВОМ ПОЛЕ В ВЕРХНЕМ ПРАВОМ УГЛУ.
//
// УСТАНОВКА: добавить компонент на пустой объект в сцене.
// Если поле timerText пустое — текст создастся сам в правом верхнем углу.
// ============================================================
public class DayTimer : MonoBehaviour
{
    [Header("Текст таймера (создастся сам в правом верхнем углу, если пусто)")]
    public Text timerText;

    [Header("Настройки")]
    [Tooltip("Показывать ОСТАВШЕЕСЯ время дня вместо прошедшего")]
    public bool showRemaining = false;
    public string prefix = "Время: ";
    public int fontSize = 40;

    void Start()
    {
        if (timerText == null)
        {
            timerText = CreateDefaultText();
        }
    }

    void Update()
    {
        if (timerText == null) return;
        if (DayManager.Instance == null) return;

        if (!DayManager.Instance.DayActive)
        {
            timerText.text = prefix + "00:00";
            return;
        }

        float value = showRemaining ? DayManager.Instance.TimeLeft : DayManager.Instance.Elapsed;
        timerText.text = prefix + UiHelpers.FormatTime(value);
    }

    // Текст в правом верхнем углу экрана
    Text CreateDefaultText()
    {
        Canvas canvas = UiHelpers.EnsureCanvas("TimerCanvas", 70);

        Text t = UiHelpers.CreateText(canvas.transform, "DayTimerText", prefix + "00:00",
            fontSize, TextAnchor.UpperRight, Color.white);
        t.fontStyle = FontStyle.Bold;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-24f, -20f);
        rt.sizeDelta = new Vector2(540f, fontSize + 16f);

        // Обводка, чтобы время читалось на любом фоне
        Outline outline = t.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        return t;
    }
}
