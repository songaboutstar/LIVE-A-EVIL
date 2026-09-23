
using System.Collections.Generic;
using UnityEngine;

public class CharacterDraftManager : MonoBehaviour
{

    [Header("牌堆管理")]
    [SerializeField]
    private PlayerCardManager playerCardManager;

    [SerializeField]
    private PlayerCardManager enemyCardManager;


    [Header("全部角色")]
    [SerializeField]
    private List<CharacterData> allCharacters = new List<CharacterData>();

    [Header("每名玩家需要选择的角色数量")]
    [SerializeField]
    private int charactersPerPlayer = 4;

    [Header("角色摆放")]
    [SerializeField]
    private DeploymentManager deploymentManager;

    private List<Character> playerCharacterList = new List<Character>();
    private List<Character> enemyCharacterList = new List<Character>();

    private List<CharacterData> availableCharacters = new List<CharacterData>();
    private List<CharacterData> playerCharacters = new List<CharacterData>();
    private List<CharacterData> enemyCharacters = new List<CharacterData>();

    private CharacterData optionA;
    private CharacterData optionB;

    private int draftRound = 0;
    private bool playerFirst = true;
    private bool waitingForPlayerChoice = false;

    // Start is called before the first frame update
    private void Start()
    {
        StartDraft();
    }

    public void StartDraft()
    {
        availableCharacters.Clear();
        playerCharacters.Clear();
        enemyCharacters.Clear();

        availableCharacters.AddRange(allCharacters);

        draftRound = 0;
        Debug.Log("==========角色选择开始============");
        Debug.Log($"角色池数量：{availableCharacters.Count}");

        DrawTwoCharacters();
    }

    private void DrawTwoCharacters()
    {
        if (draftRound >= charactersPerPlayer)
        {
            FinishDraft();
            return;
        }

        if (availableCharacters.Count < 2)
        {
            Debug.LogError("角色池中没有足够角色！");
            return;
        }
        int indexA = Random.Range(0, availableCharacters.Count);

        optionA = availableCharacters[indexA];
        availableCharacters.RemoveAt(indexA);

        int indexB = Random.Range(0, availableCharacters.Count);

        optionB = availableCharacters[indexB];
        availableCharacters.RemoveAt(indexB);

        draftRound++;
        waitingForPlayerChoice = true;



        Debug.Log($"第{draftRound}次选择：A={optionA.GetCharacterName()},B={optionB.GetCharacterName()}");
        Debug.Log("等待先手玩家选择");

    }

    public void PlayerChoose(int option)
    {
        if (!waitingForPlayerChoice)
        {
            Debug.LogWarning("当前不是角色选择阶段！");
            return;
        }
        
        if (optionA == null || optionB == null)
        {
            Debug.LogWarning("当前没有可选择角色！");
            return;
        }

        CharacterData playerChoice;
        CharacterData enemyChoice;

        if (option == 0)
        {
            playerChoice = optionA;
            enemyChoice = optionB;
        }
        else
        {
            playerChoice = optionB;
            enemyChoice = optionA;
        }
        playerCharacters.Add(playerChoice);
        enemyCharacters.Add(enemyChoice);

        Debug.Log($"Player选择：{playerChoice.GetCharacterName()}");
        Debug.Log($"Enemy选择：{enemyChoice.GetCharacterName()}");

        //当前这一轮选择结束
        waitingForPlayerChoice = false;

        optionA = null;
        optionB = null;
        //四轮选择完成
        if (draftRound >= charactersPerPlayer)
        {
            FinishDraft();
            return;
        }
        //下一轮
        DrawTwoCharacters();
    }

