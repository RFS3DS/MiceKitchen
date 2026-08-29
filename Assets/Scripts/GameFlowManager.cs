using System;
using UnityEngine;

public enum StationState
{
    Order,      // Приём заказа
    Prep,       // Стол готовки (тесто + ингредиенты)
    Bake,       // Духовка
    Drinks,     // Напитки
    Serve,      // Выдача
    Office      // НОВОЕ: Кабинет владельца (отсюда начинается и заканчивается день)
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Точки станций (Transforms)")]
    public Transform orderPoint;
    public Transform prepPoint;
    public Transform bakePoint;
    public Transform drinksPoint;
    public Transform servePoint;

    [Header("НОВОЕ: Точка камеры для кабинета")]
    [Tooltip("Куда ездит камера в кабинете. Если пусто — камера остаётся на месте")]
    public Transform officePoint;

    [Header("Камера и движение")]
    public Camera mainCamera;
    public float cameraSpeed = 5f;

    private StationState currentState = StationState.Office; // НОВОЕ: игра начинается в кабинете
    private Vector3 targetCameraPosition;

    public StationState CurrentState => currentState;

    // НОВОЕ: событие смены станции (на него подписан OfficeManager)
    public event Action<StationState> OnStateChanged;

    void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;

        // НОВОЕ: если точка станции не назначена — камера просто стоит на месте,
        // а не уезжает в центр сцены
        if (mainCamera != null) targetCameraPosition = mainCamera.transform.position;
    }

    void Start()
    {
        // НОВОЕ: теперь день начинается в кабинете, а не на стойке заказов
        SetState(StationState.Office, true);
    }

    void Update()
    {
        // Плавное перемещение камеры к активной станции
        Vector3 targetPos = new Vector3(targetCameraPosition.x, targetCameraPosition.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraSpeed);
    }

    /// Вызывается для перехода игрока на станцию (и перемещения камеры в нужную точку)
    public void SetState(StationState newState, bool immediate = false)
    {
        // НОВОЕ: пока день не начат, находиться можно только в кабинете
        if (newState != StationState.Office &&
            DayManager.Instance != null &&
            !DayManager.Instance.DayActive)
        {
            Debug.Log("День не начат! Сначала нажмите «НАЧАТЬ ДЕНЬ» в кабинете.");
            return;
        }

        currentState = newState;
        Transform targetPoint = GetStationPoint(newState);

        if (targetPoint != null)
        {
            targetCameraPosition = targetPoint.position;
            if (immediate)
            {
                mainCamera.transform.position = new Vector3(targetPoint.position.x, targetPoint.position.y, mainCamera.transform.position.z);
            }
        }

        // НОВОЕ: сообщаем всем (кабинету и т.д.), что станция сменилась
        if (OnStateChanged != null)
        {
            OnStateChanged(currentState);
        }
    }

    private Transform GetStationPoint(StationState state)
    {
        switch (state)
        {
            case StationState.Order: return orderPoint;
            case StationState.Prep: return prepPoint;
            case StationState.Bake: return bakePoint;
            case StationState.Drinks: return drinksPoint;
            case StationState.Serve: return servePoint;
            case StationState.Office: return officePoint; // НОВОЕ
            default: return null;
        }
    }
}
