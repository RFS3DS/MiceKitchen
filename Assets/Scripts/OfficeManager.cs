using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ============================================================
// КАБИНЕТ ВЛАДЕЛЬЦА ПИЦЦЕРИИ.
// Из кабинета:
//  • НАЧАТЬ ДЕНЬ      — запускает день и таймер
//  • ЗАВЕРШИТЬ ДЕНЬ   — заканчивает день досрочно, показывает итоги
//  • МАГАЗИН          — покупка улучшений за монеты
//
// УСТАНОВКА: добавить компонент на пустой объект в сцене.
// Если панели не назначены — весь интерфейс кабинета СОЗДАСТСЯ САМ.
// ============================================================
public class OfficeManager : MonoBehaviour
{
    public static OfficeManager Instance;

    [Header("ПАНЕЛИ (создадутся автоматически, если оставить пустыми)")]
    public GameObject officePanel;    // главный экран кабинета
    public GameObject shopPanel;      // магазин улучшений
    public GameObject resultsPanel;   // итоги дня

    [Header("Зона списка улучшений (создастся сама)")]
    public RectTransform shopContent;

    [Header("Кнопки (создадутся сами)")]
    public Button startDayButton;
    public Button openShopButton;
    public Button backFromShopButton;
    public Button resultsOkButton;
    public Button toMenuButton;

    [Header("Тексты (создадутся сами)")]
    public Text dayTitleText;
    public Text resultsText;
    public List<Text> coinsTexts = new List<Text>();

    const int MaxLevel = 5; // максимальный уровень улучшения

    // Строка магазина
    class ShopRow
    {
        public UpgradeType type;
        public Text info;
        public Button buy;
        public Text buyLabel;
    }
    readonly List<ShopRow> shopRows = new List<ShopRow>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (officePanel == null) BuildDefaultUi();

