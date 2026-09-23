using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas 版手牌面板（P3）
/// 只负责「显示 + 转发点击」，数据全部从 PlayerCardManager 的公开接口拉：
///   GetHandTipText / GetRoleInfoText / GetSkillInfoText / GetShortPreviewText
///   GetSkillHand / GetSelectedCard / IsMovementCardInHand / GetMovementCardValue
/// 用 PlayerCardManager 上的 F3 在 OnGUI 版和 Canvas 版之间切换
/// </summary>
public class HandView : MonoBehaviour
{
    [Header("数据源（玩家那一侧的 PlayerCardManager）")]
    [SerializeField]
    private PlayerCardManager cardManager;

    [Header("文本")]
    [SerializeField]
    private TMP_Text tipText;

    [SerializeField]
    private TMP_Text roleText;

    [SerializeField]
    private TMP_Text skillText;

    [SerializeField]
    private TMP_Text previewText;

    [Header("卡槽：移动卡 1 个 + 技能卡 5 个")]
    [SerializeField]
    private CardButtonView movementSlot;

    [SerializeField]
    private CardButtonView[] skillSlots;

    [Header("右侧竖栏（按棋盘右缘定位，窄窗口也不会压棋盘）")]
    [SerializeField]
    private RectTransform rightRail;

    [SerializeField]
    private float railMinWidth = 260f;

    [SerializeField]
    private float railMaxWidth = 460f;

    [SerializeField]
    private float railPadding = 8f;

    [Header("刷新间隔（秒），0 表示每帧都刷")]
    [SerializeField]
    private float refreshInterval = 0.15f;

    private BattleManager battleManager;
    private Camera mainCamera;

    private bool visible = true;
    private bool appliedVisible;
    private bool appliedOn;

    private float nextRefresh;

    //文本缓存：内容没变就不碰 TMP
    private string lastTip;
    private string lastRole;
    private string lastSkill;
    private string lastPreview;

    private void Awake()
    {
        if (cardManager == null)
        {
            cardManager = FindPlayerSideCardManager();
        }

        ApplyLayoutFromConfig();

        if (movementSlot != null)
        {
            movementSlot.Setup(OnSlotClicked);
        }

        if (skillSlots != null)
        {
            for (int i = 0; i < skillSlots.Length; i++)
            {
                if (skillSlots[i] != null)
                {
                    skillSlots[i].Setup(OnSlotClicked);
                }
            }
        }
    }

    private void Update()
    {
        if (cardManager == null)
        {
            cardManager = FindPlayerSideCardManager();

            if (cardManager == null)
            {
                return;
            }
        }

        if (refreshInterval > 0f && Time.unscaledTime < nextRefresh)
        {
            return;
        }

        nextRefresh = Time.unscaledTime + refreshInterval;

        Refresh();
    }

    //F1 控制显隐（由 PlayerCardManager 转发过来）
    public void SetVisible(bool value)
    {
        visible = value;
    }

    //立即刷新（点完卡的立刻反馈用）
    public void Refresh()
    {
        if (cardManager == null)
        {
            return;
        }

        ApplyVisible();

        //显隐被关掉时不用刷内容
        if (!appliedOn)
        {
            return;
        }

        PlaceRightRail();

        SetText(tipText, ref lastTip, cardManager.GetHandTipText() + "　[F1 隐藏]");
        SetText(roleText, ref lastRole, cardManager.GetRoleInfoText());
        SetText(skillText, ref lastSkill, cardManager.GetSkillInfoText());
        SetText(previewText, ref lastPreview, cardManager.GetShortPreviewText());

        //移动卡槽
        if (movementSlot != null)
        {
            if (cardManager.IsMovementCardInHand())
            {
                movementSlot.SetMovementCard(
                    cardManager.GetMovementCardValue(),
                    cardManager.IsMovementCardUsed(),
                    cardManager.IsMovementCardModeActive()
                );
            }
            else
            {
                movementSlot.SetEmpty();
            }
        }

        //技能卡槽
        List<SkillCard> hand = cardManager.GetSkillHand();
        SkillCard selected = cardManager.GetSelectedCard();

        if (skillSlots == null)
        {
            return;
        }

        for (int i = 0; i < skillSlots.Length; i++)
        {
            CardButtonView slot = skillSlots[i];

            if (slot == null)
            {
                continue;
            }

            if (hand != null && i < hand.Count && hand[i] != null)
            {
                SkillCard card = hand[i];

                slot.SetCard(card);
                slot.SetSelected(card == selected);
            }
            else
            {
                slot.SetEmpty();
            }
        }
    }

