using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        float boardRight = local.x + railPadding;
        float railRight = canvasRect.rect.width * 0.5f - railPadding;

        float width = Mathf.Clamp(
            railRight - boardRight,
            railMinWidth,
            railMaxWidth
        );

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