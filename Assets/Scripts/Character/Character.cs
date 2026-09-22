using UnityEngine;

public class Character : MonoBehaviour
{
    //基础数据
    [Header("角色信息")]
    [SerializeField]
    private string characterName = "Test Character";
    //serialzefiled private 方便再inspector中方便配置数据，又保证代码层面私有性，外部脚本无法直接访问。
    [SerializeField]
    private Team team= Team.Player;

    [Header("角色数据")]
    [SerializeField]
    private CharacterData characterData;

    //战斗属性
    [Header("战斗属性")]
    [SerializeField]
    private int maxHp = 100;
    private int currentHp;
    [SerializeField]
    private int attack = 20;
    

    [SerializeField]
    private int defense = 10;

    //移动属性
    [Header("移动")]
    [SerializeField]
    private int movementRange = 4;


    //攻击范围
    [Header("攻击")]
    [SerializeField]
    private int attackRange = 1;

    [Header("行动卡")]
    [SerializeField]
    private MovementCard movementCard;

    [SerializeField]
    private SkillCard[] skillCards;


    public int GetAttackRange()
    {
        return attackRange;
    }


    // 当前棋盘坐标
    private GridPosition gridPosition;

    // 当前所在 Tile
    private Tile currentTile;

    // 是否被选中
    private bool isSelected = false;

    // 角色 Renderer,组件
    private Renderer characterRenderer;

    private MovementManager movementManager;
    private BattleManager battleManager;

    private Material characterMaterial;
    private CharacterActionData actionData=new CharacterActionData();

    private readonly Color playerColor = Color.blue;
    // 普通状态颜色
    private readonly Color enemyColor = Color.red;

    // 选中状态颜色
    private readonly Color selectedColor = Color.yellow;

    private void Awake()
    {
       // actionData = new CharacterActionData();改进在声明时候初始化

        characterRenderer = GetComponent<Renderer>();

        if (characterRenderer == null)
        {
            Debug.LogError(
                 $"{gameObject.name}没有Renderer！"
                );
            return;
        }
        //使用URP Unlit Shader
        Shader shader = Shader.Find(
            //"Universal Render Pipeline/Unlit"
            "Unlit/Color"
            );
        if (shader == null)
        {
            Debug.LogError("找不到Universal Render Pipeline/Unlit Shader");
            return;
        }

        //Material material = new Material(shader);
        characterMaterial = new Material(shader);
        characterRenderer.material = characterMaterial;
        SetNormalColor();
        Debug.Log(
            $"{gameObject.name}颜色设置完成，Team={team}"
            );
    }

    /// <summary>
    /// 初始化角色
    /// </summary>
    public void Initialize(
        GridPosition position,
        Tile tile,Team characterTeam)
    {
        gridPosition = position;
        currentTile = tile;
        team = characterTeam;

        // HP 以角色面板（CharacterData）为准
        if (characterData != null)
        {
            maxHp = characterData.GetMaxHp();
        }

        currentHp = maxHp;

        gameObject.name = "TestCharacter";

        // 告诉 Tile，这个格子已经被角色占据
        tile.SetOccupant(gameObject);

        // 设置世界坐标
        UpdateWorldPosition();

        SetSelected(false);
        movementManager = FindFirstObjectByType<MovementManager>();
        battleManager = FindFirstObjectByType<BattleManager>();

        // 设置角色初始颜色
        SetNormalColor();

        Debug.Log(
            $"角色生成：{gameObject.name}|，"+
            $"阵营：{team}|"+
            $"位置：{gridPosition}"
        );
    }

    /// <summary>
    /// 设置角色世界坐标
    /// </summary>
    private void UpdateWorldPosition()
    {
        if (currentTile == null)
        {
            return;
        }
        Vector3 tilePosition = currentTile.transform.position;
        transform.position = tilePosition+new Vector3(
            0f,
            0f,
            -0.1f
        );
    }
    public string GetCharacterName() 
    {
        return characterName;
    }
    public void SetCharacterName(string name)
    {
        characterName = name;
        gameObject.name = name;
    }
    public Team GetTeam()
    {
        return team;
    }
    public int GetMaxHp()
    {
        return maxHp;
    }
    public int GetCurrentHp()
    {
        return currentHp;
    }
    public int GetAttack()
    {
        return attack;
    }

 
    public int GetDefense()
    {
        return defense;
    }
    public int GetMovementRange()
    {
        return movementRange;
    }
    /// <summary>
    /// 获取角色当前棋盘坐标
    /// </summary>
    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    /// <summary>
    /// 获取角色当前所在 Tile
    /// </summary>
    public Tile GetCurrentTile()
    {
        return currentTile;
    }

    public MovementCard GetMovementCard()
    {
        return movementCard;
    }

    public SkillCard[] GetSkillCards()
    {
        return skillCards;
    }

