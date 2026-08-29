using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// КНОПКА-ПЕРЕКЛЮЧАТЕЛЬ БЛОКНОТА (выпадающее окно).
//  • Первое нажатие — блокнот плавно "выпадает" сверху.
//  • Повторное нажатие — окно прячется обратно.
//
// УСТАНОВКА: повесить на кнопку (Button) в углу экрана.
// Панель блокнота найдётся сама через OrderNotepad.
// ============================================================
[RequireComponent(typeof(Button))]
public class NotepadToggle : MonoBehaviour
{
    [Header("Панель блокнота (найдётся сама, если пусто)")]
    public RectTransform dropdownPanel;

    [Header("Настройки анимации")]
    [Tooltip("Куда уезжает окно, когда спрятано. (0,0) — посчитать автоматически по высоте панели")]
    public Vector2 hiddenOffset = new Vector2(0f, 0f);
    [Tooltip("Длительность анимации в секундах (0 — мгновенно)")]
    public float slideDuration = 0.25f;
    [Tooltip("Окно открыто при старте игры?")]
    public bool startOpen = false;

    [Header("Надпись на кнопке (необязательно)")]
    public Text buttonText;
    public string openLabel = "ЗАКАЗ ▼";
    public string closeLabel = "ЗАКАЗ ▲";

    Vector2 shownPosition;
    bool isOpen;
    Coroutine animation;
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(ToggleWindow);

        // Если панель не назначена — ищем блокнот на сцене
        if (dropdownPanel == null)
        {
            OrderNotepad notepad = FindObjectOfType<OrderNotepad>();
            if (notepad != null) dropdownPanel = (RectTransform)notepad.transform;
        }

        if (dropdownPanel == null)
        {
            Debug.LogError("NotepadToggle: не найден блокнот OrderNotepad! Добавьте его на сцену.");
            return;
        }

        // Если смещение не задано — прячем окно за верхний край по его высоте
        if (hiddenOffset == Vector2.zero)
        {
            hiddenOffset = new Vector2(0f, dropdownPanel.sizeDelta.y + 80f);
        }

        if (buttonText == null) buttonText = GetComponentInChildren<Text>();

        shownPosition = dropdownPanel.anchoredPosition;

        if (startOpen)
        {
            isOpen = true;
            dropdownPanel.gameObject.SetActive(true);
        }
        else
        {
            isOpen = false;
            dropdownPanel.anchoredPosition = shownPosition + hiddenOffset;
            dropdownPanel.gameObject.SetActive(false);
        }

        UpdateLabel();
    }

    // ============================================================
    //  ПУБЛИЧНЫЕ МЕТОДЫ (можно вешать на кнопки в Инспекторе)
    // ============================================================

    // Переключить: открыто -> спрятать, спрятано -> открыть
    public void ToggleWindow()
    {
        if (isOpen) Hide();
        else Show();
    }

    public void Show()
    {
        if (dropdownPanel == null) return;
        isOpen = true;
        dropdownPanel.gameObject.SetActive(true);
        PlayAnimation(shownPosition, false);
        UpdateLabel();
    }

    public void Hide()
    {
        if (dropdownPanel == null) return;
        isOpen = false;
        PlayAnimation(shownPosition + hiddenOffset, true);
        UpdateLabel();
    }

    // ============================================================
    //  АНИМАЦИЯ
    // ============================================================
    void PlayAnimation(Vector2 target, bool disableAtEnd)
    {
        if (animation != null) StopCoroutine(animation);

        if (slideDuration <= 0f || !dropdownPanel.gameObject.activeInHierarchy)
        {
            dropdownPanel.anchoredPosition = target;
            if (disableAtEnd) dropdownPanel.gameObject.SetActive(false);
            return;
        }

        animation = StartCoroutine(SlideRoutine(target, disableAtEnd));
    }

    IEnumerator SlideRoutine(Vector2 target, bool disableAtEnd)
    {
        Vector2 start = dropdownPanel.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / slideDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            dropdownPanel.anchoredPosition = Vector2.Lerp(start, target, smooth);
            yield return null;
        }

        dropdownPanel.anchoredPosition = target;
        if (disableAtEnd) dropdownPanel.gameObject.SetActive(false);
    }

    void UpdateLabel()
    {
        if (buttonText != null)
        {
            buttonText.text = isOpen ? closeLabel : openLabel;
        }
    }
}
