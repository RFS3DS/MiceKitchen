using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoughSpawner : MonoBehaviour
{
    public GameObject doughPrefab;
    public Vector3 spawnPosition = new Vector3(0, 0, 0);
    private GameObject currentDough;

    void OnMouseDown()
    {
        // ПРОВЕРКА: Если мы не на станции готовки, клик не работает
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.CurrentState != StationState.Prep)
        {
            return;
        }
        SpawnDough();
    }

    public void SpawnDough()
    {
        if (currentDough != null) return;
        // ЭТО СООБЩЕНИЕ ДОЛЖНО ПОЯВИТЬСЯ В КОНСОЛИ
        Debug.Log("Click!");
        currentDough = Instantiate(doughPrefab, spawnPosition, Quaternion.identity);
    }
}