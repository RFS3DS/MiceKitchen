using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    [Header("Текущий рецепт (нужные ингредиенты)")]
    public List<IngredientType> requiredIngredients = new List<IngredientType>();

    // Подсчет кусочков на пицце (3 кусочка = 1 засчитанный ингредиент)
    private Dictionary<IngredientType, int> currentCounts = new Dictionary<IngredientType, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateNewRecipe();
    }

    // Генерация случайного рецепта — теперь всегда ровно 3 ингредиента
    public void GenerateNewRecipe()
    {
        requiredIngredients.Clear();
        ResetCounts();

        // ИСПРАВЛЕНО: рецепт всегда состоит из 3 типов ингредиентов
        int typesCount = 3;

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

        string recipeText = "НОВЫЙ РЕЦЕПТ: ";
        for (int i = 0; i < typesCount; i++)
        {
            int randomIndex = Random.Range(0, allTypes.Count);
            IngredientType selected = allTypes[randomIndex];
            requiredIngredients.Add(selected);
            allTypes.RemoveAt(randomIndex); // Чтобы не повторялись типы в рецепте

            recipeText += $"[{selected} (нужно 3 шт)] ";
        }

        Debug.Log(recipeText);
    }

    public void ResetCounts()
    {
        // БОНУС: вместо 21 строки вручную — обнуляем по всем значениям enum
        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            currentCounts[type] = 0;
        }
    }

    // Вызывается, когда мы бросили кусочек на тесто
    public void AddPiece(IngredientType type)
    {
        // Кусочек добавляется на пиццу В ЛЮБОМ СЛУЧАЕ
        currentCounts[type]++;
        int count = currentCounts[type];

        // ИСПРАВЛЕНО: ингредиент не из рецепта — ошибка в консоль,
        // но кусочек всё равно остаётся на пицце
        if (!requiredIngredients.Contains(type))
        {
            Debug.LogError($"ОШИБКА: {type} НЕ входит в рецепт! Но кусочек всё равно добавлен на пиццу (всего: {count} шт).");
        }
        else
        {
            Debug.Log($"Добавлен {type}! На пицце сейчас: {count}/3 шт.");

            if (count == 3)
            {
                Debug.Log($"Ингредиент {type} ПОЛНОСТЬЮ добавлен на пиццу (3/3)!");
            }
            else if (count > 3)
            {
                Debug.LogWarning($"ВНИМАНИЕ: {type} добавлен больше нормы ({count}/3)!");
            }
        }

        CheckRecipeCompletion();
    }

    // Проверка, готова ли пицца
    void CheckRecipeCompletion()
    {
        foreach (var req in requiredIngredients)
        {
            if (currentCounts[req] < 3) return;
        }

        Debug.Log("ПИЦЦА ПОЛНОСТЬЮ ГОТОВА И СООТВЕТСТВУЕТ РЕЦЕПТУ! Пора в печь!");

        // Автоматически переводим камеру к печи через 1 секунду после сборки
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
}