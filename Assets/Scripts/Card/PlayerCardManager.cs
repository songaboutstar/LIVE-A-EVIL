
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardManager : MonoBehaviour
{

    [Header("玩家名称")]
    [SerializeField]
    private string playerName = "Player";

    [Header("固定移动卡")]
    [SerializeField]
    private bool hasMovementCard = true;

    [Header("手牌UI（在场景里建好 CardUI 槽后拖上来）")]
    [SerializeField]
    private HandUIManager handUIManager;

    [Header("调试：直接在屏幕上显示手牌（按 F1 开关）")]
    [SerializeField]
    private bool showDebugHandUI = true;

    private BattleManager battleManager;

    //12张技能卡牌堆
    private List<SkillCard> skillDeck = new List<SkillCard>();

    //当前5张手牌
    private List<SkillCard> skillHand = new List<SkillCard>();

    //当前是否持有移动卡
    private bool movementCardInHand;

    public void Initialize(List<CharacterData> characters)
    {
        skillDeck.Clear();
        skillHand.Clear();

        movementCardInHand = hasMovementCard;
        if (characters == null)
        {
            Debug.LogError($"{playerName}：角色列表为空！");
            return;
        }

        //4个角色，每个角色3张技能卡
        foreach(CharacterData character in characters)
        {
            if (character == null)
                continue;
            SkillCard[] cards = character.GetSkillCards();

            if (cards == null)
                continue;
            foreach(SkillCard card in cards)
            {
                if (card != null)
                {
                    skillDeck.Add(card);
                }
            }
        }
        Debug.Log($"{playerName}牌堆初始化完成：技能卡{skillDeck.Count}张,移动卡{(movementCardInHand ? 1:0)}张");

        ShuffleDeck();
        DrawIntialHand();
    }

    private void ShuffleDeck()
    {
        for(int i = skillDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            SkillCard temp = skillDeck[i];
            skillDeck[i] = skillDeck[randomIndex];
            skillDeck[randomIndex] = temp;
        }
    }

    private void DrawIntialHand()
    {
        const int initialSkillHandCount = 5;

        int drawCount = Mathf.Min(initialSkillHandCount,skillDeck.Count);
        
        for(int i = 0; i < drawCount; i++)
        {
            SkillCard card = skillDeck[0];

            skillDeck.RemoveAt(0);
            skillHand.Add(card);
        }
        Debug.Log($"{playerName}初始手牌，技能卡{skillHand.Count}张，移动卡{(movementCardInHand?1:0)}张");

        PrintHand();

        // 把初始手牌推给手牌 UI（如果场景里已经建好并拖进来）
        if (handUIManager != null)
        {
            handUIManager.SetHand(skillHand.ToArray());
        }
    }

    // 调试用：不依赖任何 UI 物体，直接在屏幕上画出手牌
    // 按 F1 随时开关；选秀 / 角色摆放阶段会自动隐藏，避免挡住棋盘
    private void OnGUI()
    {
        if (Event.current != null &&
            Event.current.type == EventType.KeyDown &&
            Event.current.keyCode == KeyCode.F1)
        {
            showDebugHandUI = !showDebugHandUI;

            Debug.Log($"手牌调试显示：{(showDebugHandUI ? "开" : "关")}");
        }

        if (!showDebugHandUI)
            return;

        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        // 还没进入战斗（state == None，即选秀 / 角色摆放阶段）就不显示
        if (battleManager == null ||
            battleManager.GetCurrentState() == BattleState.None)
        {
            return;
        }

        bool isEnemy = playerName != null && playerName.Contains("Enemy");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 20;

        GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
        cardStyle.fontSize = 16;
        cardStyle.alignment = TextAnchor.MiddleCenter;

        // 对手手牌画在棋盘上方、自己的画在棋盘下方，横向再宽也不会压住棋盘
        float y = isEnemy ? 10f : Screen.height - 120f;

        GUI.Label(
            new Rect(10, y, 900, 28),
            $"{playerName}手牌：技能卡 {skillHand.Count} 张 + 移动卡 {(movementCardInHand ? 1 : 0)} 张",
            titleStyle
        );

        y += 30f;

        float x = 10f;

        if (movementCardInHand)
        {
            GUI.Box(new Rect(x, y, 110, 52), "移动卡\n4格", cardStyle);
            x += 118f;
        }

        for (int i = 0; i < skillHand.Count; i++)
        {
            SkillCard card = skillHand[i];
            string label = card != null ? card.GetCardName() : "(空)";

            GUI.Box(new Rect(x, y, 110, 52), $"技能卡{i + 1}\n{label}", cardStyle);
            x += 118f;
        }
    }

    private void PrintHand()
    {
        Debug.Log($"=============={playerName}当前手牌=================");

        if (movementCardInHand)
        {
            Debug.Log("手牌：移动卡");
        }

        for(int i = 0; i < skillHand.Count; i++)
        {
            SkillCard card = skillHand[i];

            if (card == null)
                continue;
            Debug.Log($"技能卡{i + 1}：{card.GetCardName()}");
        }
        Debug.Log("===============================================");

    }

    public List<SkillCard> GetSkillHand()
    {
        return skillHand;
    }
    public List<SkillCard> GetSkillDeck()
    {
        return skillDeck;

    }

    public bool HasMovementCard()
    {
        return movementCardInHand;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}
