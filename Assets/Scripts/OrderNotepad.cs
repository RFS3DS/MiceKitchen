using UnityEngine;
using UnityEngine.UI;

// ============================================================
// БЛОКНОТ ЗАКАЗОВ — визуальная сборка заказа.
// Показывает: имя и тип клиента, его ожидание и список ингредиентов.
// Точки ●●○ заполняются по мере добавления кусочков на пиццу.
//
// УСТАНОВКА: добавить компонент на пустой объект ПОД канвасом
// (или на пустой объект в сцене — канвас создастся сам).
// Все тексты создаются автоматически.
// ============================================================
[RequireComponent(typeof(RectTransform))]
public class OrderNotepad : MonoBehaviour
{
    public static OrderNotepad Instance;

    [Header("Тексты (создадутся автоматически, если оставить пустыми)")]
    public Text headerText;     // имя и тип клиента
    public Text patienceText;   // сколько клиент ещё готов ждать
    public Text orderText;      // список ингредиентов с точками

    [Header("Размер блокнота")]
    [Tooltip("Размер панели, если автоподгон выключена")]
    public Vector2 panelSize = new Vector2(620f, 800f);
    [Tooltip("Автоматически подгонять блокнот под размер экрана (рекомендуется)")]
    public bool fitToScreen = true;
    [Tooltip("Какую долю ШИРИНЫ экрана занимает блокнот")]
    [Range(0.25f, 0.6f)] public float screenWidthFraction = 0.42f;
    [Tooltip("Какую долю ВЫСОТЫ экрана занимает блокнот")]
    [Range(0.35f, 0.85f)] public float screenHeightFraction = 0.70f;

    // Цвета блокнота
    static readonly Color PaperColor = new Color(0.98f, 0.94f, 0.80f, 0.96f);
    static readonly Color InkColor = new Color(0.16f, 0.13f, 0.10f);

    RectTransform root;
    bool subscribed;
    bool orderCompleted; // заказ полностью собран

    void Awake()
    {
        Instance = this;
        root = (RectTransform)transform;

        // Если блокнот не под канвасом — переносим на новый канвас
        if (GetComponentInParent<Canvas>() == null)
        {
            transform.SetParent(UiHelpers.EnsureCanvas("NotepadCanvas", 40).transform, false);
        }

        BuildUiIfNeeded();
    }

    void Start()
    {
        ApplySize(); // канвас уже точно знает размер — подгоняем точнее
        TrySubscribe();
        Rebuild();
    }

    void Update()
    {
        if (!subscribed) TrySubscribe();
        UpdatePatience();
    }

    // ============================================================
    //  АВТОПОСТРОЙКА UI
    // ============================================================
    void BuildUiIfNeeded()
    {
        if (GetComponent<Image>() == null)
        {
            Image bg = gameObject.AddComponent<Image>();
            bg.color = PaperColor; // бумага блокнота
        }

        // Позиция по умолчанию: левый верхний угол
        if (root.sizeDelta == Vector2.zero || root.sizeDelta == new Vector2(100f, 100f))
        {
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = new Vector2(16f, -16f);
        }
        ApplySize();

        if (headerText == null)
        {
            headerText = UiHelpers.CreateText(transform, "Header", "БЛОКНОТ ЗАКАЗОВ", 34, TextAnchor.UpperLeft, InkColor);
            headerText.fontStyle = FontStyle.Bold;
            headerText.rectTransform.anchorMin = new Vector2(0f, 1f);
            headerText.rectTransform.anchorMax = new Vector2(1f, 1f);
            headerText.rectTransform.pivot = new Vector2(0.5f, 1f);
            headerText.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            headerText.rectTransform.sizeDelta = new Vector2(-28f, 88f);
            BestFit(headerText, 22, 46);
        }

        if (patienceText == null)
        {
            patienceText = UiHelpers.CreateText(transform, "Patience", "", 30, TextAnchor.UpperLeft, InkColor);
            patienceText.fontStyle = FontStyle.Bold;
            patienceText.rectTransform.anchorMin = new Vector2(0f, 1f);
            patienceText.rectTransform.anchorMax = new Vector2(1f, 1f);
            patienceText.rectTransform.pivot = new Vector2(0.5f, 1f);
            patienceText.rectTransform.anchoredPosition = new Vector2(0f, -108f);
            patienceText.rectTransform.sizeDelta = new Vector2(-28f, 48f);
            BestFit(patienceText, 20, 38);
        }

        if (orderText == null)
        {
            orderText = UiHelpers.CreateText(transform, "Order", "", 34, TextAnchor.UpperLeft, InkColor);
            UiHelpers.Stretch(orderText.rectTransform, 16f, 166f, 16f, 16f);
            BestFit(orderText, 20, 52);
        }
    }