        WireButtons();
        BuildShopRows();

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnStateChanged += HandleStateChanged;
        }
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded += HandleDayEnded;
        }

        RefreshOffice();
        HandleStateChanged(GameFlowManager.Instance != null
            ? GameFlowManager.Instance.CurrentState
            : StationState.Office);
    }

    void OnDestroy()
    {
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.OnStateChanged -= HandleStateChanged;
        if (DayManager.Instance != null) DayManager.Instance.OnDayEnded -= HandleDayEnded;
    }

    // ============================================================
    //  КНОПКИ
    // ============================================================
    void WireButtons()
    {
        if (startDayButton != null) startDayButton.onClick.AddListener(OnStartDayPressed);
        if (openShopButton != null) openShopButton.onClick.AddListener(OpenShop);
        if (backFromShopButton != null) backFromShopButton.onClick.AddListener(CloseShop);
        if (resultsOkButton != null) resultsOkButton.onClick.AddListener(CloseResults);
        if (toMenuButton != null) toMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // Главная кнопка кабинета: НАЧАТЬ ДЕНЬ или ЗАВЕРШИТЬ ДЕНЬ
    void OnStartDayPressed()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogError("OfficeManager: DayManager не найден на сцене!");
            return;
        }

        if (DayManager.Instance.DayActive)
        {
            DayManager.Instance.EndDay(); // досрочно завершить
        }
        else
        {
            DayManager.Instance.BeginDay(); // начать новый день
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        RefreshShop();
        RefreshCoins();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    public void CloseResults()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        SaveSystem.Save();
        SceneManager.LoadScene(0); // сцена главного меню (индекс 0)
    }

    // ============================================================
    //  ПОКУПКА УЛУЧШЕНИЙ
    // ============================================================
    void TryBuy(ShopRow row)
    {
        int level = SaveSystem.GetUpgradeLevel(row.type);

        if (level >= MaxLevel)
        {
            Debug.Log("Уже максимальный уровень!");
            return;
        }

        int cost = UpgradeInfo.GetCost(row.type, level);
        if (SaveSystem.Data.coins < cost)
        {
            Debug.Log("Не хватает монет! Нужно: " + cost + ", есть: " + SaveSystem.Data.coins);
            return;
        }

        SaveSystem.Data.coins -= cost;
        SaveSystem.SetUpgradeLevel(row.type, level + 1);
        SaveSystem.Save();

        Debug.Log("Куплено улучшение: " + UpgradeInfo.GetName(row.type) + " — уровень " + (level + 1));

        RefreshShop();
        RefreshCoins();
    }

    // ============================================================
    //  ОБНОВЛЕНИЕ ИНТЕРФЕЙСА
    // ============================================================

    // Реагируем на смену станции: кабинет показываем только в кабинете
    void HandleStateChanged(StationState newState)
    {
        bool inOffice = newState == StationState.Office;

        if (officePanel != null) officePanel.SetActive(inOffice);
        if (!inOffice && shopPanel != null) shopPanel.SetActive(false);

        if (inOffice) RefreshOffice();
    }

    // Итоги дня (событие из DayManager)
    void HandleDayEnded(int earned, int served)
    {
        if (resultsText != null)
        {
            resultsText.text =
                "День " + (SaveSystem.Data.dayNumber - 1) + " закончен!\n\n" +
                "Обслужено клиентов: " + served + "\n" +
                "Заработано: " + earned + " монет\n\n" +
                "Всего монет: " + SaveSystem.Data.coins + "\n" +
                "Следующий день: " + SaveSystem.Data.dayNumber;
        }
        if (resultsPanel != null) resultsPanel.SetActive(true);

        RefreshOffice();
    }

    void RefreshOffice()
    {
        if (dayTitleText != null)
        {
            dayTitleText.text = "КАБИНЕТ — ДЕНЬ " + SaveSystem.Data.dayNumber;
        }

        // Текст главной кнопки зависит от состояния дня
        if (startDayButton != null)
        {
            Text t = startDayButton.GetComponentInChildren<Text>();
            if (t != null)
            {
                bool dayActive = DayManager.Instance != null && DayManager.Instance.DayActive;
                t.text = dayActive ? "ЗАВЕРШИТЬ ДЕНЬ" : "НАЧАТЬ ДЕНЬ";
            }
        }

        RefreshCoins();
    }

    void RefreshCoins()
    {
        for (int i = 0; i < coinsTexts.Count; i++)
        {
            if (coinsTexts[i] != null)
            {
                coinsTexts[i].text = "Монеты: " + SaveSystem.Data.coins;
            }
        }
    }

    void RefreshShop()
    {
        for (int i = 0; i < shopRows.Count; i++)
        {
            ShopRow row = shopRows[i];
            int level = SaveSystem.GetUpgradeLevel(row.type);

            if (row.info != null)
            {
                row.info.text = "<b>" + UpgradeInfo.GetName(row.type) + "</b>  [Ур. " + level + "/" + MaxLevel + "]\n" +
                                UpgradeInfo.GetDescription(row.type);
            }

            if (row.buy != null && row.buyLabel != null)
            {
                if (level >= MaxLevel)
                {
                    row.buyLabel.text = "МАКС.";
                    row.buy.interactable = false;
                }
                else
                {
                    int cost = UpgradeInfo.GetCost(row.type, level);
                    row.buyLabel.text = "КУПИТЬ\n" + cost + " монет";
                    row.buy.interactable = SaveSystem.Data.coins >= cost;
                }
            }
        }
    }

    // ============================================================
    //  ПОСТРОЕНИЕ СТРОК МАГАЗИНА
    // ============================================================
    void BuildShopRows()
    {
        if (shopContent == null) return;

        float rowHeight = 96f;
        float spacing = 14f;

        System.Array all = System.Enum.GetValues(typeof(UpgradeType));

        for (int i = 0; i < all.Length; i++)
        {
            UpgradeType type = (UpgradeType)all.GetValue(i);

            GameObject row = new GameObject("Row_" + type, typeof(RectTransform), typeof(Image));
            row.transform.SetParent(shopContent, false);

            RectTransform rowRt = (RectTransform)row.transform;
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, -i * (rowHeight + spacing));
            rowRt.sizeDelta = new Vector2(0f, rowHeight);

            row.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

            // Информация слева
            Text info = UiHelpers.CreateText(row.transform, "Info", "", 24, TextAnchor.MiddleLeft, Color.white);
            UiHelpers.Stretch(info.rectTransform, 16f, 8f, 240f, 8f);

            // Кнопка покупки справа
            Button buy = UiHelpers.CreateButton(row.transform, "Buy", "", Vector2.zero,
                new Vector2(210f, 70f), new Color(0.30f, 0.55f, 0.30f), 22);

            RectTransform buyRt = (RectTransform)buy.transform;
            buyRt.anchorMin = new Vector2(1f, 0.5f);
            buyRt.anchorMax = new Vector2(1f, 0.5f);
            buyRt.pivot = new Vector2(1f, 0.5f);
            buyRt.anchoredPosition = new Vector2(-14f, 0f);

            Text buyLabel = buy.GetComponentInChildren<Text>();

            ShopRow r = new ShopRow();
            r.type = type;
            r.info = info;
            r.buy = buy;
            r.buyLabel = buyLabel;

            // Внимание: копия переменной для замыкания
            ShopRow captured = r;
            buy.onClick.AddListener(delegate { TryBuy(captured); });

            shopRows.Add(r);
        }

        RefreshShop();
    }

    // ============================================================
    //  АВТОПОСТРОЙКА ВСЕГО ИНТЕРФЕЙСА КАБИНЕТА
    // ============================================================
    void BuildDefaultUi()
    {
        Canvas canvas = UiHelpers.EnsureCanvas("OfficeCanvas", 60);
        Transform root = canvas.transform;

        // ---------- 1. ГЛАВНЫЙ ЭКРАН КАБИНЕТА ----------
        officePanel = CreateFullScreenPanel(root, "OfficePanel", new Color(0.13f, 0.10f, 0.08f, 0.98f));

        dayTitleText = UiHelpers.CreateText(officePanel.transform, "DayTitle", "КАБИНЕТ", 44, TextAnchor.MiddleCenter, Color.white);
        dayTitleText.fontStyle = FontStyle.Bold;
        dayTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        dayTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        dayTitleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        dayTitleText.rectTransform.anchoredPosition = new Vector2(0f, -40f);
        dayTitleText.rectTransform.sizeDelta = new Vector2(-80f, 80f);

        Text coins1 = UiHelpers.CreateText(officePanel.transform, "Coins", "", 34, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f));
        coins1.fontStyle = FontStyle.Bold;
        coins1.rectTransform.anchorMin = new Vector2(0f, 1f);
        coins1.rectTransform.anchorMax = new Vector2(1f, 1f);
        coins1.rectTransform.pivot = new Vector2(0.5f, 1f);
        coins1.rectTransform.anchoredPosition = new Vector2(0f, -130f);
        coins1.rectTransform.sizeDelta = new Vector2(-200f, 50f);
        coinsTexts.Add(coins1);

        startDayButton = UiHelpers.CreateButton(officePanel.transform, "StartDayButton", "НАЧАТЬ ДЕНЬ",
            new Vector2(0f, 40f), new Vector2(560f, 110f), new Color(0.25f, 0.60f, 0.30f), 38);

        openShopButton = UiHelpers.CreateButton(officePanel.transform, "OpenShopButton", "МАГАЗИН УЛУЧШЕНИЙ",
            new Vector2(0f, -110f), new Vector2(560f, 100f), new Color(0.35f, 0.50f, 0.68f), 34);

        toMenuButton = UiHelpers.CreateButton(officePanel.transform, "ToMenuButton", "ГЛАВНОЕ МЕНЮ",
            new Vector2(0f, -300f), new Vector2(430f, 80f), new Color(0.45f, 0.45f, 0.45f), 28);

        // ---------- 2. МАГАЗИН УЛУЧШЕНИЙ ----------
        shopPanel = CreateFullScreenPanel(root, "ShopPanel", new Color(0.10f, 0.10f, 0.13f, 0.98f));

        Text shopTitle = UiHelpers.CreateText(shopPanel.transform, "ShopTitle", "МАГАЗИН УЛУЧШЕНИЙ", 40, TextAnchor.MiddleCenter, Color.white);
        shopTitle.fontStyle = FontStyle.Bold;
        shopTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
        shopTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
        shopTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
        shopTitle.rectTransform.anchoredPosition = new Vector2(0f, -30f);
        shopTitle.rectTransform.sizeDelta = new Vector2(-80f, 70f);

        Text coins2 = UiHelpers.CreateText(shopPanel.transform, "Coins", "", 30, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f));
        coins2.fontStyle = FontStyle.Bold;
        coins2.rectTransform.anchorMin = new Vector2(0f, 1f);
        coins2.rectTransform.anchorMax = new Vector2(1f, 1f);
        coins2.rectTransform.pivot = new Vector2(0.5f, 1f);
        coins2.rectTransform.anchoredPosition = new Vector2(0f, -104f);
        coins2.rectTransform.sizeDelta = new Vector2(-200f, 46f);
        coinsTexts.Add(coins2);

        // Область со списком улучшений
        GameObject contentGo = new GameObject("ShopContent", typeof(RectTransform));
        contentGo.transform.SetParent(shopPanel.transform, false);
        shopContent = (RectTransform)contentGo.transform;
        UiHelpers.Stretch(shopContent, 60f, 160f, 60f, 140f);

        backFromShopButton = UiHelpers.CreateButton(shopPanel.transform, "BackButton", "НАЗАД",
            new Vector2(0f, -70f), new Vector2(430f, 80f), new Color(0.45f, 0.45f, 0.45f), 28);

        // ---------- 3. ИТОГИ ДНЯ ----------
        GameObject resultsGo = new GameObject("ResultsPanel", typeof(RectTransform), typeof(Image));
        resultsGo.transform.SetParent(root, false);
        RectTransform resRt = (RectTransform)resultsGo.transform;
        resRt.anchorMin = new Vector2(0.5f, 0.5f);
        resRt.anchorMax = new Vector2(0.5f, 0.5f);
        resRt.sizeDelta = new Vector2(640f, 700f);
        resultsGo.GetComponent<Image>().color = new Color(0.15f, 0.13f, 0.10f, 1f);
        resultsPanel = resultsGo;

        resultsText = UiHelpers.CreateText(resultsGo.transform, "ResultsText", "", 32, TextAnchor.MiddleCenter, Color.white);
        UiHelpers.Stretch(resultsText.rectTransform, 30f, 120f, 30f, 120f);

        resultsOkButton = UiHelpers.CreateButton(resultsGo.transform, "OkButton", "ПРОДОЛЖИТЬ",
            new Vector2(0f, -290f), new Vector2(360f, 84f), new Color(0.25f, 0.60f, 0.30f), 30);

        resultsPanel.SetActive(false);
    }

    GameObject CreateFullScreenPanel(Transform parent, string name, Color bg)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)panel.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        panel.GetComponent<Image>().color = bg;
        return panel;
    }
}

