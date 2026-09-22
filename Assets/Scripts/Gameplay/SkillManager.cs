
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
                CalculateHorizontalInfinite(center);
                break;
            case RangeType.Vertical:
                CalculateVertical(center, range);
                break;
            case RangeType.VerticalInfinite:
                CalculateVerticalInfinite(center);
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
    private void CalculateHorizontalInfinite(GridPosition center)
    {
        //左边
        for(int x = center.x - 1; x >= 0; x--)
        {
            AddTargetTile(x, center.y);
        }
        //右边
        for(int x = center.x + 1; x < 7; x++)
        {
            AddTargetTile(x, center.y);
        }
    }
    //纵向无限
    private void CalculateVerticalInfinite(GridPosition center)
    {
        //下
        for(int y = center.y - 1; y > -0; y--)
        {
            AddTargetTile(center.x, y);
        }
        //上
        for(int y = center.y + 1; y < 7; y++)
        {
            AddTargetTile(center.x, y);
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

        if (range <= 0)
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
        int damage = selectedCard.GetDamage();
        // Debug.Log($"立即发动技能：{selectedCard.GetCardName()}，伤害：{damage}");

        //DirectLine
        if (selectedCard.GetRangeType() == RangeType.DirectLine)
        {
            if (selectedTargetTile == null)
            {
                Debug.LogWarning("SkillManager：没有选择直线技能目标！");
                return;
            }
            ExecuteDirectLineSkill(selectedCharacter,selectedCard,selectedTargetTile);
            ClearSkill();
            return;
        }
        //普通

        foreach (Tile tile in effectTiles)
        {
            if (tile == null)
                continue;

            GameObject occupant = tile.GetOccupant();

            if (occupant == null)
                continue;

            Character target = occupant.GetComponent<Character>();

            if (target == null)
                continue;
            //暂时不攻击自己人

            if (target.GetTeam() == selectedCharacter.GetTeam())
                continue;
            //没有defense这一属性
            // int damage = Mathf.Max(1, selectedCard.GetDamage() - target.GetDefense());

            Debug.Log(
                $"{selectedCharacter.GetCharacterName()}" +
                $"使用技能攻击" +
                $"{target.GetCharacterName()}," +
                $"造成{damage}点伤害");

            target.TakeDamage(damage);

        }
        ClearSkill();
    }

    //执行直线攻击技能
    private void ExecuteDirectLineSkill(Character character, SkillCard card, Tile targetTile)
    {
        if (character == null)
        {
            Debug.LogWarning("SkillManager：直线技能使用者为空！");
            return;
        }
        if (card == null)
        {
            Debug.LogWarning("SkillManager：直线技能卡为空！");
            return;
        }
        if (targetTile == null)
        {
            Debug.LogWarning("SkillManager：直线技能目标为空!");
            return;
        }

        GridPosition start = character.GetGridPosition();
        GridPosition target = targetTile.GetGridPosition();

        int dx = target.x - start.x;
        int dy = target.y - start.y;

        //必须是水平或垂直直线
        bool horizontal = dy == 0;
        bool vertical = dx == 0;

        if (!horizontal && !vertical)
        {
            Debug.LogWarning("SkillManager:DirectLine的目标不是水平或垂直直线！");
            return;
        }

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

        Debug.Log($"执行直线技能：{card.GetCardName()},起点={start}，目标={target},距离={distance}");

        //从使用者相邻的第一格开始检查
        for(int i = 1; i <= distance; i++)
        {
            GridPosition checkPosition = new GridPosition(start.x+stepX*i,start.y+stepY*i);

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

            //自己的角色不受到自己的技能攻击
            if(targetCharacter.GetTeam()==character.GetTeam())
            {
                continue;
            }
            //找到第一个对方角色
            Debug.Log($"直线技能命中：{targetCharacter.GetCharacterName()},伤害={card.GetDamage()}");

            targetCharacter.TakeDamage(card.GetDamage());
            //直线攻击被该角色阶段
            break;
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
        //直线攻击技能
        if (card.GetRangeType() == RangeType.DirectLine)
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
        int damage = card.GetDamage();

        foreach (Tile tile in effectTiles)
        {
            if (tile == null)
                continue;
            GameObject occupant = tile.GetOccupant();

            if (occupant == null)
                continue;
            Character target = occupant.GetComponent<Character>();

            if (target == null)
                continue;
            if (target.GetTeam() == character.GetTeam())
            {
                continue;
            }

            Debug.Log($"计时技能命中：{target.GetCharacterName()},伤害：{damage}");

            target.TakeDamage(damage);
        }
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
