
using UnityEngine;

[CreateAssetMenu(
    fileName ="MovementCard",
    menuName ="Battle/Card/Movement Card"
    )]
public class MovementCard:ActionCard
{
    [Header("ÒÆ¶¯")]
    [SerializeField]
    private int moveDistance = 4;

    public int GetMoveDistance()
    {
        return moveDistance;
    }
}