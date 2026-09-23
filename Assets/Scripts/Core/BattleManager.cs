
using UnityEngine;

public class BattleManager: MonoBehaviour
{
    private BattleState currentState = BattleState.None;

    private Character currentCharacter;

    //是否处于「移动卡模式」（点移动卡后，等着点一个己方角色）
    private bool movementCardMode = false;

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
       
       
       //新一局：移动卡恢复可用
       if (movementManager != null)
       {
           movementManager.ResetMovementCardUsed();
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

    // =========================
    // 卡牌驱动的角色选择
    // 点技能卡时调用：让这张卡所属的角色进入选中阶段
    // 规则：还没移动之前可以自由换卡预览；
    //       一旦移动过，就不能再切换到别的角色
    // =========================
    public bool SelectCharacterByCard(Character character, SkillCard card)
    {
        if (character == null)
        {
            return false;
        }

        if (character.GetTeam() != Team.Player)
        {
            Debug.Log("只能选择玩家角色！");
            return false;
        }

        //同一个角色：直接放行（用于切换它自己的不同技能卡）
        if (currentCharacter == character)
        {
            return true;
        }

        //已经移动过的角色：不允许再切到别的角色
        if (currentCharacter != null && currentCharacter.HasMoved())
        {
            Debug.Log(
                $"{currentCharacter.GetCharacterNameV2()} 本回合已经移动过，" +
                $"不能再切换到其他角色了"
            );
            return false;
        }

        //点了技能卡：退出移动卡模式
        movementCardMode = false;

        currentCharacter = character;
        currentState = BattleState.CharacterAction;

        Debug.Log($"卡牌选择角色：{character.GetCharacterNameV2()}");

        //显示这个角色的移动范围：移动力来自这张技能卡上的「移動」值
        if (movementManager != null)
        {
            if (card != null)
            {
                movementManager.SelectSkillCard(card);
                movementManager.StartSkillCardMove(character, card);
            }
            else
            {
                movementManager.SelectCharacter(character);
            }
        }

        //让 SkillManager 记住当前角色（具体看哪张卡由点卡的人再传进来）
        if (skillManager != null)
        {
            skillManager.SelectCharacter(character);
        }

        return true;
    }

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

        //移动卡模式：点角色 → 按这张移动卡的移动值显示范围
        if (movementCardMode)
        {
            if (movementManager != null &&
                movementManager.HasUsedMovementCard())
            {
                //移动卡已经消耗掉了，自动退出模式，走普通选角色
                ExitMovementCardMode();
            }
            else
            {
                SelectCharacterByMovementCard(character);
                return;
            }
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

    // =========================
    // 移动卡模式
    // 点移动卡 → 进这里 → 点任意己方角色 → 按移动卡走 → 走完消耗这张卡
    // =========================
    public bool IsMovementCardMode()
    {
        return movementCardMode;
    }

    public void EnterMovementCardMode()
    {
        movementCardMode = true;

        //清掉技能卡那套选中状态，改由移动卡接管
        currentCharacter = null;
        currentState = BattleState.SelectCharacter;

        if (movementManager != null)
        {
            movementManager.ClearMovementRange();
        }
        if (skillManager != null)
        {
            skillManager.ClearSkillPreview();
        }
        if (attackManager != null)
        {
            attackManager.ClearAttackRange();
        }
        if (actionUIManager != null)
        {
            actionUIManager.HideActionPanel();
        }

        Debug.Log("移动卡：请点一个己方角色");
    }

    public void ExitMovementCardMode()
    {
        movementCardMode = false;
    }

    //移动卡模式下点角色：直接按移动卡的移动值显示范围
    private void SelectCharacterByMovementCard(Character character)
    {
        if (character.HasMoved())
        {
            Debug.Log(
                $"{character.GetCharacterNameV2()} 本回合已经移动过了，" +
                $"移动卡不能用在他身上"
            );
            return;
        }

        currentCharacter = character;
        currentState = BattleState.CharacterAction;

        if (movementManager != null)
        {
            movementManager.StartMovementCardMove(character);
        }
        if (skillManager != null)
        {
            skillManager.SelectCharacter(character);
        }

        Debug.Log($"移动卡：{character.GetCharacterNameV2()} 可以移动了");
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

        //新一轮：退出移动卡模式 + 移动卡恢复可用
        //（角色自己的 HasMoved / HasAttacked 复位留给回合框架做）
        movementCardMode = false;

        if (movementManager != null)
        {
            movementManager.ResetMovementCardUsed();
        }

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