    //点和棋盘无关的东西时也要刷新（点卡后立刻更新选中态）
    private void OnSlotClicked(CardButtonView slot)
    {
        if (cardManager == null || slot == null)
        {
            return;
        }

        if (slot.IsMovementCard())
        {
            cardManager.OnMovementCardClicked();
        }
        else if (slot.GetCard() != null)
        {
            cardManager.OnSkillCardClicked(slot.GetCard());
        }

        Refresh();
    }

    //右侧竖栏贴着棋盘右缘摆：把棋盘最右一列投影到屏幕，再换算成 Canvas 本地坐标
    private void PlaceRightRail()
    {
        if (rightRail == null)
        {
            return;
        }

        Camera cam = GetMainCamera();

        if (cam == null)
        {
            return;
        }

        Canvas canvas = rightRail.GetComponentInParent<Canvas>();
        RectTransform canvasRect = (canvas != null)
            ? canvas.transform as RectTransform
            : null;

        if (canvasRect == null)
        {
            return;
        }

        Vector3 screenPoint = cam.WorldToScreenPoint(
            new Vector3(BoardManager.BOARD_WIDTH - 0.5f, 0f, 0f)
        );

        Camera uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : canvas.worldCamera;

        Vector2 local;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                new Vector2(screenPoint.x, 0f),
                uiCamera,
                out local))
        {
            return;
        }

        UILayoutConfig config = UILayoutConfig.Get();

        float width;

        if (config != null && config.railFixedWidth > 0f)
        {
            //配置里指定了固定宽度：不再跟随棋盘
            width = config.railFixedWidth;
        }
        else
        {
            float boardRight = local.x + railPadding;
            float railRight = canvasRect.rect.width * 0.5f - railPadding;

            width = Mathf.Clamp(
                railRight - boardRight,
                railMinWidth,
                railMaxWidth
            );
        }

        Vector2 size = rightRail.sizeDelta;

        if (!Mathf.Approximately(size.x, width))
        {
            size.x = width;
            rightRail.sizeDelta = size;
        }
    }

    private Camera GetMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        return mainCamera;
    }

    // =========================
    // 把 UILayoutConfig 里的尺寸应用到面板上
    // 好处：改资源 → 重进 Play 就生效，不用重跑生成器，也不会被生成器覆盖
    // =========================
    //改完 Assets/Resources/UILayoutConfig.asset 后，右键这个组件 →「套用布局配置」即可立刻看到效果
    [ContextMenu("套用布局配置")]
    public void ApplyLayoutFromConfig()
    {
        UILayoutConfig config = UILayoutConfig.Get();

        if (config == null)
        {
            return;
        }

        //右侧竖栏的定位参数也从配置读（PlaceRightRail 会用到）
        railMinWidth = config.railMinWidth;
        railMaxWidth = config.railMaxWidth;
        railPadding = config.railPadding;

        //--- 手牌条 ---
        RectTransform panel = transform.Find("HandPanel") as RectTransform;

        if (panel != null)
        {
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, config.handPanelHeight);
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, config.handPanelBottom);

            RectTransform tip = panel.Find("TipText") as RectTransform;

            if (tip != null)
            {
                tip.anchoredPosition = new Vector2(tip.anchoredPosition.x, config.tipTop);
                tip.sizeDelta = new Vector2(tip.sizeDelta.x, config.tipHeight);
                SetFontSize(tip, config.tipFontSize);
            }

            RectTransform row = panel.Find("CardRow") as RectTransform;

            if (row != null)
            {
                row.anchoredPosition = new Vector2(row.anchoredPosition.x, config.cardRowTop);
                row.sizeDelta = new Vector2(row.sizeDelta.x, config.cardRowHeight);

                HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();

                if (layout != null)
                {
                    layout.spacing = config.cardGap;
                }
            }
        }

        ApplySlotLayout(movementSlot, config.movementSlotWidth, config);
        ApplySlotLayout(skillSlots, config.cardSlotWidth, config);

        //--- 右侧竖栏 ---
        if (rightRail != null)
        {
            rightRail.anchoredPosition = new Vector2(
                rightRail.anchoredPosition.x,
                -config.railTop
            );

            rightRail.sizeDelta = new Vector2(
                config.railFixedWidth > 0f
                    ? config.railFixedWidth
                    : rightRail.sizeDelta.x,
                -(config.railTop + config.railBottomClearance)
            );

            ApplyTextLayout(rightRail, "RoleText", config.roleTop, config.roleHeight, config);
            ApplyTextLayout(rightRail, "SkillText", config.skillTop, config.skillHeight, config);
            ApplyTextLayout(rightRail, "PreviewText", config.previewTop, config.previewHeight, config);
        }
    }

    private void ApplySlotLayout(CardButtonView slot, float width, UILayoutConfig config)
    {
        if (slot == null)
        {
            return;
        }

        LayoutElement element = slot.GetComponent<LayoutElement>();

        if (element != null)
        {
            element.preferredWidth = width;
            element.preferredHeight = config.cardSlotHeight;
        }

        slot.ApplyLayout(config.cardFontSize);
    }

    private void ApplySlotLayout(CardButtonView[] slots, float width, UILayoutConfig config)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            ApplySlotLayout(slots[i], width, config);
        }
    }

    private void ApplyTextLayout(
        RectTransform parent,
        string childName,
        float top,
        float height,
        UILayoutConfig config)
    {
        RectTransform rect = parent.Find(childName) as RectTransform;

        if (rect == null)
        {
            return;
        }

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, top);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);

        SetFontSize(rect, config.infoFontSize);
    }

    private void SetFontSize(RectTransform rect, int fontSize)
    {
        TMP_Text text = rect.GetComponent<TMP_Text>();

        if (text != null)
        {
            text.fontSize = fontSize;
        }
    }

    private void SetText(TMP_Text target, ref string cache, string value)
    {
        if (target == null || cache == value)
        {
            return;
        }

        cache = value;
        target.text = value;
    }

    //选秀 / 部署阶段还没进战斗，手牌面板整体藏起来（和 OnGUI 版行为一致）
    private void ApplyVisible()
    {
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        bool inBattle = battleManager != null &&
                        battleManager.GetCurrentState() != BattleState.None;

        //OnGUI 版接管时（F3 切回 / Inspector 取消勾选）自己藏起来，避免两套 UI 同时出现
        bool on = visible && inBattle && cardManager.IsCanvasUIEnabled();

        if (appliedVisible && appliedOn == on)
        {
            return;
        }

        appliedVisible = true;
        appliedOn = on;

        //HandView 挂在一个常驻节点上，只切换子节点，
        //否则自己被 SetActive(false) 之后 Update 就不再执行了
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(on);
        }
    }

    //场景里有两个 PlayerCardManager（玩家 / 对手），要挑玩家那个
    private PlayerCardManager FindPlayerSideCardManager()
    {
        PlayerCardManager[] all =
            FindObjectsByType<PlayerCardManager>(FindObjectsSortMode.None);

        foreach (PlayerCardManager manager in all)
        {
            if (manager != null && !manager.IsEnemySide())
            {
                return manager;
            }
        }

        return null;
    }
}