using UnityEngine;

/// <summary>
/// 所有 UI 尺寸的唯一来源。
/// 改这个资源（Assets/Resources/UILayoutConfig.asset）就能调整整套界面，
/// 不用改代码、也不用重跑生成器 —— Canvas 版和 OnGUI 版都读它。
/// 资源不存在时会在编辑器里自动生成一份默认值。
/// </summary>
[CreateAssetMenu(fileName = "UILayoutConfig", menuName = "Battle/UI/Layout Config")]
public class UILayoutConfig : ScriptableObject
{
    [Header("通用（Canvas Scaler 参考分辨率）")]
    public float referenceWidth = 1920f;
    public float referenceHeight = 1080f;
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;

    [Header("Canvas 手牌条（贴底）")]
    public float handPanelHeight = 126f;
    public float handPanelBottom = 6f;
    public float tipTop = -4f;
    public float tipHeight = 32f;
    public int tipFontSize = 20;
    public float cardRowTop = -40f;
    public float cardRowHeight = 78f;
    public float movementSlotWidth = 170f;
    public float cardSlotWidth = 150f;
    public float cardSlotHeight = 78f;
    public float cardGap = 8f;
    public int cardFontSize = 17;

    [Header("Canvas 右侧竖栏（宽度默认跟随棋盘右缘）")]
    public float railTop = 40f;
    public float railBottomClearance = 130f;
    public float railMinWidth = 260f;
    public float railMaxWidth = 460f;
    public float railPadding = 8f;
    [Tooltip("大于 0 就用固定宽度，不再跟随棋盘右缘")]
    public float railFixedWidth = 0f;
    public int infoFontSize = 19;
    public float roleTop = -8f;
    public float roleHeight = 76f;
    public float skillTop = -92f;
    public float skillHeight = 290f;
    public float previewTop = -392f;
    public float previewHeight = 60f;

    [Header("OnGUI 手牌（按 F3 切回的那套）")]
    public float onGuiBaseHeight = 900f;
    public float onGuiScaleMin = 0.75f;
    public float onGuiScaleMax = 1f;
    public float onGuiCompactCardHeight = 46f;
    public float onGuiCompactCardHeightMin = 30f;
    public float onGuiCardWidthMin = 74f;
    public float onGuiCardWidthMax = 138f;
    public float onGuiCardGap = 6f;
    public int onGuiTipFontSize = 15;
    public int onGuiCardFontSize = 12;
    public int onGuiInfoFontSize = 13;
    public float onGuiInfoLinePadding = 6f;
    public float onGuiDetailedBlockOffset = 210f;
    public float onGuiDetailedCardHeight = 62f;
    public float onGuiCompactInfoLines = 3f;
    public float onGuiInfoPanelWidth = 900f;
    public float onGuiInfoPanelHeight = 118f;
    public float onGuiRightPanelTop = 34f;
    public float onGuiRightPanelMinWidth = 170f;
    public float onGuiRightRoleLines = 3f;
    public float onGuiRightSkillLines = 7f;
    public float onGuiRightPreviewLines = 2f;

    [Header("部署 HUD（左上角）")]
    public Vector2 deploymentPanelPos = new Vector2(10f, 10f);
    public Vector2 deploymentPanelSize = new Vector2(430f, 134f);

    [Header("选秀面板（居中）")]
    public float draftTitleY = 50f;
    public Vector2 draftTitleSize = new Vector2(500f, 50f);
    public float draftTitleFontSize = 28f;
    public float draftButtonY = 150f;
    public Vector2 draftButtonSize = new Vector2(300f, 150f);
    public float draftButtonFontSize = 24f;
    public float draftButtonOffsetX = 350f;
    public float draftButtonRightGap = 50f;

    private const string ResourcePath = "UILayoutConfig";

    private static UILayoutConfig instance;

    public static UILayoutConfig Get()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = Resources.Load<UILayoutConfig>(ResourcePath);

#if UNITY_EDITOR
        if (instance == null)
        {
            instance = CreateDefaultAsset();
        }
#endif

        if (instance == null)
        {
            //打包后资源缺失的兜底：用代码里的默认值跑，不至于崩
            instance = CreateInstance<UILayoutConfig>();

            Debug.LogWarning("找不到 UI 布局配置，暂时用代码默认值");
        }

        return instance;
    }

    //改了资源后想立刻生效（编辑器里用）
    public static void ClearCache()
    {
        instance = null;
    }

#if UNITY_EDITOR
    private static UILayoutConfig CreateDefaultAsset()
    {
        const string folder = "Assets/Resources";
        const string path = folder + "/UILayoutConfig.asset";

        if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
        }

        UILayoutConfig created = CreateInstance<UILayoutConfig>();
        UnityEditor.AssetDatabase.CreateAsset(created, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        Debug.Log(
            $"已自动生成 UI 布局配置：{path}\n" +
            "所有界面尺寸都在这个资源里，改完重新 Play 就生效"
        );

        return created;
    }
#endif
}