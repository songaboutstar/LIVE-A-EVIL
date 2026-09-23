using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    private Character selectedCharacter;
    //[Header("默认移动距离")]
   // [SerializeField]
    // private int movementRange = 4;
   
    private BoardManager boardManager;
    private ActionUIManager actionUIManager;

    private SkillCard selectedSkillCard;
    //是否处于技能攻击后的移动
    private bool isSkillMove = false;

    //当前这一次移动允许的最大距离
    private int currentMoveRange = 0;

    //是否处于「移动卡」移动（点移动卡 → 点己方角色 → 走格子）
    private bool isMovementCardMove = false;

    //本轮移动卡是否已经用过（用过就消耗掉，一轮一张）
    private bool movementCardUsed = false;

    // 当前可以移动到的 Tile
    private readonly List<Tile> movableTiles = new List<Tile>();

    private void Start()
    {
        boardManager =
            FindFirstObjectByType<BoardManager>();
        actionUIManager =
            FindAnyObjectByType<ActionUIManager>();

        if (boardManager == null)
        {
            Debug.LogError("MovementManager：找不到 BoardManager！");
        }
        if (actionUIManager==null)
        {
            Debug.LogError("MovementManager:找不到ActionUIManager!");
        }
    }

    /// <summary>
    /// 选择角色并计算移动范围
    /// </summary>
    public void SelectCharacter(Character character)
    {
        if (character == null)
        {
            return;
        }
        //清除之前角色选中状态
        if (selectedCharacter != null)
        {
            selectedCharacter.SetSelected(false);
        }
        //清除之前的移动范围
        ClearMovementRange();


        //普通移动
        isSkillMove = false;
        isMovementCardMove = false;
        //设置新的选中角色
        selectedCharacter = character;

        //设置新角色为选中状态
        selectedCharacter.SetSelected(true);

        CalculateMovementRange();

        Debug.Log(
            $"选择角色：{character.gameObject.name}，" +
            $"阵营：{character.GetTeam()}"+
            $"当前位置：{character.GetGridPosition()}，" +
            $"移动距离：{character.GetMovementRange()}"
        );
    }
    //开始技能攻击后的追加移动
    public void StartSkillMove(Character character,int moveRange)
    {
        if (character == null)
        {
            Debug.LogWarning("技能移动失败：角色为空！");
            return;
        }

        if (moveRange <= 0)
        {
            Debug.Log("该技能没有攻击后的移动");
            return;
        }

        //清除之前的移动范围
        ClearMovementRange();
        //技能移动状态
        isSkillMove = true;
        isMovementCardMove = false;
        //当前角色
        selectedCharacter = character;
        //确保角色保持选中
        selectedCharacter.SetSelected(true);

        Debug.Log($"开始技能后移动：{character.GetCharacterNameV2()},最多移动{moveRange}格");

        //使用相同BFS
        CalculateMovementRange(moveRange);
    }

    // =========================
    // 移动卡：点移动卡 → 点己方任意角色 → 走格子 → 消耗这张卡
    // 移动值来自角色身上挂的移动卡（MovementCard_4 = 4格）
    // =========================
    public void StartMovementCardMove(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("移动卡：没有选择角色！");
            return;
        }

        //先清掉上一次的范围
        ClearMovementRange();

        //一个角色一回合只能移动一次
        if (character.HasMoved())
        {
            Debug.Log(
                $"{character.GetCharacterNameV2()} 本回合已经移动过了，" +
                $"移动卡不能用在他身上"
            );
            return;
        }

        MovementCard movementCard = character.GetMovementCard();

        int moveDistance = movementCard != null
            ? movementCard.GetMoveDistance()
            : character.GetMovementRange();

        if (movementCard == null)
        {
            Debug.LogWarning(
                $"{character.GetCharacterNameV2()}身上没有移动卡，" +
                $"改用角色自身移动力 {moveDistance}"
            );
        }

        if (selectedCharacter != null && selectedCharacter != character)
        {
            selectedCharacter.SetSelected(false);
        }

        isSkillMove = false;
        //标记：这一次移动是移动卡走出来的，走完要消耗掉这张卡
        isMovementCardMove = true;

        selectedCharacter = character;
        selectedCharacter.SetSelected(true);

        Debug.Log(
            $"使用移动卡：{character.GetCharacterNameV2()} 最多移动 {moveDistance} 格"
        );

        if (moveDistance <= 0)
        {
            Debug.Log("移动卡移动值为0，不能移动");
            return;
        }

        CalculateMovementRange(moveDistance);
    }

    /// <summary>本轮移动卡是否已经用过</summary>
    public bool HasUsedMovementCard()
    {
        return movementCardUsed;
    }

    /// <summary>新一轮开始：移动卡恢复可用</summary>
    public void ResetMovementCardUsed()
    {
        movementCardUsed = false;
        isMovementCardMove = false;

        Debug.Log("移动卡：新一轮开始，移动卡恢复可用");
    }

    /// <summary>
    /// 计算当前角色可以移动到哪些格子
    /// </summary>
    private void CalculateMovementRange()
    {
        if (selectedCharacter == null)
        {
            return;
        }

        // int movementRange =
        //   selectedCharacter.GetMovementRange();
        //稍微修改移动逻辑
        MovementCard movementCard = selectedCharacter.GetMovementCard();

        int movementRange;

        if (movementCard != null)
        {
            movementRange = movementCard.GetMoveDistance();
        }
        else
        {
            //兜底：运行时可创建的角色身上可能没挂移动卡，
            //退回角色自身的移动力（默认4，等同移动卡的4格），避免直接卡死
            movementRange = selectedCharacter.GetMovementRange();

            Debug.LogWarning(
                $"{selectedCharacter.GetCharacterNameV2()}没有移动卡，" +
                $"改用角色自身移动力 {movementRange}"
            );
        }
       
        //currentMoveRange = movementRange;



        GridPosition startPosition =
            selectedCharacter.GetGridPosition();

        Queue<MovementNode> queue =
            new Queue<MovementNode>();

        HashSet<GridPosition> visited =
            new HashSet<GridPosition>();

        queue.Enqueue(
            new MovementNode(
                startPosition,
                0
            )
        );

        visited.Add(startPosition);

        while (queue.Count > 0)
        {
            MovementNode current =
                queue.Dequeue();

            if (current.distance >= movementRange)
            {
                continue;
            }

            TryAddPosition(
                current,
                1,
                0,
                queue,
                visited
            );

            TryAddPosition(
                current,
                -1,
                0,
                queue,
                visited
            );

            TryAddPosition(
                current,
                0,
                1,
                queue,
                visited
            );

            TryAddPosition(
                current,
                0,
                -1,
                queue,
                visited
            );
        }

        Debug.Log(
            $"可移动格数量：{movableTiles.Count}"
        );
    }

    private void CalculateMovementRange(int movementRange)
    {
        if (selectedCharacter == null)
        {
            return;
        }

        if (movementRange <= 0)
        {
            return;
        }

        //currentMoveRange = movementRange;

        GridPosition startPosition =
            selectedCharacter.GetGridPosition();

        Queue<MovementNode> queue =
            new Queue<MovementNode>();

        HashSet<GridPosition> visited =
            new HashSet<GridPosition>();

        queue.Enqueue(
            new MovementNode(
                startPosition,
                0
            )
        );

        visited.Add(startPosition);

        while (queue.Count > 0)
        {
            MovementNode current =
                queue.Dequeue();

            if (current.distance >= movementRange)
            {
                continue;
            }

            TryAddPosition(
                current,
                1,
                0,
                queue,
                visited
            );

            TryAddPosition(
                current,
                -1,
                0,
                queue,
                visited
            );

            TryAddPosition(
                current,
                0,
                1,
                queue,
                visited
            );

            TryAddPosition(
                current,
                0,
                -1,
                queue,
                visited
            );
        }

        Debug.Log(
            $"可移动格数量：{movableTiles.Count}，" +
            $"最大移动距离：{movementRange}"
        );
    }

    /// <summary>
    /// 尝试加入一个相邻格
    /// </summary>
    private void TryAddPosition(
     MovementNode current,
     int offsetX,
     int offsetY,
     Queue<MovementNode> queue,
     HashSet<GridPosition> visited)
    {
        GridPosition nextPosition =
            new GridPosition(
                current.position.x + offsetX,
                current.position.y + offsetY
            );

        // 超出棋盘
        if (!boardManager.IsValidPosition(
            nextPosition))
        {
            return;
        }

        // 已经访问过
        if (visited.Contains(nextPosition))
        {
            return;
        }

        Tile nextTile =
            boardManager.GetTile(nextPosition);

        if (nextTile == null)
        {
            return;
        }

        visited.Add(nextPosition);

        GameObject occupant =
            nextTile.GetOccupant();

        // =========================
        // 当前格没有角色
        // =========================

        if (occupant == null)
        {
            int nextDistance =
                current.distance + 1;

            movableTiles.Add(nextTile);

            //nextTile.SetMovable(true);
            HighlightMovableTile(nextTile);
            //继续BFS
            queue.Enqueue(
                new MovementNode(
                    nextPosition,
                    nextDistance
                )
            );

            return;
        }


        // =========================
        // 当前格有角色
        // =========================

        Character otherCharacter =
            occupant.GetComponent<Character>();

        if (otherCharacter == null)
        {
            return;
        }


        // =========================
        // 己方角色
        // =========================

        if (otherCharacter.GetTeam()
            == selectedCharacter.GetTeam())
        {
            /*
             * 己方角色可以穿过，
             * 但是不能作为最终落点。
             *
             * 因此：
             * 1. 不加入 movableTiles
             * 2. 继续 BFS
             */

            int nextDistance =
                current.distance + 1;

            queue.Enqueue(
                new MovementNode(
                    nextPosition,
                    nextDistance
                )
            );

            return;
        }


        // =========================
        // 敌方角色
        // =========================

        /*
         * 敌方角色阻挡移动。
         *
         * 不能进入，
         * 也不能继续穿过。
         */

        return;
    }

    /// <summary>
    /// 高亮可移动 Tile
    /// </summary>
    private void HighlightMovableTile(Tile tile)
    {
        tile.SetVisualState(TileVisualState.Movable);
    }

    /// <summary>
    /// 清除所有移动范围
    /// </summary>
    public void ClearMovementRange()
    {
        foreach (Tile tile in movableTiles)
        {
            if (tile != null)
            {
                tile.SetVisualState(TileVisualState.Normal);
            }
        }

        movableTiles.Clear();
    }

    /// <summary>
    /// 判断一个 Tile 是否在移动范围内
    /// </summary>
    public bool IsMovableTile(Tile tile)
    {
        return movableTiles.Contains(tile);
    }

    /// <summary>
    /// 执行移动
    /// </summary>
    public void MoveCharacter(Tile targetTile)
    {
        if (selectedCharacter == null)
        {
            return;
        }

        if (!IsMovableTile(targetTile))
        {
            Debug.Log("目标格不在移动范围内！");
            return;
        }

        selectedCharacter.MoveTo(targetTile);
        selectedCharacter.MarkMoved();

        //移动卡走完：消耗掉这张卡（本轮不能再点移动卡）
        if (isMovementCardMove)
        {
            isMovementCardMove = false;
            movementCardUsed = true;

            //退出移动卡模式（手牌上的移动卡随之置灰）
            BattleManager battleManager =
                FindFirstObjectByType<BattleManager>();

            if (battleManager != null)
            {
                battleManager.ExitMovementCardMode();
            }

            Debug.Log("移动卡已经使用（本轮不能再点移动卡了）");
        }

        ClearMovementRange();

        Debug.Log(
            $"角色移动完成，新位置：{targetTile.GetGridPosition()}"
        );
        if (actionUIManager != null)
        {
            actionUIManager.ShowActionPanel();
        }
    }



    private struct MovementNode
    {
        public GridPosition position;
        public int distance;

        public MovementNode(
            GridPosition position,
            int distance)
        {
            this.position = position;
            this.distance = distance;
        }
    }


    public void StartSkillCardMove(Character character,SkillCard skillCard)
    {
        if (character == null)
        {
            Debug.LogWarning("移动失败：没有选择角色卡！");
            return;
        }
        if (skillCard == null)
        {
            Debug.LogWarning("移动失败：没有选择技能卡！");
            return;
        }

        int moveRange = skillCard.GetMoveRange();

        Debug.Log($"使用技能卡：{skillCard.GetCardName()},移动值：{moveRange}");

        //1、这一次不是移动卡移动，别把移动卡误消耗掉
        isMovementCardMove = false;

        //2、先清掉上一张卡留下的移动范围：
        //   否则切换到「移动值为0」的卡时，棋盘上会残留上一张卡的范围
        ClearMovementRange();

        //3、切换角色选中高亮
        if (selectedCharacter != null && selectedCharacter != character)
        {
            selectedCharacter.SetSelected(false);
        }

        selectedCharacter = character;
        selectedCharacter.SetSelected(true);

        //4、已经移动过的角色：不显示移动范围（一个角色一回合只能移动一次）
        if (character.HasMoved())
        {
            Debug.Log(
                $"{character.GetCharacterNameV2()} 本回合已经移动过了，" +
                $"不再显示移动范围"
            );
            return;
        }

        //5、移动值为0的卡：范围已经清空，本回合不能移动
        if (moveRange <= 0)
        {
            Debug.Log(
                $"「{skillCard.GetCardName()}」移动值为0：" +
                $"已清空移动范围，本回合不能移动"
            );
            return;
        }

        //6、按这张卡的实际移动值画移动范围
        CalculateMovementRange(moveRange);
    }

    public void SelectSkillCard(SkillCard card)
    {
        if (card == null)
            return;

        selectedSkillCard = card;

        Debug.Log(
            $"当前选择技能卡：{card.GetCardName()}"
        );
    }
}