    //方便测试方法
    public SkillCard GetDefaultSkillCard()
    {
        if (skillCards == null || skillCards.Length == 0)
        {
            return null;
        }
        return skillCards[0];
    }


    //为了让在CharacterManager物体上拖动
    public void SetMovementCard(MovementCard card)
    {
        movementCard = card;
    }
    public void SetSkillCards(SkillCard[] cards)
    {
        skillCards = cards;
    }

    /// <summary>
    /// 设置角色选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (characterMaterial == null)
        {
            return;
        }

        //characterRenderer.material.color = isSelected ? selectedColor : normalColor;
        if (isSelected)
        {
            characterMaterial.color = selectedColor;
        }
        else
        {
            SetNormalColor();
        }
    }

    /// <summary>
    /// 设置普通状态颜色
    /// </summary>
    private void SetNormalColor()
    {
        if (characterMaterial == null)
        {
            //characterRenderer.material.color =
            //  normalColor;
            return;
        }
       // Color color;
        if (team==Team.Player)
        {
            characterMaterial.color = playerColor;
        }
        else
        {
            characterMaterial.color = enemyColor;
        }
    }
    ///移动角色到指定Tile
    public void MoveTo(Tile targetTile)
    {
        if (targetTile == null)
        {
            return;
        }
        //清除原Tile角色
        if (currentTile != null)
        {
            currentTile.SetOccupant(null);
        }
        //更新坐标
        gridPosition = targetTile.GetGridPosition();
        //更新当前Tile
        currentTile = targetTile;
        //新Tile设置角色
        targetTile.SetOccupant(gameObject);
        //更新世界坐标
        UpdateWorldPosition();
        Debug.Log($"角色移动到:{gridPosition}");

    }

    //受伤

    public void TakeDamage(int damage)
    {
        //保证damage不小于0，比较两个数返回最大的
        damage = Mathf.Max(0, damage);
        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);
        Debug.Log($"{characterName}受到{damage}点伤害，"+
                  $"当前HP：{currentHp}/{maxHp}");
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public string GetCharacterNameV2()
    {
        return characterData != null ? characterData.GetCharacterName() : "Unknow Character";
    }

    public int GetMaxHpV2()
    {
        return characterData != null ? characterData.GetMaxHp() : 0;
    }

    public CharacterData GetCharacterData()
    {
        return characterData;
    }

    /// <summary>
    /// 设置角色数据（运行时由选秀结果注入）
    /// </summary>
    public void SetCharacterData(CharacterData data)
    {
        characterData = data;

        if (characterData != null)
        {
            maxHp = characterData.GetMaxHp();
        }

        currentHp = maxHp;
    }

    /// <summary>
    /// 部署阶段用：先不上棋盘，等玩家点格子后再 MoveTo 上盘
    /// </summary>
    public void InitializeOffBoard(Team characterTeam)
    {
        team = characterTeam;
        currentTile = null;
        gridPosition = new GridPosition(-1, -1);

        if (characterData != null)
        {
            maxHp = characterData.GetMaxHp();
        }

        currentHp = maxHp;

        movementManager = FindFirstObjectByType<MovementManager>();
        battleManager = FindFirstObjectByType<BattleManager>();

        SetSelected(false);
        SetNormalColor();
    }

    public SkillCard[] GetSkillCardsV2()
    {
        if (characterData == null)
            return null;
        return characterData.GetSkillCards();
    }

    public SkillCard GetSkillCard(int index)
    {
        if (characterData == null)
            return null;
        return characterData.GetSkillCard(index);
    }

    //死亡
    private void Die()
    {
        Debug.Log($"{characterName}已死亡");
        if (currentTile != null)
        {
            currentTile.SetOccupant(null);
        }
        Destroy(gameObject);
    }

    public CharacterActionData GetActionData()
    {
        return actionData;
    }

    public bool HasMoved()
    {
        return actionData.HasMoved;
    }
    public bool HasAttacked()
    {
        return actionData.HasAttacked;
    }
    public void MarkMoved()
    {
        actionData.SetMoved();
    }
    public void MarkAttacked()
    {
        actionData.SetAttacked();
    }
    public void ResetAction()
    {
        actionData.Reset();
    }

    /// <summary>
    /// 鼠标点击角色
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log(
            $"点击角色：{gameObject.name},阵营：{team}，位置：{gridPosition}"
        );

        if (battleManager == null)
        {
            Debug.LogError("Character:找不到BattleManager!");
            return;
        }

        if (team != Team.Player)
        {
            Debug.Log("当前不是玩家角色！");
            return;
        }

        if (battleManager.GetCurrentState() != BattleState.SelectCharacter)
        {
            Debug.Log("当前不是选择角色阶段！");
            return;
        }

        
        battleManager.SelectCharacter(this);
        SetSelected(true);

        //  SetSelected(true);
        //  if (movementManager != null)
        //{
        //  movementManager.SelectCharacter(this);
        //}
    }
}