using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientTray : MonoBehaviour
{
    [Header("Префаб ингредиента (кусочка)")]
    public GameObject toppingPrefab;

    private GameObject currentTopping;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // Когда зажали палец/мышку на лотке
    void OnMouseDown()
    {
        if (toppingPrefab == null) return;

        // ПРОВЕРКА: Брать ингредиенты можно только на Prep-станции
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.CurrentState != StationState.Prep)
        {
            return;
        }

        // Создаем маленький ингредиент в точке нажатия
        Vector3 spawnPos = GetMouseWorldPosition();
        currentTopping = Instantiate(toppingPrefab, spawnPos, Quaternion.identity);
    }

    // Пока тащим палец по экрану
    void OnMouseDrag()
    {
        if (currentTopping != null)
        {
            currentTopping.transform.position = GetMouseWorldPosition();
        }
    }

    // Когда отпустили палец
    void OnMouseUp()
    {
        if (currentTopping == null) return;

        // Проверяем, есть ли под пальцем объект с тегом "Pizza"
        Collider2D hitCollider = Physics2D.OverlapPoint(currentTopping.transform.position);

        if (hitCollider != null && hitCollider.CompareTag("Pizza"))
        {
            // Прикрепляем ингредиент к пицце (чтобы он был её "детем")
            currentTopping.transform.SetParent(hitCollider.transform);
            Topping toppingComponent = currentTopping.GetComponent<Topping>();
            if (toppingComponent != null && OrderManager.Instance != null)
            {
                OrderManager.Instance.AddPiece(toppingComponent.type);
            }
        }
        else
        {
            // Если промахнулись мимо пиццы — удаляем кусочек
            Destroy(currentTopping);
        }

        currentTopping = null;
    }

    // Вспомогательный метод для получения 2D-координат пальца/мыши
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = -0.1f; // Чуть ближе к камере, чтобы точно было видно
        return worldPos;
    }
}
