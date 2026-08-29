using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ============================================================
// СОХРАНЕНИЕ ИГРЫ
//  • Монеты, номер дня, статистика
//  • Уровни улучшений кабинета
//  • Файл JSON: <persistentDataPath>/pizzeria_save.json
//  • Автосохранение: конец дня, покупка, выход/сворачивание игры
// ============================================================

// ===== ТИПЫ УЛУЧШЕНИЙ (порядок важен — сохраняются по номеру!) =====
public enum UpgradeType
{
    OvenSpeed = 0,  // Скоростная печь
    Patience = 1,   // Комната ожидания
    DayLength = 2,  // Реклама
    Tips = 3        // Хорошее обслуживание
}

// ===== ДАННЫЕ СОХРАНЕНИЯ =====
[Serializable]
public class GameSaveData
{
    public const int UpgradeCount = 4;

    public int coins = 0;          // монеты
    public int dayNumber = 1;      // какой день по счёту
    public int totalServed = 0;    // всего обслужено клиентов
    public int totalEarned = 0;    // всего заработано
    public int angryClients = 0;   // сколько клиентов ушло злыми
    public List<int> upgradeLevels = new List<int>(); // уровни по порядку UpgradeType

    public void EnsureUpgradeList()
    {
        if (upgradeLevels == null) upgradeLevels = new List<int>();
        while (upgradeLevels.Count < UpgradeCount) upgradeLevels.Add(0);
    }
}

// ===== САМА СИСТЕМА СОХРАНЕНИЯ (статическая, монет не требует) =====
public static class SaveSystem
{
    static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "pizzeria_save.json"); }
    }

    // Текущие данные (загружаются при первом обращении)
    public static GameSaveData Data { get; private set; }

    static SaveSystem()
    {
        Data = Load();
    }

    public static int GetUpgradeLevel(UpgradeType type)
    {
        Data.EnsureUpgradeList();
        return Data.upgradeLevels[(int)type];
    }

    public static void SetUpgradeLevel(UpgradeType type, int level)
    {
        Data.EnsureUpgradeList();
        Data.upgradeLevels[(int)type] = level;
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
        }
        catch (Exception e)
        {
            Debug.LogError("Не удалось сохранить игру: " + e.Message);
        }
    }

    static GameSaveData Load()
    {
        GameSaveData data = new GameSaveData();
        data.EnsureUpgradeList();

        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(json);
                if (loaded != null) data = loaded;
                data.EnsureUpgradeList();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Не удалось загрузить сохранение: " + e.Message);
        }

        return data;
    }

    // Полный сброс прогресса (кнопка «Сбросить прогресс» в кабинете)
    public static void ResetAll()
    {
        Data = new GameSaveData();
        Data.EnsureUpgradeList();
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Не удалось удалить файл сохранения: " + e.Message);
        }
        Save();
    }
}
