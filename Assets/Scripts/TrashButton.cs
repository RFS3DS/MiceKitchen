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

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.GenerateNewRecipe();
            // Возвращаем игрока к приему заказа
            GameFlowManager.Instance.SetState(StationState.Order);
        }
    }
}
