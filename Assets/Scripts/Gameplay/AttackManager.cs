using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    // 当前选择的角色
    private Character selectedCharacter;

    // 棋盘管理器
    private BoardManager boardManager;

    // 战斗管理器（攻击结束后用它返回行动面板）
    private BattleManager battleManager;

    // 当前可以攻击的 Tile
    private readonly List<Tile> attackableTiles =
        new List<Tile>();

    // 当前角色攻击范围内的所有 Tile（含空格与己方）
    private readonly List<Tile> attackRangeTiles =
        new List<Tile>();

    private SkillCard selectedSkillCard;

    private void Start()
    {
        boardManager =
            FindFirstObjectByType<BoardManager>();

        battleManager =
            FindFirstObjectByType<BattleManager>();

        if (battleManager == null)
        {
            Debug.LogError(
                "AttackManager：找不到 BattleManager！"
            );
        }

        if (boardManager == null)
        {
            Debug.LogError(
                "AttackManager：找不到 BoardManager！"
            );
        }
    }


    /// <summary>
    /// 选择角色
    /// </summary>
    public void SelectCharacter(Character character)
    {
        if (character == null)
        {
            return;
        }

        // 清除之前的攻击范围
        ClearAttackRange();

        // 设置当前角色
        selectedCharacter = character;

        selectedSkillCard = character.GetDefaultSkillCard();
        if (selectedSkillCard == null)
        {
            Debug.LogWarning($"{character.GetCharacterName()}没有技能卡！");
            return;
        }

        Debug.Log(
            $"AttackManager 选择角色：" +
            $"{character.GetCharacterName()}"
        );
    }


    /// <summary>
    /// 显示攻击范围
    /// </summary>
    public void ShowAttackRange()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning(
                "AttackManager：没有选择角色！"
            );

            return;
        }

        if (boardManager == null)
        {
            Debug.LogError(
                "AttackManager：BoardManager为空！"
            );

            return;
        }


        // 清除旧攻击范围
        ClearAttackRange();


        GridPosition center =
            selectedCharacter.GetGridPosition();

        //int range =
        //  selectedCharacter.GetAttackRange();
        if (selectedSkillCard == null)
        {
            Debug.LogWarning("没有选择技能卡！");
            return;
        }
        int range = selectedSkillCard.GetRange();

        // =========================
        // 遍历攻击范围
        // =========================

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                // 自己所在的位置
                if (x == 0 && y == 0)
                {
                    continue;
                }


                // 曼哈顿距离
                int distance =
                    Mathf.Abs(x) + Mathf.Abs(y);

                if (distance > range)
                {
                    continue;
                }


                GridPosition position =
                    new GridPosition(
                        center.x + x,
                        center.y + y
                    );


                // 超出棋盘
                if (!boardManager.IsValidPosition(
                    position))
                {
                    continue;
                }


                Tile tile =
                    boardManager.GetTile(position);

                if (tile == null)
                {
                    continue;
                }


                // 范围内所有格子先标记为攻击范围
                attackRangeTiles.Add(tile);

                tile.SetVisualState(
                    TileVisualState.AttackRange
                );


                // 获取 Tile 上的角色
                Character target =
                    GetCharacterOnTile(tile);


                // 没有角色
                if (target == null)
                {
                    continue;
                }


                // 己方角色
                if (target.GetTeam() ==
                    selectedCharacter.GetTeam())
                {
                    continue;
                }


                // =========================
                // 敌方角色
                // =========================

                attackableTiles.Add(tile);

                tile.SetVisualState(
                    TileVisualState.Attackable
                );
            }
        }


        Debug.Log(
            $"攻击范围计算完成，" +
            $"范围格数量：{attackRangeTiles.Count}，" +
            $"可攻击目标数量：{attackableTiles.Count}"
        );
    }


    /// <summary>
    /// 判断 Tile 上是否存在角色
    /// </summary>
    private Character GetCharacterOnTile(Tile tile)
    {
        if (tile == null)
        {
            return null;
        }

        GameObject occupant =
            tile.GetOccupant();

        if (occupant == null)
        {
            return null;
        }

        return occupant.GetComponent<Character>();
    }


    /// <summary>
    /// 判断 Tile 是否可以攻击
    /// </summary>
    public bool IsAttackableTile(Tile tile)
    {
        return attackableTiles.Contains(tile);
    }


    /// <summary>
    /// 执行攻击
    /// </summary>
    public void Attack(Tile targetTile)
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning(
                "AttackManager：没有选择攻击者！"
            );

            return;
        }


        if (targetTile == null)
        {
            return;
        }


        // 目标不在攻击范围
        if (!IsAttackableTile(targetTile))
        {
            Debug.Log(
                "这个 Tile 不在攻击范围内！"
            );

            return;
        }


        // 获取敌人
        Character target =
            GetCharacterOnTile(targetTile);


        if (target == null)
        {
            Debug.LogWarning(
                "攻击目标不存在！"
            );

            return;
        }


        // =========================
        // 计算伤害
        // =========================

       // int damage =
         //   Mathf.Max(
           //     1,
             //   selectedCharacter.GetAttack()
               // - target.GetDefense()
            //);
        int damage = selectedSkillCard.GetDamage();

        Debug.Log(
            $"{selectedCharacter.GetCharacterName()} " +
            $"攻击 " +
            $"{target.GetCharacterName()}，" +
            $"攻击力：{selectedCharacter.GetAttack()}，" +
            $"防御力：{target.GetDefense()}，" +
            $"造成伤害：{damage}"
        );


        // 造成伤害
        target.TakeDamage(damage);
        selectedCharacter.MarkAttacked();

        // 攻击结束
        ClearAttackRange();

        // 攻击结束，返回行动面板
        if (battleManager != null)
        {
            battleManager.ReturnToCharacterAction();
        }
        else
        {
            Debug.LogWarning(
                "AttackManager：找不到 BattleManager，" +
                "无法返回行动面板！"
            );
        }
    }


    /// <summary>
    /// 清除攻击范围
    /// </summary>
    public void ClearAttackRange()
    {
        foreach (Tile tile in attackRangeTiles)
        {
            if (tile != null)
            {
                tile.SetVisualState(
                    TileVisualState.Normal
                );
            }
        }

        foreach (Tile tile in attackableTiles)
        {
            if (tile != null &&
                !attackRangeTiles.Contains(tile))
            {
                tile.SetVisualState(
                    TileVisualState.Normal
                );
            }
        }

        attackRangeTiles.Clear();
        attackableTiles.Clear();
    }
}