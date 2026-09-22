
using UnityEngine;


[CreateAssetMenu(
    fileName ="CharacterData",
    menuName ="Battle/Character/Character Data"
    )]
public class CharacterData : ScriptableObject
{
    [Header("角色信息")]
    [SerializeField]
    private string characterName;

    [SerializeField]
    private int maxHp = 10;

    [Header("专属技能卡")]
    [SerializeField]
    private SkillCard[] skillCards = new SkillCard[3];

    public string GetCharacterName() 
    {
        return characterName;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }

    public SkillCard[] GetSkillCards()
    {
        return skillCards;
    }

    public SkillCard GetSkillCard(int index)
    {
        if (skillCards == null)
            return null;
        if (index < 0 || index >= skillCards.Length)
            return null;

        return skillCards[index];
    }
}
