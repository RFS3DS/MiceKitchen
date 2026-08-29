using System;
using UnityEngine;

// ============================================================
// МЕНЕДЖЕР ДНЯ — сердце нового цикла игры:
//  • Начало и конец дня (из кабинета)
//  • Таймер дня (DayTimer его показывает)
//  • Вызов клиентов (кнопка «СЛЕДУЮЩИЙ КЛИЕНТ»)
//  • Начисление монет за поданные заказы
//  • Эффекты улучшений из магазина кабинета
// ============================================================
public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Клиент (оба поля необязательны)")]
    [Tooltip("Префаб клиента со скриптом Client + SpriteRenderer. Если пусто — клиент будет невидимым")]
    public GameObject clientPrefab;
    [Tooltip("Точка, где появляется клиент (у стойки заказов)")]
    public Transform clientPoint;

    [Header("Настройки дня")]
    [Tooltip("Базовая длительность дня в секундах (+30 сек за уровень улучшения 'Реклама')")]
    public float baseDayLength = 120f;

    [Header("Экономика")]
    [Tooltip("Базовая цена пиццы в монетах")]
    public int basePizzaPrice = 50;
    [Tooltip("Максимальные чаевые (если отдал заказ мгновенно)")]
    public float maxTip = 50f;

    // ===== СОСТОЯНИЕ ДНЯ =====
    public bool DayActive { get; private set; }          // идёт ли день
    public float Elapsed { get; private set; }           // сколько секунд прошло с начала дня
    public Client CurrentClient { get; private set; }    // текущий клиент
    public int ServedToday { get; private set; }         // обслужено за день
    public int EarnedToday { get; private set; }         // заработано за день

    public float DayLength
    {
        get { return baseDayLength + 30f * SaveSystem.GetUpgradeLevel(UpgradeType.DayLength); }
    }
    public float TimeLeft
    {
        get { return Mathf.Max(0f, DayLength - Elapsed); }
    }

    // ===== СОБЫТИЯ =====
    public event Action OnDayStarted;                 // день начался
    public event Action<int, int> OnDayEnded;         // день закончился (заработано, обслужено)
    public event Action<Client> OnClientArrived;      // пришёл новый клиент

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!DayActive) return;

        // Таймер дня
        Elapsed += Time.deltaTime;
        if (Elapsed >= DayLength)
        {
            EndDay();
        }
    }

    // ============================================================
    //  НАЧАЛО ДНЯ — вызывается кнопкой в кабинете
    // ============================================================
    public void BeginDay()
    {
        if (DayActive) return;

        DayActive = true;
        Elapsed = 0f;
        ServedToday = 0;
        EarnedToday = 0;

        Debug.Log($"=== ДЕНЬ {SaveSystem.Data.dayNumber} НАЧАЛСЯ! Длительность: {DayLength:0} сек ===");

        // Переходим к стойке заказов
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetState(StationState.Order);
        }

        if (OnDayStarted != null) OnDayStarted();

        // Первый клиент приходит сам
        CallNextClient();
    }

    // ============================================================
    //  КОНЕЦ ДНЯ — по таймеру или кнопкой «Завершить день» в кабинете
    // ============================================================
    public void EndDay()
    {
        if (!DayActive) return;
        DayActive = false;

        // Клиент уходит
        RemoveCurrentClient();

        // Сбрасываем заказ и недоделанную пиццу
        if (OrderManager.Instance != null) OrderManager.Instance.ClearOrder();
        GameObject pizza = GameObject.FindGameObjectWithTag("Pizza");
        if (pizza != null) Destroy(pizza);
        OvenStation oven = FindObjectOfType<OvenStation>();
        if (oven != null) oven.ResetOven();

        // Статистика и сохранение
        SaveSystem.Data.totalServed += ServedToday;
        SaveSystem.Data.totalEarned += EarnedToday;
        SaveSystem.Data.dayNumber++; // следующий день
        SaveSystem.Save();

        Debug.Log($"=== ДЕНЬ ЗАКОНЧЕН! Заработано: {EarnedToday} монет, обслужено клиентов: {ServedToday} ===");

        // Возвращаемся в кабинет
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetState(StationState.Office);
        }

        if (OnDayEnded != null) OnDayEnded(EarnedToday, ServedToday);
    }

    // ============================================================
    //  СЛЕДУЮЩИЙ КЛИЕНТ — вызывается кнопкой NextClientButton.
    //  Именно ЗДЕСЬ (а не в Start) теперь генерируется заказ!
    // ============================================================
    public void CallNextClient()
    {
        if (!DayActive)
        {
            Debug.LogWarning("День не начат! Нажмите «НАЧАТЬ ДЕНЬ» в кабинете.");
            return;
        }

        // Старый клиент уходит (даже если не успели обслужить)
        RemoveCurrentClient();

        // Создаём нового клиента с характеристиками: тип, ожидание, имя
        ClientProfile profile = ClientFactory.Generate(SaveSystem.Data.dayNumber);
        CurrentClient = SpawnClient(profile);
        CurrentClient.OnPatienceExpired += HandleClientAngryLeave;

        Debug.Log($"Пришёл клиент: {profile.clientName} [{Client.GetTypeName(profile.type)}] — готов ждать {profile.patienceSeconds:0} сек");

        // Генерируем заказ под этого клиента
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.GenerateNewRecipe(profile);
        }

        // Камера — к стойке заказов
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetState(StationState.Order);
        }

        if (OnClientArrived != null) OnClientArrived(CurrentClient);
    }

    // ============================================================
    //  ЗАКАЗ ПОДАН — вызывается из ServingStation
    // ============================================================
    public void RegisterServedOrder()
    {
        int reward = ComputeReward();
        SaveSystem.Data.coins += reward;
        ServedToday++;
        EarnedToday += reward;

        Debug.Log($"Заказ подан! +{reward} монет (всего: {SaveSystem.Data.coins})");

        // Довольный клиент уходит
        RemoveCurrentClient();
    }

    // Награда = цена пиццы × множитель типа + чаевые × доля оставшегося терпения
    int ComputeReward()
    {
        if (CurrentClient == null || CurrentClient.Profile == null)
        {
            return basePizzaPrice; // клиента уже нет — платят по базовой цене
        }

        ClientProfile p = CurrentClient.Profile;
        float price = basePizzaPrice * p.rewardMultiplier;
        float tip = maxTip * CurrentClient.PatienceNormalized * p.tipMultiplier * GetTipMultiplier();
        return Mathf.RoundToInt(price + tip);
    }

    // ============================================================
    //  УЛУЧШЕНИЯ (уровни берутся из сохранения)
    // ============================================================

    // Скоростная печь: -12% времени выпечки за уровень (не быстрее 40% от базового)
    public float GetBakeTime(float baseTime)
    {
        int level = SaveSystem.GetUpgradeLevel(UpgradeType.OvenSpeed);
        return baseTime * Mathf.Max(0.4f, 1f - 0.12f * level);
    }

    // Комната ожидания: +10 сек к терпению клиентов за уровень
    public float GetPatienceBonus()
    {
        return 10f * SaveSystem.GetUpgradeLevel(UpgradeType.Patience);
    }

    // Хорошее обслуживание: +20% к чаевым за уровень
    public float GetTipMultiplier()
    {
        return 1f + 0.2f * SaveSystem.GetUpgradeLevel(UpgradeType.Tips);
    }

    // ============================================================
    //  ВНУТРЕННЕЕ
    // ============================================================

    Client SpawnClient(ClientProfile profile)
    {
        Client client = null;

        if (clientPrefab != null)
        {
            GameObject go = Instantiate(
                clientPrefab,
                clientPoint != null ? clientPoint.position : Vector3.zero,
                Quaternion.identity);

            client = go.GetComponent<Client>();
            if (client == null) client = go.AddComponent<Client>();
        }
        else
        {
            // Без префаба — невидимый клиент (просто данные и таймер терпения)
            GameObject go = new GameObject("Client_" + profile.clientName);
            if (clientPoint != null) go.transform.position = clientPoint.position;
            client = go.AddComponent<Client>();
        }

        client.Init(profile);
        return client;
    }

    void RemoveCurrentClient()
    {
        if (CurrentClient == null) return;
        Client c = CurrentClient;
        CurrentClient = null;
        c.OnPatienceExpired -= HandleClientAngryLeave;
        Destroy(c.gameObject);
    }

    // Клиент устал ждать и ушёл
    void HandleClientAngryLeave(Client client)
    {
        if (client == null || client.Profile == null) return;

        Debug.Log($"{client.Profile.clientName} [{Client.GetTypeName(client.Profile.type)}] устал ждать и ушёл!");

        if (CurrentClient == client) CurrentClient = null;
        if (client != null) Destroy(client.gameObject);

        // Заказ больше неактуален — блокнот попросит следующего клиента
        if (OrderManager.Instance != null) OrderManager.Instance.ClearOrder();

        SaveSystem.Data.angryClients++;
    }

    // Сохраняемся при выходе из игры и сворачивании (важно для телефона)
    void OnApplicationQuit()
    {
        SaveSystem.Save();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();
    }
}
