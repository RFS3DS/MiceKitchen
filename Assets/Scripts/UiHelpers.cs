using UnityEngine;
using UnityEngine.UI;

// ============================================================
// ПОМОЩНИКИ ДЛЯ UI — чтобы все новые скрипты ставились на сцену
// "как есть", без ручного создания канвасов и текстов.
// ============================================================
public static class UiHelpers
{
    // Находит любой канвас на сцене, а если его нет — создаёт новый
    public static Canvas EnsureCanvas(string canvasName, int sortOrder)
    {
        Canvas existing = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (existing != null) return existing;

        GameObject go = new GameObject(canvasName);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    // Стандартный шрифт (для старых и новых версий Unity)
    public static Font GetDefaultFont()
    {
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null)
        {
            try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        }
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 28);
        return font;
    }

    // Создаёт Text как ребёнка parent
    public static Text CreateText(Transform parent, string name, string content,
        int fontSize, TextAnchor alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Text t = go.AddComponent<Text>();
        t.font = GetDefaultFont();
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // Создаёт кнопку с надписью (якоря по умолчанию — центр)
    public static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPosition, Vector2 size, Color color, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPosition;

        Image img = go.GetComponent<Image>();
        img.color = color;

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        Text t = CreateText(go.transform, "Label", label, fontSize, TextAnchor.MiddleCenter, Color.white);
        Stretch(t.rectTransform, 4f, 4f, 4f, 4f);
        return btn;
    }

    // Растягивает RectTransform на весь родителя с отступами
    public static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    // Форматирует время в ММ:СС (или Ч:ММ:СС, если час и больше)
    public static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(Mathf.Max(0f, seconds));
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;

        if (h > 0) return string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
        return string.Format("{0:D2}:{1:D2}", m, s);
    }
}
