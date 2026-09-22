
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

    //点击棋盘格
    public void SelectTile(Tile tile)
    {
        if (tile == null)
            return;
        if (selectedCharacter == null)
        {
            Debug.Log("请先选择一个角色");
            return;
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
  //  public DeploymentSide GetCurrentSide()
    //{
      //  return currentSide;
    //}
}
