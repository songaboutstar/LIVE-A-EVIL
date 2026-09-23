using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 一键生成 Canvas 版手牌面板（P3a）
/// 菜单：Battle / UI / 3. 生成手牌面板（Canvas）
///
/// 生成的结构：
///   Canvas
///   └─ HandUI                 ← HandView 挂这里（常驻不关，只切子节点显隐）
///      ├─ HandPanel           贴底：提示行 + 手牌一行（移动卡 1 + 技能卡 5）
///      └─ RightInfo           棋盘右侧竖栏：角色 / 技能卡详情 / 预览
///
/// 可以反复执行（会先删掉上一次生成的 HandUI）
/// </summary>
public static class HandPanelBuilder
{
    private const int SkillSlotCount = 5;

    [MenuItem("Battle/UI/3. 生成手牌面板（Canvas）")]
    public static void BuildHandPanel()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("场景里没找到 Canvas！先建一个（GameObject / UI / Canvas）");
            return;
        }

        //先删掉上次生成的，方便反复调整
        Transform old = canvas.transform.Find("HandUI");

        if (old != null)
        {
            Object.DestroyImmediate(old.gameObject);
        }

        // ---------- 根节点（常驻，HandView 挂这里）----------
        GameObject root = CreateUI("HandUI", canvas.transform);
        Stretch(root.GetComponent<RectTransform>());

        // ---------- 底部手牌面板 ----------
        GameObject panel = CreateUI("HandPanel", root.transform);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 6f);
        panelRect.sizeDelta = new Vector2(0f, 126f);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.55f);

        TextMeshProUGUI tip = CreateText(
            "TipText",
            panel.transform,
            20f,
            TextAlignmentOptions.MidlineLeft
        );

        SetTopStretch(
            tip.rectTransform,
            new Vector2(12f, -4f),
            new Vector2(-24f, 32f)
        );

        GameObject row = CreateUI("CardRow", panel.transform);

        SetTopStretch(
            row.GetComponent<RectTransform>(),
            new Vector2(12f, -40f),
            new Vector2(-24f, 78f)
        );

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CardButtonView movementSlot = CreateCardSlot(row.transform, "SlotMovement", 170f);

        CardButtonView[] skillSlots = new CardButtonView[SkillSlotCount];

        for (int i = 0; i < SkillSlotCount; i++)
        {
            skillSlots[i] = CreateCardSlot(row.transform, "SlotSkill" + (i + 1), 150f);
        }

        // ---------- 棋盘右侧竖栏 ----------
        GameObject info = CreateUI("RightInfo", root.transform);

        RectTransform infoRect = info.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(1f, 0f);
        infoRect.anchorMax = new Vector2(1f, 1f);
        infoRect.pivot = new Vector2(1f, 1f);
        infoRect.anchoredPosition = new Vector2(-8f, -40f);
        infoRect.sizeDelta = new Vector2(420f, -180f);

        Image infoBg = info.AddComponent<Image>();
        infoBg.color = new Color(0f, 0f, 0f, 0.55f);

        TextMeshProUGUI role = CreateText(
            "RoleText",
            info.transform,
            20f,
            TextAlignmentOptions.TopLeft
        );

        SetTopStretch(
            role.rectTransform,
            new Vector2(0f, -8f),
            new Vector2(-28f, 76f)
        );

        TextMeshProUGUI skill = CreateText(
            "SkillText",
            info.transform,
            19f,
            TextAlignmentOptions.TopLeft
        );

        SetTopStretch(
            skill.rectTransform,
            new Vector2(0f, -92f),
            new Vector2(-28f, 290f)
        );

        TextMeshProUGUI preview = CreateText(
            "PreviewText",
            info.transform,
            19f,
            TextAlignmentOptions.TopLeft
        );

        SetTopStretch(
            preview.rectTransform,
            new Vector2(0f, -392f),
            new Vector2(-28f, 60f)
        );

        // ---------- HandView + 自动接线 ----------
        HandView view = root.AddComponent<HandView>();

        SerializedObject so = new SerializedObject(view);

        PlayerCardManager playerCards = FindPlayerCardManager();

        SetRef(so, "cardManager", playerCards);
        SetRef(so, "tipText", tip);
        SetRef(so, "roleText", role);
        SetRef(so, "skillText", skill);
        SetRef(so, "previewText", preview);
        SetRef(so, "movementSlot", movementSlot);
        SetRef(so, "rightRail", infoRect);

        SerializedProperty array = so.FindProperty("skillSlots");

        if (array != null)
        {
            array.arraySize = skillSlots.Length;

            for (int i = 0; i < skillSlots.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = skillSlots[i];
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(view);

        //生成的尺寸只是「起步值」，真正生效的是 UILayoutConfig 资源
        view.ApplyLayoutFromConfig();

        //让 PlayerCardManager 记住这个面板，并打开 Canvas 版（F3 可切回 OnGUI 对比）
        if (playerCards != null)
        {
            SerializedObject cardSo = new SerializedObject(playerCards);

            SetRef(cardSo, "handView", view);

            SerializedProperty canvasFlag = cardSo.FindProperty("useCanvasUI");

            if (canvasFlag != null)
            {
                canvasFlag.boolValue = true;
            }

            cardSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(playerCards);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            canvas.gameObject.scene
        );

        Debug.Log(
            "手牌面板已生成：Canvas / HandUI（HandPanel 贴底 + RightInfo 右侧竖栏）\n" +
            "· 按 F3 在「OnGUI 版」和「Canvas 版」之间切换（PlayerCardManager 上也有 useCanvasUI 勾选框）\n" +
            "· F1 隐藏 / 显示，Q 切预览，F2 只影响 OnGUI 版\n" +
            "· 忘了 Ctrl+S 保存场景的话，退出时会丢\n" +
            DescribeActionPanel(canvas)
        );
    }

    // ---------- 工具方法 ----------
    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));

        go.transform.SetParent(parent, false);

        int uiLayer = LayerMask.NameToLayer("UI");

        if (uiLayer >= 0)
        {
            go.layer = uiLayer;
        }

        return go;
    }

    private static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject go = CreateUI(name, parent);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();

        text.font = GetUIFontAsset();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(1f, 0.92f, 0.5f);
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private static CardButtonView CreateCardSlot(
        Transform parent,
        string name,
        float width)
    {
        GameObject slot = CreateUI(name, parent);

        Image background = slot.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.10f);

        Button button = slot.AddComponent<Button>();
        button.targetGraphic = background;

        LayoutElement element = slot.AddComponent<LayoutElement>();
        element.preferredWidth = width;
        element.preferredHeight = 78f;
        element.flexibleWidth = 0f;
        element.flexibleHeight = 0f;

        //选中标记压在文字下面
        GameObject mark = CreateUI("SelectedMark", slot.transform);

        Image markImage = mark.AddComponent<Image>();
        markImage.color = new Color(1f, 0.9f, 0.2f, 0.30f);
        markImage.raycastTarget = false;

        Stretch(mark.GetComponent<RectTransform>());
        mark.SetActive(false);

        TextMeshProUGUI label = CreateText(
            "Label",
            slot.transform,
            17f,
            TextAlignmentOptions.Center
        );

        Stretch(label.rectTransform);
        label.rectTransform.sizeDelta = new Vector2(-10f, -8f);

        CardButtonView view = slot.AddComponent<CardButtonView>();

        SerializedObject so = new SerializedObject(view);

        SetRef(so, "button", button);
        SetRef(so, "label", label);
        SetRef(so, "selectedMark", mark);

        so.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    //UI 文本优先直接用中文字体资源（找不到才退回 TMP 默认字体）
    private static TMP_FontAsset GetUIFontAsset()
    {
        TMP_FontAsset cjk = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/NotoSansSC SDF.asset"
        );

        return (cjk != null) ? cjk : TMP_Settings.defaultFontAsset;
    }

    private static void SetRef(SerializedObject so, string field, Object value)
    {
        SerializedProperty property = so.FindProperty(field);

        if (property == null)
        {
            Debug.LogWarning($"HandPanelBuilder：找不到字段 {field}，请手动在 Inspector 里拖");
            return;
        }

        property.objectReferenceValue = value;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static void SetTopStretch(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    //场景里有两个 PlayerCardManager（玩家 / 对手），要拿玩家那个
    private static PlayerCardManager FindPlayerCardManager()
    {
        PlayerCardManager[] all =
            Object.FindObjectsByType<PlayerCardManager>(FindObjectsSortMode.None);

        foreach (PlayerCardManager manager in all)
        {
            if (manager != null && !manager.IsEnemySide())
            {
                return manager;
            }
        }

        Debug.LogWarning("HandPanelBuilder：没找到玩家侧的 PlayerCardManager，请手动接线");

        return null;
    }

    //顺便报一下 ActionPanel 的位置，避免和新的手牌面板叠在一起
    private static string DescribeActionPanel(Canvas canvas)
    {
        Transform panel = canvas.transform.Find("ActionPanel");

        if (panel == null)
        {
            return "· 场景里没有 ActionPanel，无需担心重叠\n";
        }

        RectTransform rt = panel as RectTransform;

        if (rt == null)
        {
            return "";
        }

        return
            $"· 提醒：ActionPanel 现在在 {rt.anchoredPosition}，尺寸 {rt.sizeDelta}\n" +
            "  新的手牌面板占了屏幕底部 126（参考分辨率下的像素），如果叠上了就把 ActionPanel 往上挪\n";
    }
}