using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Обязательно для работы со сценами

public class MainMenu : MonoBehaviour
{
    [Header("Панель Настроек")]
    public GameObject settingsPanel;

    // Вызывается при нажатии "ИГРАТЬ"
    public void PlayGame()
    {
        // Загружает следующую сцену по порядку в Build Settings (индекс 1)
        SceneManager.LoadScene(1);
        // Или можно по названию: SceneManager.LoadScene("SampleScene");
    }

    // Вызывается при нажатии "НАСТРОЙКИ"
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // Вызывается при нажатии "ЗАКРЫТЬ" в настройках
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Вызывается для выхода из игры (работает на сборке в телефоне)
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Игра закрыта!");
    }
}