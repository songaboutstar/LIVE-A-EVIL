using System.Collections;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private BoardManager boardManager;
    [SerializeField]
    private MovementCard defaultMovementCard;
    [SerializeField]
    private SkillCard[] defaultSkillCards;
   

    private IEnumerator Start()
    {
        // 等待 BoardManager 创建棋盘
        yield return null;

        boardManager =
            FindFirstObjectByType<BoardManager>();

        if (boardManager == null)
        {
            Debug.LogError(
                "没有找到 BoardManager！"
            );

            yield break;
        }

        // ↓↓↓ 角色改由「选秀 + 部署」流程生成（CharacterDraftManager.CreateCharacters），
        //     这里不再硬编码生成，否则会和选秀结果冲突。需要恢复旧测试时取消下面注释即可。
        /*
        CreateCharacter("Player 1", new GridPosition(1, 3), Team.Player);
        CreateCharacter("Player 2", new GridPosition(1, 4), Team.Player);
        CreateCharacter("Enemy 1",  new GridPosition(4, 4), Team.Enemy);
        CreateCharacter("Enemy 2",  new GridPosition(5, 4), Team.Enemy);
        */
    }


    private void CreateCharacter(
        string characterName,
        GridPosition position,
        Team team)
    {
        Tile tile =
            boardManager.GetTile(position);

        if (tile == null)
        {
            Debug.LogError(
                $"找不到 Tile：{position}"
            );

            return;
        }

        if (!tile.IsWalkable())
        {
            Debug.LogError(
                $"Tile {position} 无法放置角色"
            );

            return;
        }


        // 创建角色
        GameObject characterObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Quad
            );

        characterObject.name =
            characterName;

        characterObject.transform.SetParent(
            transform
        );

        characterObject.transform.localScale =
            Vector3.one * 0.65f;


        // 添加 Character
        Character character =
            characterObject.AddComponent<Character>();


        character.SetMovementCard(defaultMovementCard);
        character.SetSkillCards(defaultSkillCards);

        // 通过反射设置测试名称
       character.SetCharacterName(
            characterName
        );


        // 初始化
        character.Initialize(
            position,
            tile,
            team
        );


        Debug.Log(
            $"成功放置角色：" +
            $"{characterName} | " +
            $"阵营：{team} | " +
            $"位置：{position}"
        );
    }


    //private void SetCharacterName(
        //Character character,
      //  string characterName)
    //{
        // 这里暂时不处理
        // Character 会使用默认名称
   // }
}