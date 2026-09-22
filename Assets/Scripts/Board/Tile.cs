using UnityEngine;


//棋盘上一个格子，
//每个Tile对应一个GridPosition,

public class Tile: MonoBehaviour
{
    //基础数据

    //这个格子的棋盘坐标
    private GridPosition gridPosition;

    //是否可以进入
    private bool walkable = true;

    //当前占据这个格子的角色
    private GameObject occupant;

    //Renderer

    private Renderer tileRenderer;
    private Material tileMaterial;

    //Tile视觉状态

    private TileVisualState visualState = TileVisualState.Normal;

    //颜色
    //普通状态
    private readonly Color normalColor = Color.white;
    //鼠标悬停
    private readonly Color hoverColor = Color.yellow;
    //选中
    private readonly Color selectedColor = Color.green;
    //可移动
    private readonly Color movableColor = Color.cyan;
    //可攻击
    private readonly Color attackableColor = Color.red;
    // 攻击范围内（该格没有敌人）
    private readonly Color attackRangeColor = new Color(1f, 0.6f, 0f, 1f);

    private readonly Color effectAreaColor = new Color(1f, 0.5f, 0f);

    private readonly Color skillTargetColor = Color.blue;


    //管理器
    private TileSelector tileSelector;
    private MovementManager movementManager;
    private AttackManager attackManager;
    private SkillManager skillManager;


    private DeploymentManager deploymentManager;


    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();

        if (tileRenderer != null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError(
                        $"{gameObject.name}找不到 Sprites/Default Shader!"
                    );
                return;
            }

            tileMaterial = new Material(shader);
            tileRenderer.material = tileMaterial;

            //初始状态
            SetVisualState(TileVisualState.Normal);

        }
        else
        {
            Debug.LogError(
                $"{gameObject.name}没有Renderer!"
                );
        }
    }

    //初始化
    //初始化棋盘格
    public void Initialize(GridPosition position)
    {
        gridPosition = position;

        walkable = true;
        occupant = null;
        //初始化视觉状态
        SetVisualState(TileVisualState.Normal);
        gameObject.name = $"Tile_{position.x}_{position.y}";

        //查找棋盘选择器
        tileSelector = FindFirstObjectByType<TileSelector>();
        //查找移动管理器
        movementManager = FindFirstObjectByType<MovementManager>();
        //查找攻击管理器
        attackManager = FindFirstObjectByType<AttackManager>();

        skillManager = FindFirstObjectByType<SkillManager>();

        deploymentManager = FindAnyObjectByType<DeploymentManager>();
    }

    //GridPosition

    //获取棋盘坐标

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    //Walkable
    //判断这个格子是否可以移动进入
    public bool IsWalkable()
    {
        return walkable && occupant == null;
    }
    //设置格子是否可以通行

    public void SetWalkable(bool value)
    {
        walkable = value;

    }
    //Occupant
    //获取当前占据这个格子的对象

    public GameObject GetOccupant()
    {
        return occupant;
    }
    //设置占据这个格子的对象
    public void SetOccupant(GameObject obj)
    {
        occupant = obj;
    }

    //Visual State
    //设置Tile的视觉状态

    public void SetVisualState(TileVisualState state)
    {
        visualState = state;
        UpdateColor();
    }

    //获取Tile当前视觉状态
    public TileVisualState GetVisualState()
    {
        return visualState;
    }
    //根据当前视觉状态更新颜色

    private void UpdateColor()
    {
        if (tileMaterial == null)
        {
            return;
        }
        switch (visualState)
        {
            case TileVisualState.Normal:
                tileMaterial.color = normalColor;
                break;
            case TileVisualState.Movable:
                tileMaterial.color = movableColor;
                break;
            case TileVisualState.Attackable:
                tileMaterial.color = attackableColor;
                break;
            case TileVisualState.AttackRange:
                tileMaterial.color = attackRangeColor;
                break;
            case TileVisualState.Selected:
                tileMaterial.color = selectedColor;
                break;
            case TileVisualState.EffectArea:
                tileMaterial.color = effectAreaColor;
                break;
            case TileVisualState.SkillTarget:
                tileMaterial.color = skillTargetColor;
                break;
        }
    }

    //鼠标
    //鼠标进入Tile
    private void OnMouseEnter()
    {
        //如果当前不是普通状态
        //就不要让鼠标悬停覆盖当前颜色
        if (visualState != TileVisualState.Normal)
        {
            return;
        }
        if (tileMaterial != null)
        {
            tileMaterial.color = hoverColor;
        }
    }

    //鼠标离开Tile
    private void OnMouseExit()
    {
        //如果当前不是普通状态
        //就不要恢复普通颜色
        if (visualState != TileVisualState.Normal)
        {
            return;
        }
        if (tileMaterial != null)
        {
            tileMaterial.color = normalColor;
        }
    }

    //鼠标点击
    //鼠标点击tile
    private void OnMouseDown()
    {
        Debug.Log($"点击棋盘格：{gridPosition}");
        
        if (deploymentManager != null &&
                     deploymentManager.IsDeploymentTile(this))
        {
            deploymentManager.SelectTile(this);
            return;
        }
        //判断是否为移动目标
        if (movementManager != null && movementManager.IsMovableTile(this))
        {
            movementManager.MoveCharacter(this);
            return;
        }

        //判断是否为攻击目标
        if(attackManager!=null && attackManager.IsAttackableTile(this))
        {
            attackManager.Attack(this);
            return;
        }
        //技能目标
        if (skillManager != null && skillManager.IsTargetTile(this))
        {
            skillManager.SelectTarget(this);
            Debug.Log($"选择技能目标：{gridPosition}");
            return;
        }
        //普通Tile选择
        if (tileSelector != null)
        {
            tileSelector.SelectTile(this);
        }
        else
        {
            Debug.LogWarning("没有找到TileSelector！");
        }
    }

}
