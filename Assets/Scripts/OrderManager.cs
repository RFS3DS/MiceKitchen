using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    [Header("Ингредиенты заказа (генерируются автоматически)")]
    public List<IngredientType> requiredIngredients = new List<IngredientType>();

    [Header("Сколько кусочков каждого ингредиента нужно")]
    public int piecesPerIngredient = 3; // 3 кусочка = 1 полноценный ингредиент

    // Словарь собранных кусочков по типу
    private Dictionary<IngredientType, int> currentCounts = new Dictionary<IngredientType, int>();

    // НОВОЕ: клиент, под которого создан заказ (характеристики: тип, ожидание, имя)
    public ClientProfile CurrentClientProfile { get; private set; }

    // НОВОЕ: есть ли активный заказ
    public bool HasActiveOrder
    {
        get { return requiredIngredients.Count > 0; }
    }

    // ============================================================
    // НОВОЕ: СОБЫТИЯ — на них подписан блокнот OrderNotepad
    // ============================================================
    public event System.Action OnOrderGenerated;                                // заказ создан
    public event System.Action OnOrderCleared;                                  // заказ отменён (клиент ушёл / конец дня)
    public event System.Action<IngredientType, int, int> OnPieceAdded;          // добавлен кусочек (тип, сколько уже, сколько нужно)
    public event System.Action OnOrderCompleted;                                // заказ полностью собран

    void Awake()
    {
        Instance = this;
    }

    // ВАЖНО: автоматической генерации в Start() больше НЕТ!
    // Заказ создаётся ТОЛЬКО по кнопке «СЛЕДУЮЩИЙ КЛИЕНТ»
    // (DayManager.CallNextClient вызывает метод ниже).

    // ============================================================
    // ГЕНЕРАЦИЯ ЗАКАЗА — вызывается из DayManager.CallNextClient()
    // ============================================================
    public void GenerateNewRecipe(ClientProfile client = null)
    {
        if (client != null) CurrentClientProfile = client;

        if (CurrentClientProfile == null)
        {
            Debug.LogWarning("Нет клиента! Нажмите кнопку «СЛЕДУЮЩИЙ КЛИЕНТ».");
            return;
        }

        requiredIngredients.Clear();
        ResetCounts();

        // НОВОЕ: количество видов берётся из типа клиента (гурман просит 4)
        int typesCount = CurrentClientProfile.ingredientCount;

        List<IngredientType> allTypes = new List<IngredientType> {
            IngredientType.Ananas,
            IngredientType.Anchous,
            IngredientType.Bazilik,
            IngredientType.Bekon,
            IngredientType.Brokli,
            IngredientType.Grib,
            IngredientType.Kabachok,
            IngredientType.KolbasaDoktor,
            IngredientType.KolbasaKapcha,
            IngredientType.KolbasaSred,
            IngredientType.Krevetka,
            IngredientType.Luk,
            IngredientType.LukKrasniy,
            IngredientType.Maslina,
            IngredientType.Olivka,
            IngredientType.PerecZH,
            IngredientType.PerecZ,
            IngredientType.PerecK,
            IngredientType.PerecOstr,
            IngredientType.Pomidor,
            IngredientType.Sir
        };

        string recipeText = "Клиент " + CurrentClientProfile.clientName +
                            " [" + Client.GetTypeName(CurrentClientProfile.type) + "] заказывает: ";

        for (int i = 0; i < typesCount && allTypes.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, allTypes.Count);
            IngredientType selected = allTypes[randomIndex];
            requiredIngredients.Add(selected);
            allTypes.RemoveAt(randomIndex); // чтобы не повторять ингредиент в заказе

            recipeText += "[" + GetDisplayName(selected) + " x" + piecesPerIngredient + "] ";
        }

        Debug.Log(recipeText);

        if (OnOrderGenerated != null) OnOrderGenerated();
    }

    // ============================================================
    // НОВОЕ: ОТМЕНА ЗАКАЗА (клиент ушёл, не дождавшись / конец дня)
    // ============================================================
    public void ClearOrder()
    {
        requiredIngredients.Clear();
        ResetCounts();
        CurrentClientProfile = null;

        if (OnOrderCleared != null) OnOrderCleared();
    }

    public void ResetCounts()
    {
        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            currentCounts[type] = 0;
        }
    }

    // НОВОЕ: сколько кусочков данного типа уже на пицце (для блокнота)
    public int GetPieceCount(IngredientType type)
    {
        return currentCounts.ContainsKey(type) ? currentCounts[type] : 0;
    }

    // Добавление кусочка ингредиента на пиццу
    public void AddPiece(IngredientType type)
    {
        // НОВОЕ: без активного заказа кусочки не принимаются
        if (requiredIngredients.Count == 0)
        {
            Debug.LogWarning("Нет активного заказа! Нажмите «СЛЕДУЮЩИЙ КЛИЕНТ».");
            return;
        }

        currentCounts[type]++;
        int count = currentCounts[type];

        if (!requiredIngredients.Contains(type))
        {
            Debug.LogError("Ошибка: " + GetDisplayName(type) + " не входит в заказ! Клиент будет недоволен (куска: " + count + ").");
        }
        else
        {
            Debug.Log("Добавлено: " + GetDisplayName(type) + "! На пицце: " + count + "/" + piecesPerIngredient + " шт.");

            if (count == piecesPerIngredient)
            {
                Debug.Log("Ингредиент " + GetDisplayName(type) + " полностью выложен на пиццу (" + piecesPerIngredient + "/" + piecesPerIngredient + ")!");
            }
            else if (count > piecesPerIngredient)
            {
                Debug.LogWarning("Внимание: " + GetDisplayName(type) + " уже слишком много (" + count + "/" + piecesPerIngredient + ")!");
            }
        }

        // НОВОЕ: сообщаем блокноту о прогрессе
        if (OnPieceAdded != null) OnPieceAdded(type, count, piecesPerIngredient);

        CheckRecipeCompletion();
    }

    // Проверка, собран ли заказ
    void CheckRecipeCompletion()
    {
        if (requiredIngredients.Count == 0) return;

        foreach (var req in requiredIngredients)
        {
            if (currentCounts[req] < piecesPerIngredient) return;
        }

        Debug.Log("Заказ собран из нужных ингредиентов! В печь!");

        if (OnOrderCompleted != null) OnOrderCompleted();

        StartCoroutine(TransitionToBakeWithDelay());
    }

    private IEnumerator TransitionToBakeWithDelay()
    {
        yield return new WaitForSeconds(1f);
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetState(StationState.Bake);
        }
    }

    // ============================================================
    // НОВОЕ: русские названия ингредиентов (для блокнота и логов)
    // ============================================================
    public static string GetDisplayName(IngredientType type)
    {
        switch (type)
        {
            case IngredientType.Sous: return "Соус";
            case IngredientType.Ananas: return "Ананас";
            case IngredientType.Anchous: return "Анчоусы";
            case IngredientType.Bazilik: return "Базилик";
            case IngredientType.Bekon: return "Бекон";
            case IngredientType.Brokli: return "Брокколи";
            case IngredientType.Grib: return "Грибы";
            case IngredientType.Kabachok: return "Кабачок";
            case IngredientType.KolbasaDoktor: return "Докторская колбаса";
            case IngredientType.KolbasaKapcha: return "Копчёная колбаса";
            case IngredientType.KolbasaSred: return "Сервелат";
            case IngredientType.Krevetka: return "Креветки";
            case IngredientType.Luk: return "Лук";
            case IngredientType.LukKrasniy: return "Красный лук";
            case IngredientType.Maslina: return "Маслины";
            case IngredientType.Olivka: return "Оливки";
            case IngredientType.PerecZH: return "Жёлтый перец";
            case IngredientType.PerecZ: return "Зелёный перец";
            case IngredientType.PerecK: return "Красный перец";
            case IngredientType.PerecOstr: return "Острый перец";
            case IngredientType.Pomidor: return "Помидоры";
            case IngredientType.Sir: return "Сыр";
            default: return type.ToString();
        }
    }
}
