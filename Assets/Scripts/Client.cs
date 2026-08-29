using System;
using UnityEngine;

// ============================================================
// КЛИЕНТ с характеристиками: ТИП, ОЖИДАНИЕ, ИМЯ
// ============================================================

// ===== ТИП КЛИЕНТА =====
public enum ClientType
{
    Obychniy,   // Обычный — стандартные заказ и оплата
    Speshashiy, // Спешащий — мало терпения, но щедрые чаевые
    Vip,        // VIP — платит вдвое больше
    Gurman      // Гурман — просит 4 вида ингредиентов вместо 3
}

// ===== ХАРАКТЕРИСТИКИ КЛИЕНТА (чистые данные, можно сохранять) =====
[Serializable]
public class ClientProfile
{
    public string clientName;      // ИМЯ
    public ClientType type;        // ТИП
    public float patienceSeconds;  // ОЖИДАНИЕ — сколько секунд клиент готов ждать

    public int ingredientCount;    // сколько ВИДОВ ингредиентов он заказывает (гурман — 4)
    public float rewardMultiplier; // множитель оплаты заказа
    public float tipMultiplier;    // множитель чаевых
}

// ===== ГЕНЕРАТОР СЛУЧАЙНЫХ КЛИЕНТОВ =====
public static class ClientFactory
{
    static readonly string[] firstNames =
    {
        "Аня", "Борис", "Вера", "Галя", "Дима", "Елена", "Женя", "Зоя",
        "Игорь", "Катя", "Лёша", "Маша", "Никита", "Ольга", "Паша", "Рита",
        "Соня", "Тимур", "Ульяна", "Федя", "Юра", "Яна"
    };

    public static ClientProfile Generate(int dayNumber)
    {
        ClientType type = RollType();

        // Бонус к терпению из улучшения "Комната ожидания"
        float patienceBonus = (DayManager.Instance != null) ? DayManager.Instance.GetPatienceBonus() : 0f;

        // С каждым днём клиенты становятся чуть нетерпеливнее
        float dayHurry = Mathf.Min(25f, (dayNumber - 1) * 1.5f);

        ClientProfile p = new ClientProfile();
        p.clientName = firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
        p.type = type;

        switch (type)
        {
            case ClientType.Speshashiy:
                p.patienceSeconds = 45f;
                p.ingredientCount = 3;
                p.rewardMultiplier = 1.2f;
                p.tipMultiplier = 1.5f;
                break;
            case ClientType.Vip:
                p.patienceSeconds = 90f;
                p.ingredientCount = 3;
                p.rewardMultiplier = 2f;
                p.tipMultiplier = 1.5f;
                break;
            case ClientType.Gurman:
                p.patienceSeconds = 85f;
                p.ingredientCount = 4;
                p.rewardMultiplier = 1.5f;
                p.tipMultiplier = 1.2f;
                break;
            default: // Obychniy
                p.patienceSeconds = 70f;
                p.ingredientCount = 3;
                p.rewardMultiplier = 1f;
                p.tipMultiplier = 1f;
                break;
        }

        p.patienceSeconds = Mathf.Max(25f, p.patienceSeconds + patienceBonus - dayHurry);
        return p;
    }

    static ClientType RollType()
    {
        float r = UnityEngine.Random.value;
        if (r < 0.55f) return ClientType.Obychniy;
        if (r < 0.75f) return ClientType.Speshashiy;
        if (r < 0.90f) return ClientType.Vip;
        return ClientType.Gurman;
    }
}

// ===== КЛИЕНТ В СЦЕНЕ (объект у стойки заказов) =====
// Может работать БЕЗ префаба: DayManager создаст невидимый объект с этим скриптом.
// Если хотите видеть клиента — сделайте префаб со SpriteRenderer + этот скрипт.
public class Client : MonoBehaviour
{
    public ClientProfile Profile { get; private set; }   // характеристики
    public float PatienceLeft { get; private set; }      // сколько осталось ждать

    // 1 = только пришёл, 0 = терпение кончилось
    public float PatienceNormalized
    {
        get
        {
            if (Profile == null || Profile.patienceSeconds <= 0f) return 0f;
            return Mathf.Clamp01(PatienceLeft / Profile.patienceSeconds);
        }
    }

    // Срабатывает, когда клиент устал ждать и уходит
    public event Action<Client> OnPatienceExpired;

    private bool initialized;
    private bool expired;

    public void Init(ClientProfile profile)
    {
        Profile = profile;
        PatienceLeft = profile.patienceSeconds;
        initialized = true;
        ApplyLookByType();
    }

    void Update()
    {
        if (!initialized || expired) return;

        // Время ожидания идёт только пока день активен
        if (DayManager.Instance == null || !DayManager.Instance.DayActive) return;

        PatienceLeft -= Time.deltaTime;
        if (PatienceLeft <= 0f)
        {
            PatienceLeft = 0f;
            expired = true;
            if (OnPatienceExpired != null) OnPatienceExpired(this);
        }
    }

    // Подкрашиваем персонажа по типу (если есть SpriteRenderer)
    void ApplyLookByType()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (Profile.type)
        {
            case ClientType.Speshashiy: sr.color = new Color(1f, 0.55f, 0.45f); break;
            case ClientType.Vip:        sr.color = new Color(1f, 0.85f, 0.30f); break;
            case ClientType.Gurman:     sr.color = new Color(0.55f, 1f, 0.55f); break;
            default:                    sr.color = Color.white; break;
        }
    }

    // Русское название типа (для блокнота и логов)
    public static string GetTypeName(ClientType type)
    {
        switch (type)
        {
            case ClientType.Obychniy:   return "Обычный";
            case ClientType.Speshashiy: return "Спешащий";
            case ClientType.Vip:        return "VIP";
            case ClientType.Gurman:     return "Гурман";
            default:                    return type.ToString();
        }
    }
}
