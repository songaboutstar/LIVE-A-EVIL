
using UnityEngine;

public class ActionCard : ScriptableObject
{
    [Header("ø®≈∆–≈œ¢")]
    [SerializeField]
    private string cardName;

    [TextArea]
    [SerializeField]
    private string description;
    
    public string GetCardName()
    {
        return cardName;
    }

    public string GetDescription()
    {
        return description;
    }
}