    private void FinishDraft()
    {
        Debug.Log("=============角色选择完成==============");
        Debug.Log($"Player角色数量：{playerCharacters.Count}");
        Debug.Log($"Enemy角色数量：{enemyCharacters.Count}");

        foreach(CharacterData character in playerCharacters)
        {
            Debug.Log($"Player角色：{character.GetCharacterName()}");
        }

        foreach(CharacterData character in enemyCharacters)
        {
            Debug.Log($"Enemy角色：{character.GetCharacterName()}");
        }

        //初始化双发牌堆
        if (playerCardManager != null)
        {
            playerCardManager.Initialize(playerCharacters);
        }
        else
        {
            Debug.LogError("没有找到PlayerCardManager!");
        }

        if (enemyCardManager != null)
        {
            enemyCardManager.Initialize(enemyCharacters);
        }
        else
        {
            Debug.LogError("没有找到EnemyCardManager!");
        }

        Debug.Log("==============双方初始手牌生成完成================");

        // ============ 选秀结束 → 生成棋盘角色 → 进入部署阶段 ============
        playerCharacterList = CreateCharacters(playerCharacters, Team.Player);
        enemyCharacterList = CreateCharacters(enemyCharacters, Team.Enemy);

        if (deploymentManager != null)
        {
            deploymentManager.StartDeployment(playerCharacterList, enemyCharacterList);
        }
        else
        {
            Debug.LogError("没有找到DeploymentManager，无法进入部署阶段！");
        }
    }

    /// <summary>
    /// 按选秀结果创建棋盘上的角色实例（部署前先不上盘）
    /// </summary>
    private List<Character> CreateCharacters(List<CharacterData> datas, Team team)
    {
        List<Character> result = new List<Character>();

        if (datas == null)
        {
            return result;
        }

        foreach (CharacterData data in datas)
        {
            if (data == null)
            {
                continue;
            }

            GameObject characterObject =
                GameObject.CreatePrimitive(PrimitiveType.Quad);

            characterObject.name = data.GetCharacterName();
            characterObject.transform.SetParent(transform);
            characterObject.transform.localScale = Vector3.one * 0.65f;

            Character character =
                characterObject.AddComponent<Character>();

            character.SetCharacterData(data);
            character.SetSkillCards(data.GetSkillCards());

            //移动卡：所有角色共用一张，卡面「移動」值就是 moveDistance（MovementCard_4 = 4格）
            MovementCard movementCard = GetDefaultMovementCard();

            if (movementCard != null)
            {
                character.SetMovementCard(movementCard);
            }

            character.SetCharacterName(data.GetCharacterName());
            character.InitializeOffBoard(team);

            result.Add(character);

            Debug.Log(
                $"生成角色：{data.GetCharacterName()} | " +
                $"阵营：{team} | " +
                $"HP：{data.GetMaxHp()}"
            );
        }

        return result;
    }

    //取场景里 CharacterManager 上挂的默认移动卡（MovementCard_4）
    private MovementCard GetDefaultMovementCard()
    {
        CharacterManager characterManager =
            FindFirstObjectByType<CharacterManager>();

        if (characterManager == null)
        {
            Debug.LogWarning(
                "CharacterDraftManager：找不到CharacterManager，角色将没有移动卡"
            );

            return null;
        }

        MovementCard card = characterManager.GetDefaultMovementCard();

        if (card == null)
        {
            Debug.LogWarning(
                "CharacterManager 的 defaultMovementCard 没有赋值！" +
                "请在场景里挂上 Assets/Cards/MovementCard_4.asset"
            );
        }

        return card;
    }

    public CharacterData GetOptionA()
    {
        return optionA;
    }

    public CharacterData GetOptionB()
    {
        return optionB;
    }

    public List<CharacterData> GetPlayerCharacters()
    {
        return playerCharacters;

    }
    public List<CharacterData> GetEnemyCharacters()
    {
        return enemyCharacters;
    }


    //测试选择
    // =========================
    // 给 UI 用的只读访问 / 文本
    // =========================
    public bool IsWaitingForPlayerChoice()
    {
        return waitingForPlayerChoice;
    }

    public int GetDraftRound()
    {
        return draftRound;
    }

    public string GetDraftTitleText()
    {
        return $"第{draftRound}次选择角色";
    }

    private void OnGUI()
    {
        if (!waitingForPlayerChoice)
            return;
        if (optionA == null || optionB == null)
            return;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 28;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 24;

        GUI.Label(new Rect(Screen.width/2-250,50,500,50),GetDraftTitleText(),titleStyle);

        //左边角色
        if(GUI.Button(new Rect(Screen.width / 2 - 350, 150, 300, 150), optionA.GetCharacterName(), buttonStyle)){
            PlayerChoose(0);
            return;
        }
        //右边角色
        if (GUI.Button(new Rect(Screen.width / 2 + 50, 150, 300, 150), optionB.GetCharacterName(), buttonStyle))
        {
            PlayerChoose(1);
        }
    }
}
