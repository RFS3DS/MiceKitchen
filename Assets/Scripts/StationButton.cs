using UnityEngine;
using UnityEngine.UI;

public class StationButton : MonoBehaviour
{
    [Header("Какая станция откроется при нажатии")]
    public StationState targetStation;

    private void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(GoToTargetStation);
        }
    }

    private void OnMouseDown()
    {
        GoToTargetStation();
    }

    public void GoToTargetStation()
    {
        // Проверяем, существует ли менеджер
        if (GameFlowManager.Instance != null)
        {
            Debug.Log($"Нажали на кнопку! Переходим к: {targetStation}");
            GameFlowManager.Instance.SetState(targetStation);
        }
        else
        {
            Debug.LogError("ОШИБКА: GameFlowManager не найден на сцене! Создайте пустой объект и добавьте на него скрипт GameFlowManager.");
        }
    }
}