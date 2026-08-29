using System.Collections;
using UnityEngine;

public class SauceBottle : MonoBehaviour
{
    [Header("Префаб соуса (одно пятно)")]
    public GameObject saucePrefab;

    [Header("Тег раскатанного теста / пиццы")]
    public string pizzaTag = "Pizza";

    [Header("Если тег не используешь — перетащи сюда объект теста со сцены")]
    public Transform pizzaTransform;

    [Header("Локальное смещение соуса на пицце")]
    public Vector3 sauceLocalOffset = Vector3.zero;

    [Header("Сортировка (чтобы соус был ПОВЕРХ теста)")]
    [Tooltip("На сколько больше Order in Layer, чем у теста")]
    public int orderAboveDough = 1;

    [Header("Анимация намазывания")]
    public float spreadDuration = 0.35f;
    public Vector3 startScale = new Vector3(0.15f, 0.15f, 1f);
    public Vector3 endScale = new Vector3(1f, 1f, 1f);

    // Уникальное имя объекта соуса на пицце для проверки
    private const string SAUCE_OBJECT_NAME = "PizzaSauce_Applied";

    void OnMouseDown()
    {
        if (saucePrefab == null) return;

        // Проверка станции готовки
        if (GameFlowManager.Instance != null &&
            GameFlowManager.Instance.CurrentState != StationState.Prep)
        {
            return;
        }

        ApplySauce();
    }

    public void ApplySauce()
    {
        Transform pizza = FindPizza();
        if (pizza == null)
        {
            Debug.LogWarning("Пицца/тесто не найдены! Поставь тег 'Pizza' на раскатанное тесто.");
            return;
        }

        // =========================================================
        // ПРОВЕРКА: Если соус на этой пицце УЖЕ ЕСТЬ — отменяем
        // =========================================================
        if (pizza.Find(SAUCE_OBJECT_NAME) != null)
        {
            Debug.Log(" Соус уже добавлен на эту пиццу! Повторно добавить нельзя.");
            return;
        }

        // Создаём соус как дочерний объект пиццы
        GameObject currentSauce = Instantiate(saucePrefab, pizza);
        currentSauce.name = SAUCE_OBJECT_NAME; // Даём фиксированное имя для проверки

        currentSauce.transform.localPosition = sauceLocalOffset;
        currentSauce.transform.localRotation = Quaternion.identity;
        currentSauce.transform.localScale = startScale;

        // Поднимаем соус поверх слоя теста
        SetSauceAboveDough(currentSauce, pizza.gameObject);

        // Запускаем анимацию увеличения
        StopAllCoroutines();
        StartCoroutine(SpreadSauce(currentSauce.transform));

        Debug.Log(" Соус успешно намазан на пиццу!");
    }

    private Transform FindPizza()
    {
        if (pizzaTransform != null)
            return pizzaTransform;

        GameObject byTag = GameObject.FindWithTag(pizzaTag);
        if (byTag != null)
            return byTag.transform;

        // Резервный поиск теста на столе
        DoughSpawner spawner = FindObjectOfType<DoughSpawner>();
        if (spawner != null)
        {
            var colliders = FindObjectsOfType<CircleCollider2D>();
            foreach (var col in colliders)
            {
                if (col.GetComponent<SpriteRenderer>() != null &&
                    col.gameObject != spawner.gameObject &&
                    col.transform.localScale.x > 0.5f)
                {
                    return col.transform;
                }
            }
        }

        return null;
    }

    private void SetSauceAboveDough(GameObject sauce, GameObject dough)
    {
        SpriteRenderer sauceSr = sauce.GetComponent<SpriteRenderer>();
        if (sauceSr == null) sauceSr = sauce.GetComponentInChildren<SpriteRenderer>();

        SpriteRenderer doughSr = dough.GetComponent<SpriteRenderer>();
        if (doughSr == null) doughSr = dough.GetComponentInChildren<SpriteRenderer>();

        if (sauceSr == null) return;

        if (doughSr != null)
        {
            sauceSr.sortingLayerID = doughSr.sortingLayerID;
            sauceSr.sortingOrder = doughSr.sortingOrder + orderAboveDough;
        }
        else
        {
            sauceSr.sortingOrder = 10;
        }

        Vector3 lp = sauce.transform.localPosition;
        lp.z = -0.01f;
        sauce.transform.localPosition = lp;
    }

    private IEnumerator SpreadSauce(Transform sauce)
    {
        float t = 0f;
        while (t < 1f && sauce != null)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, spreadDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            sauce.localScale = Vector3.Lerp(startScale, endScale, smooth);
            yield return null;
        }

        if (sauce != null)
            sauce.localScale = endScale;
    }
}