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
        if (GameFlowManager.Instance.CurrentState != StationState.Bake) return;
        if (isBaking || isBaked) return;

        GameObject pizza = GameObject.FindGameObjectWithTag("Pizza");
        if (pizza != null)
        {
            StartCoroutine(BakeRoutine(pizza));
        }
        else
        {
            Debug.Log("Нет пиццы для запекания!");
        }
    }

    private IEnumerator BakeRoutine(GameObject pizza)
    {
        isBaking = true;
        Debug.Log("Пицца выпекается...");

        // Визуальный эффект: пицца постепенно темнеет (эффект корочки)
        SpriteRenderer pizzaSR = pizza.GetComponent<SpriteRenderer>();
        Color originalColor = pizzaSR.color;
        Color bakedColor = new Color(0.7f, 0.5f, 0.3f); // Коричневатый оттенок

        float timer = 0f;
        while (timer < bakeTime)
        {
            timer += Time.deltaTime;
            if (pizzaSR != null)
            {
                pizzaSR.color = Color.Lerp(originalColor, bakedColor, timer / bakeTime);
            }
            yield return null;
        }

        isBaking = false;
        isBaked = true;
        Debug.Log("Пицца готово! Пора налить напитки или выдать заказ.");

        // Автоматически переходим к следующему шагу
        GameFlowManager.Instance.SetState(StationState.Drinks);
    }

    // Сброс состояния для новой пиццы
    public void ResetOven()
    {
        isBaked = false;
        isBaking = false;
    }
}