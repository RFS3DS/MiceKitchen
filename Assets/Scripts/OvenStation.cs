using System.Collections;
using UnityEngine;

public class OvenStation : MonoBehaviour
{
    public float bakeTime = 3f;
    private bool isBaking = false;
    private bool isBaked = false;

    void OnMouseDown()
    {
        // Работает только на станции Bake
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.CurrentState != StationState.Bake) return;
        if (isBaking || isBaked) return;

        GameObject pizza = GameObject.FindGameObjectWithTag("Pizza");
        if (pizza != null)
        {
            // НОВОЕ: с учётом улучшения «Скоростная печь» из кабинета
            float actualBakeTime = bakeTime;
            if (DayManager.Instance != null)
            {
                actualBakeTime = DayManager.Instance.GetBakeTime(bakeTime);
            }

            StartCoroutine(BakeRoutine(pizza, actualBakeTime));
        }
        else
        {
            Debug.Log("Нет пиццы для запекания!");
        }
    }

    private IEnumerator BakeRoutine(GameObject pizza, float time)
    {
        isBaking = true;
        Debug.Log("Пицца выпекается... (" + time.ToString("0.0") + " сек)");

        // Визуальный эффект: пицца постепенно темнеет (эффект корочки)
        SpriteRenderer pizzaSR = pizza.GetComponent<SpriteRenderer>();
        Color originalColor = pizzaSR != null ? pizzaSR.color : Color.white;
        Color bakedColor = new Color(0.7f, 0.5f, 0.3f); // Коричневатый оттенок

        float timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            if (pizzaSR != null)
            {
                pizzaSR.color = Color.Lerp(originalColor, bakedColor, timer / time);
            }
            yield return null;
        }

        isBaking = false;
        isBaked = true;
        Debug.Log("Пицца готово! Пора налить напитки или выдать заказ.");

        // Автоматически переходим к следующему шагу
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetState(StationState.Drinks);
        }
    }

    // Сброс состояния для новой пиццы
    public void ResetOven()
    {
        isBaked = false;
        isBaking = false;
    }
}