// ============================================================
// ОПИСАНИЕ УЛУЧШЕНИЙ
// ============================================================
public static class UpgradeInfo
{
    public static string GetName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.OvenSpeed: return "Скоростная печь";
            case UpgradeType.Patience:  return "Комната ожидания";
            case UpgradeType.DayLength: return "Реклама";
            case UpgradeType.Tips:      return "Хорошее обслуживание";
            default:                    return type.ToString();
        }
    }

    public static string GetDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.OvenSpeed: return "Пицца печётся на 12% быстрее за уровень";
            case UpgradeType.Patience:  return "Клиенты ждут на 10 секунд дольше за уровень";
            case UpgradeType.DayLength: return "Рабочий день дольше на 30 секунд за уровень";
            case UpgradeType.Tips:      return "Чаевые больше на 20% за уровень";
            default:                    return "";
        }
    }

    // Цена уровня: базовая цена × (уровень + 1)
    public static int GetCost(UpgradeType type, int currentLevel)
    {
        return GetBaseCost(type) * (currentLevel + 1);
    }

    static int GetBaseCost(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.OvenSpeed: return 150;
            case UpgradeType.Patience:  return 100;
            case UpgradeType.DayLength: return 200;
            case UpgradeType.Tips:      return 120;
            default:                    return 100;
        }
    }
}
