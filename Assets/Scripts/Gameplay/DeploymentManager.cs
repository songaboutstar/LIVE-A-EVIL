
using System.Collections.Generic;
using UnityEngine;

public class DeploymentManager : MonoBehaviour
{

    [Header("玩家出生位置")]
    [SerializeField]
    private List<GridPosition> playerStartPositions = new List<GridPosition>();


    [Header("敌方出生位置")]
    [SerializeField]
    private List<GridPosition> enemyStartPositions = new List<GridPosition>();

    private BoardManager boardManager;
    private BattleManager battleManager;

    private List<Character> playerCharacters;
    private List<Character> enemyCharacters;

    private int currentIndex = 0;

    private DeploymentSide currentSide;

    private Character selectedCharacter;

    private readonly List<Tile> availableTiles = new List<Tile>();

    private enum DeploymentSide
    {
        Player,
        Enemy,
        Finished
    }



    // Start is called before the first frame update
    private void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
    }

    //开始角色摆放
    public void StartDeployment(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        this.playerCharacters = playerCharacters;
        this.enemyCharacters = enemyCharacters;

        currentSide = DeploymentSide.Player;
        currentIndex = 0;

        Debug.Log("进入角色摆放阶段：玩家");

        ShowAvailablePositions();
    }

    //玩家点击角色面板后调用
    public void SelectCharacter(Character character)
    {
        if (character == null)
            return;
        if (currentSide == DeploymentSide.Finished)
            return;

        selectedCharacter = character;

        Debug.Log($"选择摆放角色：{character.GetCharacterNameV2()}");
    }

    //当前该摆放的角色（按选秀顺序）
    private Character GetNextUnplacedCharacter()
    {
        List<Character> list =
            (currentSide == DeploymentSide.Player) ? playerCharacters : enemyCharacters;

        if (list == null)
            return null;
        if (currentIndex < 0 || currentIndex >= list.Count)
            return null;

        return list[currentIndex];
    }

    //点击棋盘格
    public void SelectTile(Tile tile)
    {
        if (tile == null)
            return;

        if (selectedCharacter == null)
        {
            // 没有角色面板 UI 时，自动取当前该摆放的那个角色
            selectedCharacter = GetNextUnplacedCharacter();

            if (selectedCharacter == null)
            {
                Debug.Log("请先选择一个角色");
                return;
            }

            Debug.Log($"自动选择待摆放角色：{selectedCharacter.GetCharacterNameV2()}");
        }
        if (!availableTiles.Contains(tile))
        {
            Debug.Log("这个位置不是当前可摆放位置");
            return;
        }

        if (tile.GetOccupant() != null)
        {
            Debug.Log("这个位置已经有角色");
            return;
        }
        PlaceCharacter(selectedCharacter, tile);
    }

    private void PlaceCharacter(Character character,Tile tile)
    {
        character.MoveTo(tile);

        Debug.Log($"{character.GetCharacterNameV2()}摆放到：{tile.GetGridPosition()}");

        currentIndex++;

        selectedCharacter = null;

        if (currentSide == DeploymentSide.Player)
        {
            if (currentIndex >= playerCharacters.Count)
            {
                BeginEnemyDeployment();
            }
        }
        else if (currentSide == DeploymentSide.Enemy)
        {
            if (currentIndex >= enemyCharacters.Count)
            {
                FinishDeployment();
            }
        }
        ShowAvailablePositions();
    }

    private void BeginEnemyDeployment()
    {
        currentSide = DeploymentSide.Enemy;
        currentIndex = 0;

        Debug.Log("玩家方摆放完成");
        Debug.Log("进入敌方角色摆放");

        ShowAvailablePositions();
    }

    private void FinishDeployment()
    {
        currentSide = DeploymentSide.Finished;

        ClearAvailablePositions();
        Debug.Log("双方角色摆放完成");

        if (battleManager != null)
        {
            battleManager.StartBattle();
        }
    }

    private void ShowAvailablePositions()
    {
        ClearAvailablePositions();

        List<GridPosition> positions;

        if (currentSide == DeploymentSide.Player)
        {
            positions = playerStartPositions;
        }
        else if (currentSide == DeploymentSide.Enemy)
        {
            positions = enemyStartPositions;
        }
        else
        {
            return;
        }

        foreach(GridPosition position in positions)
        {
            Tile tile = boardManager.GetTile(position);

            if (tile == null)
                continue;
            availableTiles.Add(tile);

            tile.SetVisualState(TileVisualState.SkillTarget);
        }

    }

    private void ClearAvailablePositions()
    {
        foreach(Tile tile in availableTiles)
        {
            if (tile != null)
            {
                tile.SetVisualState(TileVisualState.Normal);
            }
        }
        availableTiles.Clear();
    }
 
    public bool IsDeploymentTile(Tile tile)
    {
        return availableTiles.Contains(tile);

    }

    // ============ 部署阶段的屏幕提示（不依赖任何 UI 物体） ============
    private void OnGUI()
    {
        if (currentSide == DeploymentSide.Finished)
            return;

        List<Character> list =
            (currentSide == DeploymentSide.Player) ? playerCharacters : enemyCharacters;

        if (list == null || list.Count == 0)
            return;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.normal.textColor = Color.white;

        GUIStyle lineStyle = new GUIStyle(GUI.skin.label);
        lineStyle.fontSize = 17;
        lineStyle.normal.textColor = Color.yellow;

        string sideName = (currentSide == DeploymentSide.Player) ? "玩家方" : "敌方";

        Rect panel = new Rect(10f, 10f, 430f, 134f);
        GUI.Box(panel, GUIContent.none);

        float x = panel.x + 12f;
        float y = panel.y + 8f;

        GUI.Label(
            new Rect(x, y, 410f, 28f),
            $"【{sideName}角色摆放】 {currentIndex} / {list.Count}",
            titleStyle
        );
        y += 30f;

        Character current = selectedCharacter;

        if (current == null && currentIndex < list.Count)
        {
            current = list[currentIndex];
        }

        if (current != null)
        {
            GUI.Label(
                new Rect(x, y, 410f, 24f),
                $"当前要摆：{current.GetCharacterNameV2()}   HP {current.GetMaxHpV2()}",
                lineStyle
            );
        }
        y += 26f;

        GUI.Label(new Rect(x, y, 410f, 24f), "点棋盘上高亮的格子 → 放下该角色", lineStyle);
        y += 26f;

        string placed = "";

        for (int i = 0; i < currentIndex && i < list.Count; i++)
        {
            if (list[i] != null)
            {
                placed += $"{list[i].GetCharacterNameV2()}{list[i].GetGridPosition()}  ";
            }
        }

        if (placed != "")
        {
            GUI.Label(new Rect(x, y, 410f, 24f), "已摆放：" + placed, lineStyle);
        }
    }
  //  public DeploymentSide GetCurrentSide()
    //{
      //  return currentSide;
    //}
}
