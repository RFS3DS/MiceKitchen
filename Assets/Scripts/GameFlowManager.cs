using UnityEngine;

public enum StationState
{
    Order,      // Стойка заказов
    Prep,       // Стол готовки (тесто + ингредиенты)
    Bake,       // Печь
    Drinks,     // Напитки
    Serve       // Выдача
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

    [Header("Настройки камеры")]
    public Camera mainCamera;
    public float cameraSpeed = 5f;

    private StationState currentState = StationState.Order;
    private Vector3 targetCameraPosition;

    public StationState CurrentState => currentState;

    void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Start()
    {
        // При старте мгновенно ставим камеру на первую станцию
        SetState(StationState.Order, true);
    }

    void Update()
    {
        // Плавно перемещаем камеру к выбранной станции
        Vector3 targetPos = new Vector3(targetCameraPosition.x, targetCameraPosition.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraSpeed);
    }

    /// Публичный метод для мгновенного или плавного перехода к конкретной станции
    public void SetState(StationState newState, bool immediate = false)
    {
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
            default: return null;
        }
    }
}