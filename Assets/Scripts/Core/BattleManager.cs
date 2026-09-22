
using UnityEngine;

public class BattleManager: MonoBehaviour
{
    private BattleState currentState = BattleState.None;

    private Character currentCharacter;

    private MovementManager movementManager;
    private AttackManager attackManager;


    private ActionUIManager actionUIManager;
    

    //新增了技能模块
    private SkillManager skillManager;
    //技能计时器管理器
    private TimerManager timerManager;


    public void StartBattle()
    {
        movementManager = FindFirstObjectByType<MovementManager>();
        attackManager = FindFirstObjectByType<AttackManager>();
        actionUIManager = FindFirstObjectByType<ActionUIManager>();
        skillManager = FindAnyObjectByType<SkillManager>();
        timerManager = FindFirstObjectByType<TimerManager>();
        if (timerManager == null)
        {
            Debug.LogError("BattleManager：找不到TimerManager!");
        }
       
       
       StartPlayerTurn();
    }

    //开始玩家回合

    private void StartPlayerTurn()
    {
        currentState = BattleState.PlayerTurn;

        Debug.Log("==========玩家回合=========");

        EnterSelectCharacterPhase();
    }

    //进入选择角色
    private void EnterSelectCharacterPhase()
    {
        currentState = BattleState.SelectCharacter;

        if (actionUIManager != null)
        {
            actionUIManager.HideActionPanel();
        }

        Debug.Log("进入：选择角色阶段");
    }
    //选择角色

    public void SelectCharacter(Character character)
    {
        if (character == null)
            return;
        if (currentState != BattleState.SelectCharacter)
        {
            Debug.Log("当前不是选择角色阶段！");
            return;
        }
        if (character.GetTeam() != Team.Player)
        {
            Debug.Log("只能选择玩家角色！");
            return;
        }

        currentCharacter = character;
        currentState = BattleState.CharacterAction;

        Debug.Log($"选择角色：{character.GetCharacterName()}");
        // EnterMovePhase();
        // ShowActionMenu();
        if (actionUIManager != null)
        {
            actionUIManager.ShowActionPanel();
        }
        if (skillManager != null)
        {
            skillManager.SelectCharacter(currentCharacter);
        }
    }

    //进入移动阶段

    public void EnterMovePhase()
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("没有选择角色！");
            return;
        }
       
        if (currentCharacter.HasMoved())
        {
            Debug.Log("该角色本回合已经移动过了");
            return;
        }
        currentState = BattleState.MovePhase;

        if (actionUIManager != null)
        {
            actionUIManager.HideActionPanel();
        }

        if (movementManager != null)
        {
            movementManager.SelectCharacter(currentCharacter);
        }
        Debug.Log($"进入：移动阶段:{currentCharacter.GetCharacterName()}");
    }

   //进入攻击阶段
   public void EnterAttackPhase()
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("当前没有角色！");
            return;
        }
        if (currentCharacter.HasAttacked())
        {
            Debug.Log("该角色本回合已经攻击过了");
            return;
        }
        currentState = BattleState.AttackPhase;
        if (actionUIManager != null)
        {
            actionUIManager.HideActionPanel();
        }

        if (attackManager != null)
        {
            attackManager.SelectCharacter(currentCharacter);
            attackManager.ShowAttackRange();
        }
        Debug.Log($"进入：攻击阶段{currentCharacter.GetCharacterName()}");
        
    }
    //攻击结束，返回角色行动阶段
    public void ReturnToCharacterAction()
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("当前没有角色！");
            return;
        }

        currentState = BattleState.CharacterAction;

        if (actionUIManager != null)
        {
            actionUIManager.ShowActionPanel();
        }

        Debug.Log($"返回：角色行动阶段 {currentCharacter.GetCharacterName()}");
    }
    //结束当前角色行动
    public void EndCharacterAction()
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("当前没有角色！");
            return;
        }
       // currentState = BattleState.EndAction;
        Debug.Log($"角色：{currentCharacter.GetCharacterName()}行动结束");

        if (movementManager != null)
        {
            movementManager.ClearMovementRange();
        }
        if (attackManager != null)
        {
            attackManager.ClearAttackRange();
        }
        if (actionUIManager != null)
        {
            actionUIManager.HideActionPanel();
        }
        currentCharacter.SetSelected(false);
        currentCharacter = null;
        currentState = BattleState.EndAction;
        Debug.Log("当前角色行动结束");
        EnterSelectCharacterPhase();
    }

    public BattleState GetCurrentState()
    {
        return currentState;
    }
    public Character GetCurrentCharacter()
    {
        return currentCharacter;
    }

    private void ShowActionMenu()
    {
        currentState = BattleState.SelectCharacter;
        if (actionUIManager != null)
        {
            actionUIManager.ShowActionPanel();
        }
        Debug.Log("显示行动菜单");
    }
    //一整个回合结束
    [ContextMenu("测试：结束一回合")]
    public void EndRound()
    {
        Debug.Log("===========回合结束==========");

        if (timerManager != null)
        {
            timerManager.ProcessRound();
        }
        else
        {
            Debug.LogWarning("BattleManager：TimerManager为空，无法处理技能计时器！");
        }
        Debug.Log("===========回合处理完成============");
    }

}
