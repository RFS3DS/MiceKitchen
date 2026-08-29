using UnityEngine;

public class ServingStation : MonoBehaviour
{
    [SerializeField] private OvenStation oven; // Ссылка на печь, чтобы сбросить её состояние

    void OnMouseDown()
    {
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.CurrentState != StationState.Serve) return;

        GameObject pizza = GameObject.FindGameObjectWithTag("Pizza");

        if (pizza != null)
        {
            Debug.Log("ЗАКАЗ ВЫДАН КЛИЕНТУ! Идеально!");

            // НОВОЕ: начисляем монеты за заказ (цена + чаевые за скорость)
            if (DayManager.Instance != null)
            {
                DayManager.Instance.RegisterServedOrder();
            }

            // Уничтожаем пиццу
            Destroy(pizza);

            // Сбрасываем печь
            if (oven != null) oven.ResetOven();

            // ИЗМЕНЕНО: новый заказ здесь БОЛЬШЕ НЕ генерируется!
            // Нажмите кнопку «СЛЕДУЮЩИЙ КЛИЕНТ», чтобы пришёл новый клиент
            // с новым заказом.

            // Возвращаемся к приёму заказов
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetState(StationState.Order);
            }
        }
        else
        {
            Debug.Log("Вам нечего отдавать!");
        }
    }
}
