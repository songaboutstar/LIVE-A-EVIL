using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillCard",
    menuName = "Battle/Card/Skill Card"
)]




public class SkillCard : ActionCard
{

    [Header("所属角色")]
    [SerializeField]
    private CharacterData ownerCharacter;

    [Header("自定义目标范围")]
    [SerializeField]
    private List<Vector2Int> customRange = new List<Vector2Int>();

    [Header("移动")]
    [SerializeField]
    private int moveRange = 0;

    [Header("技能")]
    [SerializeField]
    private int damage = 3;

    [Header("射程")]
    [SerializeField]
    private int range = 1;

    [SerializeField]
    private RangeType rangeType = RangeType.Single;

    [Header("效果范围")]
    [SerializeField]
    private int effectRange = 0;

    [SerializeField]
    private RangeType effectRangeType = RangeType.Single;

    [Header("特殊效果（可多选）")]
    [SerializeField]
    private SkillEffectType[] effectTypes = new SkillEffectType[0];

    [Header("回复量（带「回复」效果时生效）")]
    [SerializeField]
    private int healAmount = 2;

    [Header("计时器")]
    [SerializeField]
    private int timer = 0;

    public int GetMoveRange()
    {
        return moveRange;
    }

    public CharacterData GetOwnerCharacter()
    {
        return ownerCharacter;
    }
    public int GetDamage()
    {
        return damage;
    }

    public int GetRange()
    {
        return range;
    }
    public RangeType GetRangeType()
    {
        return rangeType;
    }

    public RangeType GetEffectRangeType()
    {
        return effectRangeType;
    }
    public int GetEffectRange()
    {
        return effectRange;
    }

    public int GetHealAmount()
    {
        return healAmount;
    }

    public SkillEffectType[] GetEffectTypes()
    {
        return effectTypes;
    }

    /// <summary>
    /// 这张卡是否带有某个特殊效果（一张卡可以有多个效果）
    /// </summary>
    public bool HasEffect(SkillEffectType type)
    {
        if (effectTypes == null)
        {
            return false;
        }

        for (int i = 0; i < effectTypes.Length; i++)
        {
            if (effectTypes[i] == type)
            {
                return true;
            }
        }

        return false;
    }

    public int GetTimer()
    {
        return timer;
    }
    
    public List<Vector2Int> GetCustomRange()
    {
        return customRange;
    }

    public void SetCustomRange(List<Vector2Int> range)
    {
        customRange = new List<Vector2Int>(range);
    }
}