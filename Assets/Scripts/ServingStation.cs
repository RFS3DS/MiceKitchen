using UnityEngine;

public class ServingStation : MonoBehaviour
{
    [SerializeField] private OvenStation oven; // Ссылка на печь, чтобы сбросить её состояние

    void OnMouseDown()
    {
        if (GameFlowManager.Instance.CurrentState != StationState.Serve) return;

        GameObject pizza = GameObject.FindGameObjectWithTag("Pizza");

        if (pizza != null)
        {
            // Здесь можно добавить подсчет очков
            Debug.Log("ЗАКАЗ ВЫДАН КЛИЕНТУ! Идеально!");

            // Уничтожаем пиццу
            Destroy(pizza);

            // Сбрасываем печь
            if (oven != null) oven.ResetOven();

            // Генерируем новый рецепт
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.GenerateNewRecipe();
            }

            // Возвращаемся в начало — к приему заказов
            GameFlowManager.Instance.SetState(StationState.Order);
        }
        else
        {
            Debug.Log("Вам нечего отдавать!");
        }
    }
}