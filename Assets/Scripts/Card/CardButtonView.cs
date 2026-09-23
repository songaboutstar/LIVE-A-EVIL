using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 手牌里的一张卡（Canvas 版，挂在每个卡槽上）
/// 只负责显示 + 把点击转发出去，数据由 HandView 灌进来
/// </summary>
public class CardButtonView : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private GameObject selectedMark;

    [Header("三种状态的颜色")]
    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color selectedColor = Color.yellow;

    [SerializeField]
    private Color usedColor = Color.gray;

    private SkillCard card;

    private bool isMovementCard;
    private bool isUsed;
    private bool highlight;
    private bool isActiveSlot;
    private int moveDistance = -1;

    private Action<CardButtonView> clicked;

    //HandView 注册点击回调
    public void Setup(Action<CardButtonView> onSlotClicked)
    {
        clicked = onSlotClicked;

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public SkillCard GetCard()
    {
        return card;
    }

    public bool IsMovementCard()
    {
        return isMovementCard;
    }

    //技能卡槽：内容没变就不重复刷新（避免每帧重建 TMP 网格）
    public void SetCard(SkillCard value)
    {
        if (isActiveSlot && !isMovementCard && card == value)
        {
            return;
        }

        card = value;
        isMovementCard = false;
        isUsed = false;
        highlight = false;
        moveDistance = -1;

        if (value == null)
        {
            SetEmpty();
            return;
        }

        isActiveSlot = true;
        gameObject.SetActive(true);

        if (label != null)
        {
            label.text =
                $"{value.GetCardName()}\n" +
                $"伤害{value.GetDamage()} · 计时{value.GetTimer()}";
        }

        if (selectedMark != null)
        {
            selectedMark.SetActive(false);
        }

        ApplyColor();
    }

    //移动卡槽
    public void SetMovementCard(int distance, bool used, bool active)
    {
        if (isActiveSlot && isMovementCard &&
            moveDistance == distance && isUsed == used && highlight == active)
        {
            return;
        }

        card = null;
        isMovementCard = true;
        isUsed = used;
        highlight = active;
        moveDistance = distance;
        isActiveSlot = true;

        gameObject.SetActive(true);

        if (label != null)
        {
            label.text = used
                ? "移动卡\n（已使用）"
                : $"移动卡\n{distance}格";
        }

        if (selectedMark != null)
        {
            selectedMark.SetActive(active);
        }

        ApplyColor();
    }

    //空槽位：整个槽藏起来
    public void SetEmpty()
    {
        if (!isActiveSlot && !gameObject.activeSelf)
        {
            return;
        }

        card = null;
        isMovementCard = false;
        isUsed = false;
        highlight = false;
        moveDistance = -1;
        isActiveSlot = false;

        if (selectedMark != null)
        {
            selectedMark.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (highlight == selected)
        {
            return;
        }

        highlight = selected;

        if (selectedMark != null)
        {
            selectedMark.SetActive(selected);
        }

        ApplyColor();
    }

    private void ApplyColor()
    {
        if (label == null)
        {
            return;
        }

        if (isUsed)
        {
            label.color = usedColor;

            return;
        }

        label.color = highlight ? selectedColor : normalColor;
    }

    private void OnClick()
    {
        if (clicked != null)
        {
            clicked(this);
        }
    }
}