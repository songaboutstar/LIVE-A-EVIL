
using System.Collections.Generic;
using UnityEngine;

//技能管理器
//负责
//1：选择技能卡
//2：根据技能卡计算射程
//3：显示可以选择的目标格
public class SkillManager : MonoBehaviour
{

    private Tile selectedTargetTile;
    private Character selectedCharacter;
    private SkillCard selectedCard;
    private BoardManager boardManager;

    private TimerManager timerManager;
    //当前可以选择的格
    private readonly List<Tile> targetTiles = new List<Tile>();
    private readonly List<Tile> effectTiles = new List<Tile>();
    // Start is called before the first frame update
    void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("SkillManager:找不到BoardManager!");

        }
        timerManager = FindAnyObjectByType<TimerManager>();
        if (timerManager == null)
        {
            Debug.LogError("SkillManager：找不到TimeManager!");
        }
    }
    //选择角色

    public void SelectCharacter(Character character)
    {
        if (character == null)
            return;
        ClearSkill();
        selectedCharacter = character;
        Debug.Log($"SkillManage选择角色：{character.GetCharacterName()}");
    }

    //选择技能卡
    public void SelectSkillCard(SkillCard card)
    {
        if (card == null)
        {
            Debug.LogWarning("SkillManager:技能卡为空！");
            return;
        }

        if (selectedCharacter == null)
        {
            Debug.LogWarning("SkillManager:还没有选择角色！");
            return;
        }
        if (boardManager == null)
        {
            Debug.LogError("SkillManager:BoardManager为空！");
            return;
        }

        ClearTargetTiles();
        selectedCard = card;

        Debug.Log($"选择技能卡{card.GetCardName()}");

        CalculateTargetTiles();
    }
    //根据卡牌计算射程
    private void CalculateTargetTiles()
    {
        if (selectedCharacter == null)
        {
            return;
        }
        if (selectedCard == null)
        {
            return;
        }
        GridPosition center = selectedCharacter.GetGridPosition();

        int range = selectedCard.GetRange();

        RangeType rangeType = selectedCard.GetRangeType();

        switch (rangeType)
        {
            case RangeType.Single:
                CalculateSingle(center, range);
                break;
            case RangeType.Cross:
                CalculateCross(center, range);
                break;
            case RangeType.Horizontal:
                CalculateHorizontal(center, range);
                break;
            case RangeType.Square:
                CalculateSquare(center,range);
                break;
            case RangeType.CrossWithDiagonal:
                CalculateCrossWithDiagonal(center, range);
                break;
            case RangeType.HorizontalInfinite:
                CalculateHorizontalInfinite(center, range);
                break;
            case RangeType.Vertical:
                CalculateVertical(center, range);
                break;
            case RangeType.VerticalInfinite:
                CalculateVerticalInfinite(center, range);
                break;
            case RangeType.DirectLine:
                CalculateDirectLine(center, range);
                break;
            case RangeType.DiagonalFourAtTwo:
                CalculateDiagonalFourAtTwo(center);
                break;
            case RangeType.Custom:
                CalculateCustomRange(center);
                break;
            case RangeType.CrossInfinite:
                CalculateCrossInfinite(center);
                break;
            case RangeType.CrossWithDiagonalInfinite:
                CalculateCrossWithDiagonalInfinite(center);
                break;
        }

        Debug.Log($"技能射程计算完成，可选择目标数量：{targetTiles.Count}");
    }
    //单点
    private void CalculateSingle(GridPosition center,int range)
    {
        if (range <= 0)
            return;
        //第一版：Single按照曼哈顿距离寻找目标
        for(int x = -range; x <= range; x++)
        {
            for(int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                int distance = Mathf.Abs(x) + Mathf.Abs(y);

                if (distance > range)
                    continue;
                AddTargetTile(center.x + x, center.y + y);
            }
        }
    }

    //自定义
    private void CalculateCustomRange(GridPosition center)
    {
        if (selectedCard == null)
            return;
        List<Vector2Int> customRange = selectedCard.GetCustomRange();

        if (customRange == null)
            return;

        foreach(Vector2Int offset in customRange)
        {
            AddTargetTile(center.x+offset.x,center.y+offset.y);
        }
    }
    //斜线四个
    private void CalculateDiagonalFourAtTwo(GridPosition center)
    {
        AddTargetTile(center.x - 2, center.y + 2);
        AddTargetTile(center.x + 2, center.y + 2);

        AddTargetTile(center.x - 2, center.y - 2);
        AddTargetTile(center.x + 2, center.y - 2);

    }

    //十字
    private void CalculateCross(GridPosition center,int range)
    {
        for(int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x + i, center.y);
            AddTargetTile(center.x - i, center.y);

            AddTargetTile(center.x, center.y + i);
            AddTargetTile(center.x, center.y - i);
        }
    }
    //横向
    private void CalculateHorizontal(GridPosition center,int range)
    {
        for(int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x + i, center.y);
            AddTargetTile(center.x - i, center.y);
        }
    }

    //纵向
    private void CalculateVertical(GridPosition center,int range)
    {
        for(int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x, center.y + i);
            AddTargetTile(center.x, center.y - i);
        }
    }

    //方形
    private void CalculateSquare(GridPosition center,int range)
    {
        for(int x = -range; x <= range; x++)
        {
            for(int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                AddTargetTile(center.x + x, center.y + y);
            }
        }
    }
    //十字+斜线
    private void CalculateCrossWithDiagonal(GridPosition center,int range)
    {
        for(int i = 1; i <= range; i++)
        {
            //横
            AddTargetTile(center.x + i, center.y);
            AddTargetTile(center.x - i, center.y);

            //纵
            AddTargetTile(center.x, center.y + i);
            AddTargetTile(center.x, center.y - i);
            //四个斜角
            AddTargetTile(center.x + i, center.y + i);
            AddTargetTile(center.x + i, center.y - i);
            AddTargetTile(center.x - i, center.y + i);
            AddTargetTile(center.x - i, center.y - i);
        }
    }
    //横向无限
    //横向无限：一条横带，纵向共 (2*range+1) 行，横向贯穿整盘
    //卡面写「縦N×横∞」时，range 填 (N-1)/2
    private void CalculateHorizontalInfinite(GridPosition center, int range)
    {
        for (int dy = -range; dy <= range; dy++)
        {
            int y = center.y + dy;

            if (y < 0 || y >= BoardManager.BOARD_HEIGHT)
                continue;

            for (int x = 0; x < BoardManager.BOARD_WIDTH; x++)
            {
                //射程不包含自己所在的格子
                if (x == center.x && y == center.y)
                    continue;

                AddTargetTile(x, y);
            }
        }
    }
    //纵向无限
    //纵向无限：一条竖带，横向共 (2*range+1) 列，纵向贯穿整盘
    //卡面写「縦∞×横M」时，range 填 (M-1)/2
    private void CalculateVerticalInfinite(GridPosition center, int range)
    {
        for (int dx = -range; dx <= range; dx++)
        {
            int x = center.x + dx;

            if (x < 0 || x >= BoardManager.BOARD_WIDTH)
                continue;

            for (int y = 0; y < BoardManager.BOARD_HEIGHT; y++)
            {
                //射程不包含自己所在的格子
                if (x == center.x && y == center.y)
                    continue;

                AddTargetTile(x, y);
            }
        }
    }

    //十字无限延伸（叉形）：上下左右各自延伸到棋盘边缘
    private void CalculateCrossInfinite(GridPosition center)
    {
        for (int x = 0; x < BoardManager.BOARD_WIDTH; x++)
        {
            //射程不包含自己所在的格子
            if (x != center.x)
            {
                AddTargetTile(x, center.y);
            }
        }

        for (int y = 0; y < BoardManager.BOARD_HEIGHT; y++)
        {
            if (y != center.y)
            {
                AddTargetTile(center.x, y);
            }
        }
    }

    //米字形无限延伸：十字 + 四条斜线，各自延伸到棋盘边缘
    private void CalculateCrossWithDiagonalInfinite(GridPosition center)
    {
        CalculateCrossInfinite(center);

        int maxStep = Mathf.Max(
            BoardManager.BOARD_WIDTH,
            BoardManager.BOARD_HEIGHT
        );

        for (int i = 1; i <= maxStep; i++)
        {
            AddTargetTile(center.x + i, center.y + i);
            AddTargetTile(center.x + i, center.y - i);
            AddTargetTile(center.x - i, center.y + i);
            AddTargetTile(center.x - i, center.y - i);
        }
    }

    //直线攻击
    private void CalculateDirectLine(GridPosition center,int range)
    {
        //第一版先按照十字直线处理
        //  CalculateCross(center, range);

        if (range <= 0)
            return;
        //向上
        for(int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x, center.y + i);
        }

        //向下
        for(int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x,center.y-i);
        }
        //向左
        for (int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x-i, center.y);
        }
        //向右
        for (int i = 1; i <= range; i++)
        {
            AddTargetTile(center.x+i, center.y);
        }
    }

    //添加目标格
    private void AddTargetTile(int x,int y)
    {
        GridPosition position = new GridPosition(x, y);

        if (!boardManager.IsValidPosition(position))
            return;
        Tile tile = boardManager.GetTile(position);
        if (tile == null)
            return;
        if (targetTiles.Contains(tile))
            return;
        targetTiles.Add(tile);

        // tile.SetVisualState(TileVisualState.Attackable);
        tile.SetVisualState(TileVisualState.SkillTarget);
    }

    //判断是否是技能目标
    public bool IsTargetTile(Tile tile)
    {
        return targetTiles.Contains(tile);
    }

    //获取当前技能卡
    public SkillCard GetSelectedCard()
    {
        return selectedCard;
    }


    //选择技能目标
    public void SelectTarget(Tile targetTile)
    {
        if (targetTile == null)
            return;
        if (!targetTiles.Contains(targetTile))
        {
            Debug.Log("格子不是技能目标");
            return;
        }
        selectedTargetTile = targetTile;
        Debug.Log($"选择技能目标：{targetTile.GetGridPosition()}");

        //清除蓝色目标
        ClearTargetTiles();
        //保存目标
        CalculateEffectArea(targetTile);
    }

    private Tile GetSelectedTargetTile()
    {
        return selectedTargetTile;
    }

    //计算效果范围
    private void CalculateEffectArea(Tile targetTile)
    {
        if (selectedCard == null)
            return;
        GridPosition center = targetTile.GetGridPosition();
        int range = selectedCard.GetEffectRange();

        RangeType rangeType = selectedCard.GetEffectRangeType();

        //无限延伸类型不受 range 数值影响（range 只是有限类型的半径）
        bool isInfiniteType =
            rangeType == RangeType.HorizontalInfinite ||
            rangeType == RangeType.VerticalInfinite ||
            rangeType == RangeType.CrossInfinite ||
            rangeType == RangeType.CrossWithDiagonalInfinite;

        if (range <= 0 && !isInfiniteType)
        {
            AddEffectTile(center.x, center.y);
            return;
        }
        switch (rangeType)
        {
            case RangeType.Single:
                AddEffectTile(center.x, center.y);
                break;
            case RangeType.Cross:
                CalculateEffectCross(center, range);
                break;
            case RangeType.Horizontal:
                CalculateEffectHorizontal(center, range);
                break;
            case RangeType.Vertical:
                CalculateEffectVertical(center, range);
                break;
            case RangeType.Square:
                CalculateEffectSquare(center, range);
                break;
            case RangeType.CrossWithDiagonal:
                CalculateEffectCrossWithDiagonal(center, range);
                break;
            case RangeType.DirectLine:
                CalculateEffectDirectLine(selectedCharacter.GetGridPosition(), center);
                break;
            case RangeType.HorizontalInfinite:
                CalculateEffectHorizontalInfinite(center, range);
                break;
            case RangeType.VerticalInfinite:
                CalculateEffectVerticalInfinite(center, range);
                break;
            case RangeType.CrossInfinite:
                CalculateEffectCrossInfinite(center);
                break;
            case RangeType.CrossWithDiagonalInfinite:
                CalculateEffectCrossWithDiagonalInfinite(center);
                break;
            default:
                AddEffectTile(center.x, center.y);
                break;
        }
        Debug.Log($"效果范围计算完成：{effectTiles.Count}格");
    }

    //效果范围方法
    //单点
    private void AddEffectTile(int x, int y)
    {
        GridPosition position = new GridPosition(x, y);
        if (!boardManager.IsValidPosition(position))
            return;
        Tile tile = boardManager.GetTile(position);
        if (tile == null)
            return;
        if (effectTiles.Contains(tile))
            return;
        effectTiles.Add(tile);
        tile.SetVisualState(TileVisualState.EffectArea);
    }
    //十字
    private void CalculateEffectCross(GridPosition center,int range)
    {
        AddEffectTile(center.x, center.y);
        for(int i = 1; i <= range; i++)
        {
            AddEffectTile(center.x + i, center.y);
            AddEffectTile(center.x - i, center.y);

            AddEffectTile(center.x, center.y + i);
            AddEffectTile(center.x, center.y - i);
        }
    }
    //横向
    private void CalculateEffectHorizontal(GridPosition center,int range)
    {
        AddEffectTile(center.x, center.y);

        for(int i = 1; i <= range; i++)
        {
            AddEffectTile(center.x + i, center.y);
            AddEffectTile(center.x - i, center.y);
        }
    }
    //纵向
    private void CalculateEffectVertical(GridPosition center,int range)
    {
        AddEffectTile(center.x, center.y);
        for(int i = 1; i <= range; i++)
        {
            AddEffectTile(center.x, center.y + i);
            AddEffectTile(center.x, center.y - i);
        }
    }
    //方块
    private void CalculateEffectSquare(GridPosition center,int range)
    {
        for(int x = -range; x <= range; x++)
        {
            for(int y = -range; y <= range; y++)
            {
                AddEffectTile(center.x + x, center.y + y);
            }
        }
    }

    //十字+斜线
    private void CalculateEffectCrossWithDiagonal(GridPosition center,int range)
    {
        for(int x = -range; x <= range; x++)
        {
            for(int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) == Mathf.Abs(y) || x == 0 || y == 0)
                {
                    AddEffectTile(center.x + x, center.y + y);
                }
            }
        }
    }

    private void CalculateEffectDirectLine(
    GridPosition start,
    GridPosition target)
    {
        int dx = target.x - start.x;
        int dy = target.y - start.y;

        if (dx != 0 && dy != 0)
            return;

        int stepX = 0;
        int stepY = 0;

        if (dx > 0)
            stepX = 1;
        else if (dx < 0)
            stepX = -1;

        if (dy > 0)
            stepY = 1;
        else if (dy < 0)
            stepY = -1;

        int distance = Mathf.Abs(dx) + Mathf.Abs(dy);

        for (int i = 1; i <= distance; i++)
        {
            AddEffectTile(
                start.x + stepX * i,
                start.y + stepY * i
            );
        }
    }

    //横向无限的效果范围：横带，纵向共 (2*range+1) 行（含中心格）
    private void CalculateEffectHorizontalInfinite(GridPosition center, int range)
    {
        for (int dy = -range; dy <= range; dy++)
        {
            int y = center.y + dy;

            if (y < 0 || y >= BoardManager.BOARD_HEIGHT)
                continue;

            for (int x = 0; x < BoardManager.BOARD_WIDTH; x++)
            {
                AddEffectTile(x, y);
            }
        }
    }

    //纵向无限的效果范围：竖带，横向共 (2*range+1) 列（含中心格）
    private void CalculateEffectVerticalInfinite(GridPosition center, int range)
    {
        for (int dx = -range; dx <= range; dx++)
        {
            int x = center.x + dx;

            if (x < 0 || x >= BoardManager.BOARD_WIDTH)
                continue;

            for (int y = 0; y < BoardManager.BOARD_HEIGHT; y++)
            {
                AddEffectTile(x, y);
            }
        }
    }

    //十字无限延伸的效果范围（叉形，含中心格）
    private void CalculateEffectCrossInfinite(GridPosition center)
    {
        for (int x = 0; x < BoardManager.BOARD_WIDTH; x++)
        {
            AddEffectTile(x, center.y);
        }

        for (int y = 0; y < BoardManager.BOARD_HEIGHT; y++)
        {
            AddEffectTile(center.x, y);
        }
    }

    //米字形无限延伸的效果范围（十字 + 四条斜线，含中心格）
    private void CalculateEffectCrossWithDiagonalInfinite(GridPosition center)
    {
        CalculateEffectCrossInfinite(center);

        int maxStep = Mathf.Max(
            BoardManager.BOARD_WIDTH,
            BoardManager.BOARD_HEIGHT
        );

        for (int i = 1; i <= maxStep; i++)
        {
            AddEffectTile(center.x + i, center.y + i);
            AddEffectTile(center.x + i, center.y - i);
            AddEffectTile(center.x - i, center.y + i);
            AddEffectTile(center.x - i, center.y - i);
        }
    }

    //清除效果范围
    private void ClearEffectTiles()
    {
        foreach(Tile tile in effectTiles)
        {
            if (tile != null)
            {
                tile.SetVisualState(TileVisualState.Normal);
            }
        }
        effectTiles.Clear();
    }

    //清除目标
    private void ClearTargetTiles()
    {
        foreach(Tile tile in targetTiles)
        {
            if (tile != null)
            {
                tile.SetVisualState(TileVisualState.Normal);
            }
        }

        targetTiles.Clear();
    }

    //清除技能
    public void ClearSkill()
    {
        ClearTargetTiles();

        ClearEffectTiles();

        selectedTargetTile = null;
        selectedCard = null;
        selectedCharacter = null;
    }

    public void ExecuteSkill()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("SkillManager：没有施法角色！");
            return;
        }
        if (selectedCard == null)
        {
            Debug.LogWarning("SkillManager：没有选择技能卡！");
            return;
        }
      


        if (effectTiles.Count == 0)
        {
            Debug.LogWarning("SkillManager：没有技能效果范围！");
            return;
        }

        int timer = selectedCard.GetTimer();
        Debug.Log($"执行技能{selectedCard.GetCardName()}，计时器：{timer}");

        //无计时器
        if (timer <= 0)
        {
            ExecuteImmediateSkill();
            return;
        }

        //有计时器
        if (timerManager == null)
        {
            Debug.LogError("SkillManager：TimerManager为空!");
            return;
        }
        //当前选择的目标格
        Tile targetTile = GetSelectedTargetTile();
        if (targetTile == null)
        {
            Debug.LogWarning("SkillManager：没有记录目标格！");
            return;
        }

        timerManager.AddPendingSkill(selectedCharacter,selectedCard,targetTile);        
        ClearSkill();
    }

    //真正触发伤害的代码
    private void ExecuteImmediateSkill()
    {
        if (selectedCharacter == null)
            return;
        if (selectedCard == null)
            return;
        // Debug.Log($"立即发动技能：{selectedCard.GetCardName()}");

        //直线攻击：只要卡面特殊效果里带「直线攻击」就走这条逻辑
        if (selectedCard.HasEffect(SkillEffectType.DirectAttack))
        {
            if (selectedTargetTile == null)
            {
                Debug.LogWarning("SkillManager：没有选择直线技能目标！");
                return;
            }
            //目标不在直线上时会返回 false：不消耗这张卡，让玩家重新选目标
            if (!ExecuteDirectLineSkill(selectedCharacter, selectedCard, selectedTargetTile))
            {
                return;
            }

            ClearSkill();
            return;
        }
        //普通

        //结算效果范围：敌人吃伤害 + 特殊效果，己方只在带「回复」时被治疗
        ResolveEffectArea(selectedCharacter, selectedCard, false);

        ClearSkill();
    }

    //执行直线攻击技能
    private bool ExecuteDirectLineSkill(Character character, SkillCard card, Tile targetTile)
    {
        if (character == null)
        {
            Debug.LogWarning("SkillManager：直线技能使用者为空！");
            return false;
        }
        if (card == null)
        {
            Debug.LogWarning("SkillManager：直线技能卡为空！");
            return false;
        }
        if (targetTile == null)
        {
            Debug.LogWarning("SkillManager：直线技能目标为空!");
            return false;
        }

        GridPosition start = character.GetGridPosition();
        GridPosition target = targetTile.GetGridPosition();

        int dx = target.x - start.x;
        int dy = target.y - start.y;

        //目标必须落在这 8 个方向之一的直线上：水平 / 垂直 / 45°斜线
        bool horizontal = dy == 0;
        bool vertical = dx == 0;
        bool diagonal = Mathf.Abs(dx) == Mathf.Abs(dy);

        if (!horizontal && !vertical && !diagonal)
        {
            Debug.LogWarning(
                "SkillManager：直线攻击的目标不在直线上！" +
                "只能选水平、垂直或 45° 斜线的格子。"
            );
            return false;
        }

        //8 方向：每个方向都是一个单位步长
        int stepX = 0;
        int stepY = 0;

        if (dx > 0)
            stepX = 1;
        else if (dx < 0)
            stepX = -1;

        if (dy > 0)
            stepY = 1;
        else if (dy < 0)
            stepY = -1;

        //步数取两轴的最大值：斜线时 |dx| == |dy|，
        //用曼哈顿距离(|dx|+|dy|)会算成两倍、走到线外面去
        int distance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

        Debug.Log(
            $"执行直线技能：{card.GetCardName()}，" +
            $"起点={start}，目标={target}，" +
            $"步数={distance}，方向=({stepX},{stepY})"
        );

        //从使用者相邻的第一格开始，沿直线逐格检查
        for (int i = 1; i <= distance; i++)
        {
            GridPosition checkPosition = new GridPosition(
                start.x + stepX * i,
                start.y + stepY * i
            );

            if (!boardManager.IsValidPosition(checkPosition))
                break;

            Tile checkTile = boardManager.GetTile(checkPosition);

            if (checkTile == null)
                break;

            GameObject occupant = checkTile.GetOccupant();

            //没有角色，继续向前
            if (occupant == null)
                continue;

            Character targetCharacter = occupant.GetComponent<Character>();

            if (targetCharacter == null)
                continue;

            //己方角色穿过、继续向前
            if (targetCharacter.GetTeam() == character.GetTeam())
                continue;

            //命中直线上的第一个敌方角色
            Debug.Log(
                $"直线技能命中：{targetCharacter.GetCharacterName()}，" +
                $"伤害={card.GetDamage()}"
            );

            targetCharacter.TakeDamage(card.GetDamage());

            //直线攻击同样要结算卡面特殊效果（击飞 / 取消计时 等）
            ApplySpecialEffects(character, targetCharacter, card);

            //被这个角色挡住，直线后面的角色不受伤害
            break;
        }

        return true;
    }
    // =========================
    // 特殊效果结算
    // =========================

    //结算效果范围：敌方吃伤害 + 特殊效果；己方只在带「回复」效果时被治疗
    private void ResolveEffectArea(Character caster, SkillCard card, bool isPending)
    {
        if (caster == null || card == null)
        {
            return;
        }

        int damage = card.GetDamage();
        int heal = card.GetHealAmount();
        bool hasHeal = card.HasEffect(SkillEffectType.Heal);

        //先收集，避免边遍历边改动
        List<Character> enemies = new List<Character>();
        List<Character> allies = new List<Character>();

        foreach (Tile tile in effectTiles)
        {
            if (tile == null)
            {
                continue;
            }

            GameObject occupant = tile.GetOccupant();

            if (occupant == null)
            {
                continue;
            }

            Character target = occupant.GetComponent<Character>();

            if (target == null)
            {
                continue;
            }

            if (target.GetTeam() == caster.GetTeam())
            {
                //自己人：只有「回复」效果会作用于己方
                if (hasHeal)
                {
                    allies.Add(target);
                }

                continue;
            }

            enemies.Add(target);
        }

        foreach (Character enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            if (damage > 0)
            {
                Debug.Log(
                    $"{(isPending ? "计时技能" : "技能")}命中：" +
                    $"{enemy.GetCharacterName()}，伤害：{damage}"
                );

                enemy.TakeDamage(damage);
            }

            ApplySpecialEffects(caster, enemy, card);
        }

        foreach (Character ally in allies)
        {
            if (ally == null)
            {
                continue;
            }

            ally.Heal(heal);
        }
    }

    //把卡面上的特殊效果作用到一个被命中的角色身上
    //（「直线攻击」的判定在 ExecuteDirectLineSkill 里处理，这里不重复）
    private void ApplySpecialEffects(Character caster, Character target, SkillCard card)
    {
        if (caster == null || target == null || card == null)
        {
            return;
        }

        bool hasKnockBack = card.HasEffect(SkillEffectType.KnockBack);

        //击飞：沿攻击方向强制移动1格
        if (hasKnockBack)
        {
            ApplyKnockBack(caster, target);
        }

        //规则书：击飞同时具有「取消计时攻击」的全部效果，所以两者共用一次取消
        if (card.HasEffect(SkillEffectType.CancelTimer) || hasKnockBack)
        {
            CancelTimerOf(target);
        }
    }

    //击飞：把目标沿「攻击者 -> 目标」的方向强制移动1格
    //前方有棋子或出界时不移动，但「取消计时」照常生效
    private void ApplyKnockBack(Character caster, Character target)
    {
        if (caster == null || target == null)
        {
            return;
        }

        GridPosition from = caster.GetGridPosition();
        GridPosition to = target.GetGridPosition();

        int stepX = to.x == from.x ? 0 : (to.x > from.x ? 1 : -1);
        int stepY = to.y == from.y ? 0 : (to.y > from.y ? 1 : -1);

        if (stepX == 0 && stepY == 0)
        {
            return;
        }

        GridPosition destination = new GridPosition(to.x + stepX, to.y + stepY);

        if (!boardManager.IsValidPosition(destination))
        {
            Debug.Log($"击飞：{target.GetCharacterName()} 前方是棋盘外，位置不变");
            return;
        }

        Tile destinationTile = boardManager.GetTile(destination);

        if (destinationTile == null || destinationTile.GetOccupant() != null)
        {
            Debug.Log($"击飞：{target.GetCharacterName()} 前方有棋子，位置不变");
            return;
        }

        target.MoveTo(destinationTile);

        Debug.Log($"击飞：{target.GetCharacterName()} 被推到 {destination}");
    }

    //取消计时攻击：取下目标的计时计数块 + 弃掉那张卡 + 取回它的目标块
    private void CancelTimerOf(Character target)
    {
        if (target == null)
        {
            return;
        }

        if (timerManager == null)
        {
            timerManager = FindFirstObjectByType<TimerManager>();
        }

        if (timerManager == null)
        {
            Debug.LogWarning("SkillManager：找不到TimerManager，无法取消计时！");
            return;
        }

        int count = timerManager.CancelPendingSkillsOf(target);

        if (count <= 0)
        {
            Debug.Log($"取消计时：{target.GetCharacterName()} 当前没有计时中的技能");
        }
    }

    //计时器结束后发动过技能
    public void ExecutePendingSkill(Character character,SkillCard card,Tile targetTile)
    {
        if (character == null)
        {
            Debug.LogWarning("SkillManager：计时结束技能卡为空。");
            return;
        }
        if (card == null)
        {
            Debug.LogWarning("SkillManager：延时技能卡为空！");
            return;
        }
        if (targetTile == null)
        {
            Debug.LogWarning("SkillManager:计时结束时目标格为空。");
            return;
        }

        Debug.Log($"计时结束，发动技能：{card.GetCardName()},角色：{character.GetCharacterName()}，目标：{targetTile.GetGridPosition()}");
        //直线攻击技能：只要卡面特殊效果里带「直线攻击」就走这条逻辑
        if (card.HasEffect(SkillEffectType.DirectAttack))
        {
            ExecuteDirectLineSkill(character,card,targetTile);
            return;
        }

        //根据当时的目标重新计算效果范围
        selectedCharacter = character;
        selectedCard = card;

        //effectTiles.Clear();
        ClearEffectTiles();

        CalculateEffectArea(targetTile);

        //结算效果范围：敌人吃伤害 + 特殊效果，己方只在带「回复」时被治疗
        ResolveEffectArea(character, card, true);
        // effectTiles.Clear();
        ClearEffectTiles();
    }

    public void TestSkill()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("没有选择角色！");
            return;
        }
        SkillCard card = selectedCharacter.GetDefaultSkillCard();

        if (card == null)
        {
            Debug.LogWarning("角色没有技能卡！");
            return;
        }
        SelectSkillCard(card);
    }



    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (effectTiles.Count > 0)
            {
                ExecuteSkill();
            }
            else
            {
                TestSkill();
            }
            
        }
    }
}
