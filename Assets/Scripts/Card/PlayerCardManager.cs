
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
