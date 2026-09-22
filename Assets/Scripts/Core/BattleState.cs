public enum BattleState
{
    None,

    CharacterDraft,//角色选择
    CardSetup,//卡牌分配
    Deployment,//角色摆放


    //玩家回合
    PlayerTurn,
    //敌人回合
    EnemyTurn,
    //选择角色
    SelectCharacter,
    //已经选择角色，等待玩家选择操作
    CharacterAction,
    //移动阶段
    MovePhase,
    //攻击阶段
    AttackPhase,
    //行动结束
    EndAction,
    //战斗结束
    BattleEnd
}