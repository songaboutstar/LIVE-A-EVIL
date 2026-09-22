using System.Collections.Generic;
using UnityEngine;
//技能计时卡管理器
//负责
//  1、保存当前正在等待发动的技能卡
//2、每轮减少计时器
//3、计时结束后通知SkillManager发动技能

public class TimerManager : MonoBehaviour
{
    private SkillManager skillManager;
    private readonly List<PendingSkill> pendingSkills = new List<PendingSkill>();
    
    
    // Start is called before the first frame update
    private void Start()
    {
        skillManager = FindFirstObjectByType<SkillManager>();

        if (skillManager == null)
        {
            Debug.LogError("TimerManager：找不到SkillManager!");
        }
    }
    
    //添加一个正在等待发动的技能
    public void AddPendingSkill(Character character,SkillCard card,Tile targetTile)
    {
        if (character == null)
        {
            Debug.LogWarning("TimerManager：角色为空！");
            return;
        }
        if (card == null)
        {
            Debug.LogWarning("TimerManager：技能卡为空！");
            return;
        }
        if (targetTile == null)
        {
            Debug.LogWarning("TimerManager：目标格为空！");
            return;
        }

        int timer = card.GetTimer();
        if (timer <= 0)
        {
            Debug.LogWarning("TimerManager：计时器必须大于0！");
            return;
        }

        PendingSkill pendingSkill = new PendingSkill(character,card,targetTile,timer);
        pendingSkills.Add(pendingSkill);

        Debug.Log($"技能卡进入计时器：{card.GetCardName()},角色：{character.GetCharacterName()}，目标：{targetTile.GetGridPosition()}，剩余轮数:{timer}");
    }


    [ContextMenu("测试：处理一轮计时器")]
    //一轮结束时调用
    public void ProcessRound()
    {
        if (pendingSkills.Count == 0)
        {
            Debug.Log("TimerManager：当前没有计时中的技能。");
            return;
        }

        Debug.Log($"TimerManager：开始处理计时器，当前数量：{pendingSkills.Count}");

        //从后往前遍历，方便删除
        for(int i = pendingSkills.Count - 1; i >= 0; i--)
        {
            PendingSkill pending = pendingSkills[i];

            pending.remainingRounds--;

            Debug.Log($"技能卡：{pending.card.GetCardName()}，剩余轮数：{pending.remainingRounds}");

            if (pending.remainingRounds <= 0)
            {
                pendingSkills.RemoveAt(i);

               

                    if (skillManager != null)
                    {
                        skillManager.ExecutePendingSkill(pending.character,pending.card,pending.targetTile);
                    }
                
            }
        }
    }

    //当前计时中的技能数量
    public int GetPendingSkillCount()
    {
        return pendingSkills.Count;
    }

    //一个正在计时的技能
    private class PendingSkill {

        public Character character;
        public SkillCard card;
        public Tile targetTile;
        public int remainingRounds;

        public PendingSkill(Character character,SkillCard card,Tile targetTile,int remainingRounds)
        {
            this.character = character;
            this.card = card;
            this.targetTile = targetTile;
            this.remainingRounds = remainingRounds;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
