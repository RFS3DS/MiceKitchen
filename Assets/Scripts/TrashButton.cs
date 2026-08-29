using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashButton : MonoBehaviour
{
    void OnMouseDown()
    {
        // Выбросить пиццу можно только во время готовки
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.CurrentState != StationState.Prep)
        {
            return;
        }

        GameObject pizza = GameObject.FindGameObjectWithTag("Pizza");
        if (pizza != null)
        {
            Destroy(pizza);
            Debug.Log("Пицца выброшена!");
        }

        // ИЗМЕНЕНО: заказ здесь БОЛЬШЕ НЕ перегенерируется!
        // Если клиент ещё ждёт — соберите ЕГО заказ заново
        // (посмотрите его в блокноте «ЗАКАЗ»).
        // Если клиент ушёл — нажмите «СЛЕДУЮЩИЙ КЛИЕНТ».

        if (GameFlowManager.Instance != null && OrderManager.Instance != null)
        {
            if (OrderManager.Instance.HasActiveOrder)
            {
                Debug.Log("Клиент всё ещё ждёт — соберите тот же заказ заново!");
            }
            // Возвращаем игрока к приёму заказа
            GameFlowManager.Instance.SetState(StationState.Order);
        }
    }
}