    // Подбор размера панели: автоподгон под экран либо panelSize из Инспектора
    void ApplySize()
    {
        Vector2 size = panelSize;

        if (fitToScreen)
        {
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null)
            {
                RectTransform crt = (RectTransform)c.transform;
                if (crt.rect.width > 1f && crt.rect.height > 1f)
                {
                    size = new Vector2(
                        Mathf.Clamp(crt.rect.width * screenWidthFraction, 440f, 1100f),
                        Mathf.Clamp(crt.rect.height * screenHeightFraction, 520f, 1400f));
                }
            }
        }

        root.sizeDelta = size;
    }

    // Автоподбор размера шрифта под область текста (Text сам растёт до max)
    static void BestFit(Text t, int min, int max)
    {
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = min;
        t.resizeTextMaxSize = max;
    }

    // ============================================================
    //  ПОДПИСКИ НА OrderManager
    // ============================================================
    void TrySubscribe()
    {
        if (subscribed || OrderManager.Instance == null) return;

        OrderManager.Instance.OnOrderGenerated += HandleOrderGenerated;
        OrderManager.Instance.OnOrderCleared += HandleOrderCleared;
        OrderManager.Instance.OnPieceAdded += HandlePieceAdded;
        OrderManager.Instance.OnOrderCompleted += HandleOrderCompleted;
        subscribed = true;
    }

    void HandleOrderGenerated() { orderCompleted = false; Rebuild(); }
    void HandleOrderCleared() { orderCompleted = false; Rebuild(); }
    void HandlePieceAdded(IngredientType type, int have, int need) { Rebuild(); }
    void HandleOrderCompleted() { orderCompleted = true; Rebuild(); }

    // ============================================================
    //  ОТРИСОВКА ЗАКАЗА
    // ============================================================
    void Rebuild()
    {
        if (headerText == null || orderText == null) return;

        OrderManager om = OrderManager.Instance;
        if (om == null) return;

        // Нет заказа — просим позвать клиента
        if (!om.HasActiveOrder)
        {
            headerText.text = "БЛОКНОТ ЗАКАЗОВ";
            if (patienceText != null) patienceText.text = "";
            orderText.text = "Нет активного заказа.\n\nНажмите кнопку\n«СЛЕДУЮЩИЙ КЛИЕНТ»";
            return;
        }

        // Шапка: имя и тип клиента
        ClientProfile cp = om.CurrentClientProfile;
        if (cp != null)
        {
            headerText.text = "Клиент: " + cp.clientName + "\n[" + Client.GetTypeName(cp.type) + "]";
        }
        else
        {
            headerText.text = "Клиент: ?";
        }

        // Список ингредиентов
        string lines = "ЗАКАЗ:\n";
        foreach (IngredientType ing in om.requiredIngredients)
        {
            int have = om.GetPieceCount(ing);
            lines += BuildLine(ing, have) + "\n";
        }

        if (orderCompleted)
        {
            lines += "\n<color=green>ЗАКАЗ СОБРАН!\nНЕСИ К ПЕЧИ!</color>";
        }

        orderText.text = lines;
    }

    // Одна строка вида: "Помидоры  ●●○"
    string BuildLine(IngredientType type, int have)
    {
        string name = OrderManager.GetDisplayName(type);
        int need = OrderManager.Instance.piecesPerIngredient;

        string dots = "";
        for (int i = 0; i < need; i++)
        {
            dots += (i < have) ? "●" : "○";
        }

        if (have >= need)
        {
            return "<color=green>" + name + "  " + dots + "</color>";
        }
        return name + "  " + dots;
    }

    // Обновление строки ожидания (каждый кадр)
    void UpdatePatience()
    {
        if (patienceText == null) return;

        Client c = (DayManager.Instance != null) ? DayManager.Instance.CurrentClient : null;
        OrderManager om = OrderManager.Instance;

        if (c == null || om == null || !om.HasActiveOrder)
        {
            patienceText.text = "";
            return;
        }

        int secondsLeft = Mathf.CeilToInt(c.PatienceLeft);
        patienceText.text = "Ожидание: " + secondsLeft + " сек";

        // Цвет: зелёный → оранжевый → красный
        float f = c.PatienceNormalized;
        if (f > 0.5f) patienceText.color = new Color(0.10f, 0.45f, 0.15f);
        else if (f > 0.25f) patienceText.color = new Color(0.85f, 0.50f, 0.05f);
        else patienceText.color = new Color(0.80f, 0.15f, 0.10f);
    }
}
