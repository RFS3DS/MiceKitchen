using UnityEngine;
using UnityEngine.UI;

// ============================================================
// КНОПКА «СЛЕДУЮЩИЙ КЛИЕНТ».
// ГЕНЕРАЦИЯ ЗАКАЗА ТЕПЕРЬ ПРОИСХОДИТ ЗДЕСЬ (точнее, в DayManager,
// который вызывается этой кнопкой). OrderManager сам больше ничего
// не генерирует — ни при старте, ни после выдачи заказа.
//
// УСТАНОВКА: повесить на кнопку (Button) рядом со станцией заказов.
// Кнопка автоматически блокируется, пока день не начат.
// ============================================================
[RequireComponent(typeof(Button))]
public class NextClientButton : MonoBehaviour
{
    [Header("Надпись кнопки (необязательно)")]
    public Text label;
    [Tooltip("Если надпись пустая — написать «СЛЕДУЮЩИЙ КЛИЕНТ» автоматически")]
    public bool autoSetLabel = true;

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnClicked);

        if (label == null) label = GetComponentInChildren<Text>();
        if (autoSetLabel && label != null && string.IsNullOrEmpty(label.text))
        {
            label.text = "СЛЕДУЮЩИЙ\nКЛИЕНТ";
        }
    }

    void Update()
    {
        // Кнопка работает только пока идёт день
        bool dayActive = DayManager.Instance != null && DayManager.Instance.DayActive;
        if (button != null && button.interactable != dayActive)
        {
            button.interactable = dayActive;
        }
    }

    void OnClicked()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogError("NextClientButton: DayManager не найден на сцене! Добавьте пустой объект с DayManager.");
            return;
        }

        DayManager.Instance.CallNextClient();
    }
}
