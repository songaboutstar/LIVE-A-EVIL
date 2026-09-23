
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardManager : MonoBehaviour
{

    [Header("玩家名称")]
    [SerializeField]
    private string playerName = "Player";

    [Header("固定移动卡")]
    [SerializeField]
    private bool hasMovementCard = true;

    [Header("手牌UI（在场景里建好 CardUI 槽后拖上来）")]
    [SerializeField]
    private HandUIManager handUIManager;

    [Header("调试：直接在屏幕上显示手牌（按 F1 开关）")]
    [SerializeField]
    private bool showDebugHandUI = true;

    [Header("调试：紧凑布局（贴底、不挡棋盘；按 F2 在紧凑/详细之间切换）")]
    [SerializeField]
    private bool compactHandUI = true;

    [Header("调试：把技能描述放到棋盘右侧空白区（右侧放不下时自动退回手牌上方横条）")]
    [SerializeField]
    private bool infoPanelOnRight = true;

    private BattleManager battleManager;

    //移动卡相关（点移动卡时用）
    private MovementManager movementManager;
    private MovementCard movementCardData;

    //用来把棋盘边缘换算成屏幕坐标（信息栏定位用）
    private Camera mainCamera;

    [Header("Canvas 版手牌面板（P3）：勾上后手牌改用 Canvas 显示，运行时按 F3 也能切")]
    [SerializeField]
    private bool useCanvasUI = false;

    [SerializeField]
    private HandView handView;

    //当前选中的技能卡（高亮 + 信息面板用）
    private SkillCard selectedCard;

    //预览模式：false = 移动范围，true = 技能射程（按 Q 切换）
    private bool previewSkillRange;

    //12张技能卡牌堆
    private List<SkillCard> skillDeck = new List<SkillCard>();

    //当前5张手牌
    private List<SkillCard> skillHand = new List<SkillCard>();

    //当前是否持有移动卡
    private bool movementCardInHand;

    public void Initialize(List<CharacterData> characters)
    {
        skillDeck.Clear();
        skillHand.Clear();

        movementCardInHand = hasMovementCard;
        if (characters == null)
        {
            Debug.LogError($"{playerName}：角色列表为空！");
            return;
        }

        //4个角色，每个角色3张技能卡
        foreach(CharacterData character in characters)
        {
            if (character == null)
                continue;
            SkillCard[] cards = character.GetSkillCards();

            if (cards == null)
                continue;
            foreach(SkillCard card in cards)
            {
                if (card != null)
                {
                    skillDeck.Add(card);
                }
            }
        }
        Debug.Log($"{playerName}牌堆初始化完成：技能卡{skillDeck.Count}张,移动卡{(movementCardInHand ? 1:0)}张");

        ShuffleDeck();
        DrawIntialHand();
    }

    private void ShuffleDeck()
    {
        for(int i = skillDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            SkillCard temp = skillDeck[i];
            skillDeck[i] = skillDeck[randomIndex];
            skillDeck[randomIndex] = temp;
        }
    }

    private void DrawIntialHand()
    {
        const int initialSkillHandCount = 5;

        int drawCount = Mathf.Min(initialSkillHandCount,skillDeck.Count);
        
        for(int i = 0; i < drawCount; i++)
        {
            SkillCard card = skillDeck[0];

            skillDeck.RemoveAt(0);
            skillHand.Add(card);
        }
        Debug.Log($"{playerName}初始手牌，技能卡{skillHand.Count}张，移动卡{(movementCardInHand?1:0)}张");

        PrintHand();

        // 把初始手牌推给手牌 UI（如果场景里已经建好并拖进来）
        if (handUIManager != null)
        {
            handUIManager.SetHand(skillHand.ToArray());
        }
    }

    // =========================
    // 手牌 UI（OnGUI 版，不需要在场景里搭 Canvas）
    // 点技能卡 → 这张卡所属的角色进入选中阶段
    //   · 未移动之前可以自由点不同的卡，切换预览
    //   · 按 Q 在「移动范围」和「技能射程」预览之间切换
    //   · 按 F1 隐藏 / 显示手牌
    //   · 按 F2 在「紧凑布局」（默认，贴底不挡棋盘）和「详细布局」之间切换
    // 对手手牌只显示、不可点
    // =========================
    private void OnGUI()
    {
        HandleHotkeys();

        //Canvas 版（P3）接管后就不再画 OnGUI 手牌（按 F3 随时切回来对比）
        if (useCanvasUI)
        {
            return;
        }

        if (!showDebugHandUI)
        {
            return;
        }

        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        // 还没进入战斗（state == None，即选秀 / 角色摆放阶段）就不显示
        if (battleManager == null ||
            battleManager.GetCurrentState() == BattleState.None)
        {
            return;
        }

        bool isEnemy = IsEnemySide();

        float scale = GetUIScale();

        // ============ 对手手牌（棋盘上方）============
        if (isEnemy)
        {
            DrawHandTipLine(6f, scale);

            //紧凑模式：对手只留一行文字，不占棋盘
            if (compactHandUI)
            {
                return;
            }

            DrawCardRow(32f, scale, 62f);
            return;
        }

        // ============ 自己手牌（贴底）============
        if (compactHandUI)
        {
            float cardHeight = GetCompactCardHeight(scale);
            float cardY = Screen.height - (cardHeight + 6f);

            //技能描述优先放到棋盘右侧的空白区，不挡棋盘和棋子
            if (infoPanelOnRight && TryDrawRightInfoPanel(scale))
            {
                DrawCardRow(cardY, scale, cardHeight);
                return;
            }

            //右侧空间不够（窄窗口）：退回手牌上方的横条
            float infoHeight = GetCompactInfoHeight(scale);
            float infoY = cardY - (infoHeight + 4f);

            DrawCompactInfoPanel(infoY, scale);
            DrawCardRow(infoY + infoHeight + 4f, scale, cardHeight);
        }
        else
        {
            //详细布局：保持原来的位置和尺寸
            float blockY = Screen.height - 210f;

            DrawHandTipLine(blockY, scale);
            DrawCardRow(blockY + 28f, scale, 62f);
            DrawInfoPanel(blockY + 28f + 62f + 6f);
        }
    }

    // =========================
    // 布局尺寸：都按窗口高度缩放，小窗口下不要占满屏幕
    // =========================
    private float GetUIScale()
    {
        return Mathf.Clamp(Screen.height / 900f, 0.75f, 1f);
    }

    //手牌区高度（紧凑）
    private float GetCompactCardHeight(float scale)
    {
        return Mathf.Max(30f, Mathf.Round(46f * scale));
    }

    //紧凑信息条高度（3 行）
    private float GetCompactInfoHeight(float scale)
    {
        return GetInfoLineHeight(scale) * 3f + 4f;
    }

    private float GetInfoLineHeight(float scale)
    {
        return Mathf.Max(12, Mathf.RoundToInt(13f * scale)) + 6f;
    }

    //手牌一行始终塞进屏幕：6 张（移动卡 + 5 技能卡）
    private float GetCardWidth(float scale)
    {
        float gap = 6f * scale;

        return Mathf.Clamp(
            (Screen.width - 20f - gap * 5f) / 6f,
            74f,
            138f
        );
    }

    //手牌提示行（数量 + 快捷键）
    private void DrawHandTipLine(float y, float scale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.Max(13, Mathf.RoundToInt(15f * scale));

        string hint = "　[F1 隐藏 / F2 详细 / Q 切预览]";
        float height = Mathf.Max(22f, 24f * scale);

        GUI.Label(
            new Rect(10f, y, Screen.width - 20f, height),
            GetHandTipText() + hint,
            style
        );
    }

    //手牌一行：移动卡 + 技能卡
    private void DrawCardRow(float y, float scale, float cardHeight)
    {
        bool isEnemy = IsEnemySide();

        float gap = 6f * scale;
        float cardWidth = GetCardWidth(scale);

        GUIStyle cardStyle = new GUIStyle(GUI.skin.button);
        cardStyle.fontSize = Mathf.Max(11, Mathf.RoundToInt(12f * scale));
        cardStyle.alignment = TextAnchor.MiddleCenter;
        cardStyle.wordWrap = true;

        float x = 10f;

        // 移动卡（移动值直接从 MovementCard 资源读，不再写死）
        if (movementCardInHand)
        {
            Rect moveRect = new Rect(x, y, cardWidth, cardHeight);

            bool used = IsMovementCardUsed();
            bool inMoveCardMode = battleManager != null &&
                                  battleManager.IsMovementCardMode();

            string moveLabel = used
                ? $"移动卡\n（已使用）"
                : $"移动卡\n{GetMovementCardDistance()}格";

            GUIStyle moveStyle = new GUIStyle(cardStyle);

            if (used)
            {
                moveStyle.normal.textColor = Color.gray;
            }
            else if (inMoveCardMode)
            {
                //正在用移动卡：高亮
                moveStyle.normal.textColor = Color.yellow;
            }

            if (isEnemy)
            {
                GUI.Box(moveRect, moveLabel, cardStyle);
            }
            else if (GUI.Button(moveRect, moveLabel, moveStyle))
            {
                OnMovementCardClicked();
            }

            x += cardWidth + gap;
        }

        // 技能卡（可点）
        for (int i = 0; i < skillHand.Count; i++)
        {
            SkillCard card = skillHand[i];

            if (card == null)
            {
                continue;
            }

            Rect rect = new Rect(x, y, cardWidth, cardHeight);

            string label =
                $"{card.GetCardName()}\n" +
                $"伤害{card.GetDamage()} · 计时{card.GetTimer()}";

            bool isSelected = (selectedCard == card);

            if (isEnemy)
            {
                GUI.Box(rect, label, cardStyle);
            }
            else
            {
                if (isSelected)
                {
                    GUI.color = Color.yellow;
                }

                if (GUI.Button(rect, label, cardStyle))
                {
                    OnSkillCardClicked(card);
                }

                GUI.color = Color.white;
            }

            x += cardWidth + gap;
        }
    }

    // =========================
    // 棋盘右侧信息栏：把技能描述放到棋盘右边的空白区
    // 右侧宽度是按棋盘实际边缘算的，放不下就返回 false（调用方退回横条）
    // =========================
    private bool TryDrawRightInfoPanel(float scale)
    {
        float boardRight = GetBoardRightScreenX();

        if (boardRight <= 0f)
        {
            return false;
        }

        float panelX = boardRight + 8f;
        float panelW = Screen.width - panelX - 6f;

        //右侧太窄：不硬塞，否则会盖住棋盘
        if (panelW < 170f)
        {
            return false;
        }

        float lineHeight = GetInfoLineHeight(scale);
        float panelY = 34f;
        float panelH = Mathf.Min(
            Screen.height - panelY - 8f,
            lineHeight * 12f + 12f
        );

        Rect panel = new Rect(panelX, panelY, panelW, panelH);
        GUI.Box(panel, GUIContent.none);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.Max(12, Mathf.RoundToInt(13f * scale));
        style.normal.textColor = Color.yellow;
        style.wordWrap = true;

        float innerX = panel.x + 6f;
        float innerW = panel.width - 12f;
        float textY = panel.y + 6f;

        //角色状态（3 行）
        GUI.Label(
            new Rect(innerX, textY, innerW, lineHeight * 3f),
            GetRoleInfoText(),
            style
        );
        textY += lineHeight * 3f;

        //技能卡详情（7 行，长文本自动折行）
        GUI.Label(
            new Rect(innerX, textY, innerW, lineHeight * 7f),
            GetSkillInfoText(),
            style
        );
        textY += lineHeight * 7f;

        //预览状态 + 快捷键（2 行）
        GUI.Label(
            new Rect(innerX, textY, innerW, lineHeight * 2f),
            GetShortPreviewText() + "\n[F1 隐藏 / F2 详细 / Q 切预览]",
            style
        );

        return true;
    }

    //棋盘最右一列的右边缘，在屏幕上的 x 坐标（IMGUI 坐标，左上角为原点）
    private float GetBoardRightScreenX()
    {
        Camera cam = GetMainCamera();

        if (cam == null)
        {
            return 0f;
        }

        //棋盘格子中心在整数坐标 0..BOARD_WIDTH-1，最右格右缘 = 宽 - 0.5
        float worldRight = BoardManager.BOARD_WIDTH - 0.5f;

        Vector3 screenPoint = cam.WorldToScreenPoint(
            new Vector3(worldRight, 0f, 0f)
        );

        //Screen 坐标是左下角为原点，IMGUI 是左上角为原点，这里只要 x 不用翻
        return screenPoint.x;
    }

    private Camera GetMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        return mainCamera;
    }

    //紧凑信息条：贴在手牌上方，只占 3 行，不挡棋盘
    private void DrawCompactInfoPanel(float y, float scale)
    {
        float height = GetCompactInfoHeight(scale);
        float lineHeight = GetInfoLineHeight(scale);

        Rect panel = new Rect(6f, y, Screen.width - 12f, height);
        GUI.Box(panel, GUIContent.none);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.Max(12, Mathf.RoundToInt(13f * scale));
        style.normal.textColor = Color.yellow;
        style.wordWrap = true;

        GUI.Label(
            new Rect(panel.x + 6f, panel.y + 2f, panel.width - 12f, lineHeight),
            GetRoleInfoText() + "　｜　" + GetShortPreviewText(),
            style
        );

        //技能卡详情压成两行以内
        string skillText = GetSkillInfoText().Replace("\n", "　｜　");

        GUI.Label(
            new Rect(
                panel.x + 6f,
                panel.y + 2f + lineHeight,
                panel.width - 12f,
                lineHeight * 2f
            ),
            skillText,
            style
        );
    }

    // F1 隐藏/显示手牌；Q 在「移动范围」和「技能射程」预览之间切换
    private void HandleHotkeys()
    {
        if (Event.current == null || Event.current.type != EventType.KeyDown)
        {
            return;
        }

        if (Event.current.keyCode == KeyCode.F1)
        {
            showDebugHandUI = !showDebugHandUI;

            //Canvas 版手牌面板跟着一起显隐
            if (handView != null)
            {
                handView.SetVisible(showDebugHandUI);
            }

            Debug.Log($"手牌显示：{(showDebugHandUI ? "开" : "关")}");
        }

        //F3：在「OnGUI 版手牌」和「Canvas 版手牌」之间切换，方便对比
        if (Event.current.keyCode == KeyCode.F3)
        {
            useCanvasUI = !useCanvasUI;

            if (useCanvasUI && handView != null)
            {
                showDebugHandUI = true;
                handView.SetVisible(true);
            }

            Debug.Log($"手牌界面：{(useCanvasUI ? "Canvas 版" : "OnGUI 版")}");
        }

        if (Event.current.keyCode == KeyCode.F2)
        {
            compactHandUI = !compactHandUI;

            Debug.Log($"手牌界面：{(compactHandUI ? "紧凑" : "详细")}");
        }

        if (Event.current.keyCode == KeyCode.Q)
        {
            previewSkillRange = !previewSkillRange;

            RefreshPreview();

            Debug.Log($"预览切换：{(previewSkillRange ? "技能射程" : "移动范围")}");
        }
    }

    // 点技能卡：让这张卡对应的角色进入选中阶段
    public void OnSkillCardClicked(SkillCard card)
    {
        if (card == null)
        {
            return;
        }

        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        if (battleManager == null)
        {
            return;
        }

        Character owner = FindOwnerCharacter(card);

        if (owner == null)
        {
            Debug.LogWarning(
                $"找不到「{card.GetCardName()}」对应的角色（不在场或已阵亡）"
            );
            return;
        }

        //已移动过的角色不能再换（判断在 BattleManager 里）
        if (!battleManager.SelectCharacterByCard(owner, card))
        {
            return;
        }

        selectedCard = card;
        previewSkillRange = false;

        Debug.Log(
            $"点技能卡：{card.GetCardName()} → " +
            $"角色 {owner.GetCharacterNameV2()} 进入选中阶段"
        );

        RefreshPreview();
    }

    // 点移动卡：移动卡可以移动任意一个己方角色 → 先点棋子，再点格子
    public void OnMovementCardClicked()
    {
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        if (battleManager == null)
        {
            return;
        }

        MovementManager manager = GetMovementManager();

        if (manager != null && manager.HasUsedMovementCard())
        {
            Debug.Log("移动卡：本轮已经用过了，等下一轮再发");

            return;
        }

        //移动卡不是技能卡：清掉技能卡的选中状态
        selectedCard = null;
        previewSkillRange = false;

        battleManager.EnterMovementCardMode();
    }

    //移动卡上的移动值（从 CharacterManager 挂的 MovementCard 读）
    private int GetMovementCardDistance()
    {
        if (movementCardData == null)
        {
            CharacterManager characterManager =
                FindFirstObjectByType<CharacterManager>();

            if (characterManager != null)
            {
                movementCardData = characterManager.GetDefaultMovementCard();
            }

            if (movementCardData == null)
            {
                Debug.LogWarning(
                    "找不到移动卡数据（CharacterManager 的 defaultMovementCard 为空），" +
                    "暂时按 4 格显示"
                );

                return 4;
            }
        }

        return movementCardData.GetMoveDistance();
    }

    //本轮移动卡是否已经用过
    public bool IsMovementCardUsed()
    {
        MovementManager manager = GetMovementManager();

        return manager != null && manager.HasUsedMovementCard();
    }

    private MovementManager GetMovementManager()
    {
        if (movementManager == null)
        {
            movementManager = FindFirstObjectByType<MovementManager>();
        }

        return movementManager;
    }

    // 按卡面的「所属角色」找到棋盘上对应的角色实例
    private Character FindOwnerCharacter(SkillCard card)
    {
        CharacterData ownerData = card.GetOwnerCharacter();

        if (ownerData == null)
        {
            return null;
        }

        Character[] all = FindObjectsByType<Character>(FindObjectsSortMode.None);

        foreach (Character candidate in all)
        {
            if (candidate == null)
            {
                continue;
            }

            if (candidate.GetCharacterData() == ownerData)
            {
                return candidate;
            }
        }

        return null;
    }

    // 刷新棋盘预览：移动范围 或 技能射程（两者没法同时显示，所以用 Q 切换）
    private void RefreshPreview()
    {
        if (battleManager == null)
        {
            return;
        }

        Character current = battleManager.GetCurrentCharacter();

        if (current == null)
        {
            return;
        }

        MovementManager movementManager = FindFirstObjectByType<MovementManager>();
        SkillManager skillManager = FindFirstObjectByType<SkillManager>();

        //移动卡模式：Q 没有意义，保持显示这张移动卡的移动范围
        if (battleManager.IsMovementCardMode())
        {
            previewSkillRange = false;

            if (movementManager != null)
            {
                movementManager.StartMovementCardMove(current);
            }

            return;
        }

        if (previewSkillRange)
        {
            //技能射程预览：先把移动范围擦掉
            if (movementManager != null)
            {
                movementManager.ClearMovementRange();
            }

            if (skillManager != null && selectedCard != null)
            {
                skillManager.SelectSkillCard(selectedCard);
            }
        }
        else
        {
            //移动范围预览
            if (skillManager != null)
            {
                skillManager.ClearSkillPreview();
            }

            if (movementManager != null)
            {
                if (selectedCard != null)
                {
                    //按这张技能卡的「移動」值重算
                    movementManager.StartSkillCardMove(current, selectedCard);
                }
                else
                {
                    movementManager.SelectCharacter(current);
                }
            }
        }
    }

    // 选中角色 / 技能卡的信息面板
    private void DrawInfoPanel(float y)
    {
        Rect panel = new Rect(10f, y, 900f, 118f);
        GUI.Box(panel, GUIContent.none);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.yellow;

        float textY = panel.y + 6f;

        GUI.Label(new Rect(panel.x + 10f, textY, 880f, 24f), GetRoleInfoText(), style);
        textY += 26f;

        GUI.Label(new Rect(panel.x + 10f, textY, 880f, 48f), GetSkillInfoText(), style);
        textY += 50f;

        GUI.Label(new Rect(panel.x + 10f, textY, 880f, 24f), GetPreviewInfoText(), style);
    }

    // =========================
    // 以下是「只拼字符串、不负责画」的取文本接口
    // IMGUI 和以后的 Canvas UI 都从这里取，保证两边显示一致
    // =========================
    public bool IsEnemySide()
    {
        return playerName != null && playerName.Contains("Enemy");
    }

    //手牌提示行
    public string GetHandTipText()
    {
        if (IsEnemySide())
        {
            return $"{playerName}手牌：技能卡 {skillHand.Count} 张（对手手牌不可点）";
        }

        return
            $"{playerName}手牌：技能卡 {skillHand.Count} 张" +
            $" + 移动卡 {(movementCardInHand ? 1 : 0)} 张" +
            $"{(IsMovementCardUsed() ? "（已使用）" : "")}";
    }

    //当前角色那一行
    public string GetRoleInfoText()
    {
        Character current = battleManager != null
            ? battleManager.GetCurrentCharacter()
            : null;

        if (current == null)
        {
            return "角色：（未选中）点一张技能卡开始";
        }

        return
            $"角色：{current.GetCharacterNameV2()}" +
            $"　HP {current.GetCurrentHp()}/{current.GetMaxHpV2()}" +
            $"　能力低下 {current.GetWeakStacks()} 层" +
            $"　{(current.HasMoved() ? "已移动" : "未移动")}" +
            $"　{(current.HasAttacked() ? "已攻击" : "未攻击")}";
    }

    //选中技能卡的详情（两行）
    public string GetSkillInfoText()
    {
        if (selectedCard == null)
        {
            return "技能卡：（未选中）";
        }

        return
            $"技能卡：{selectedCard.GetCardName()}" +
            $"　移动 {selectedCard.GetMoveRange()} 格" +
            $"　伤害 {selectedCard.GetDamage()}" +
            $"　计时 {selectedCard.GetTimer()} 轮\n" +
            $"射程：{DescribeRangeType(selectedCard.GetRangeType(), selectedCard.GetRange())}" +
            $"　效果范围：{DescribeRangeType(selectedCard.GetEffectRangeType(), selectedCard.GetEffectRange())}" +
            $"　特殊效果：{DescribeEffects(selectedCard)}";
    }

    //Canvas 版手牌是否启用（HandView 用它决定自己显不显示，避免两套同时出现）
    public bool IsCanvasUIEnabled()
    {
        return useCanvasUI;
    }

    //移动卡的移动值（给 Canvas 版 UI 用）
    public int GetMovementCardValue()
    {
        return GetMovementCardDistance();
    }

    //是否正在「移动卡模式」（点了移动卡、还没点角色）
    public bool IsMovementCardModeActive()
    {
        return battleManager != null && battleManager.IsMovementCardMode();
    }

    //预览模式的短文本（紧凑布局用）
    public string GetShortPreviewText()
    {
        return previewSkillRange ? "预览：技能射程" : "预览：移动范围";
    }

    //预览模式那一行
    public string GetPreviewInfoText()
    {
        return previewSkillRange
            ? "当前预览：技能射程　（按 Q 切回移动范围）"
            : "当前预览：移动范围　（按 Q 切到技能射程）";
    }

    // =========================
    // 给 Canvas UI 用的只读访问
    // =========================
    public List<SkillCard> GetSkillHand()
    {
        return skillHand;
    }

    public SkillCard GetSelectedCard()
    {
        return selectedCard;
    }

    public bool IsMovementCardInHand()
    {
        return movementCardInHand;
    }

    public bool IsPreviewSkillRange()
    {
        return previewSkillRange;
    }

    // 射程 / 效果范围的中文说明
    private string DescribeRangeType(RangeType type, int range)
    {
        int size = range * 2 + 1;

        switch (type)
        {
            case RangeType.Single:
                return "单点";
            case RangeType.Cross:
                return $"十字（半径{range}）";
            case RangeType.Horizontal:
                return $"横向 {size} 格";
            case RangeType.Vertical:
                return $"纵向 {size} 格";
            case RangeType.Square:
                return $"{size}×{size} 方形（{size * size}格）";
            case RangeType.CrossWithDiagonal:
                return $"米字形（半径{range}）";
            case RangeType.HorizontalInfinite:
                return $"横向无限 × 纵向{size}行";
            case RangeType.VerticalInfinite:
                return $"纵向无限 × 横向{size}列";
            case RangeType.DirectLine:
                return "直线";
            case RangeType.DiagonalFourAtTwo:
                return "斜向四点";
            case RangeType.CrossInfinite:
                return "十字无限（叉形）";
            case RangeType.CrossWithDiagonalInfinite:
                return "米字形无限";
            case RangeType.Custom:
                return "自定义图案";
            default:
                return type.ToString();
        }
    }

    // 特殊效果的中文说明（一张卡可能有多个）
    private string DescribeEffects(SkillCard card)
    {
        SkillEffectType[] effects = card.GetEffectTypes();

        if (effects == null || effects.Length == 0)
        {
            return "无";
        }

        string text = "";

        for (int i = 0; i < effects.Length; i++)
        {
            if (text != "")
            {
                text += " + ";
            }

            switch (effects[i])
            {
                case SkillEffectType.KnockBack:
                    text += "击飞";
                    break;
                case SkillEffectType.Weak:
                    text += "能力低下";
                    break;
                case SkillEffectType.CancelTimer:
                    text += "取消计时";
                    break;
                case SkillEffectType.Heal:
                    text += $"回复({card.GetHealAmount()})";
                    break;
                case SkillEffectType.DirectAttack:
                    text += "直线攻击";
                    break;
                default:
                    text += "无";
                    break;
            }
        }

        return text;
    }

    private void PrintHand()
    {
        Debug.Log($"=============={playerName}当前手牌=================");

        if (movementCardInHand)
        {
            Debug.Log("手牌：移动卡");
        }

        for(int i = 0; i < skillHand.Count; i++)
        {
            SkillCard card = skillHand[i];

            if (card == null)
                continue;
            Debug.Log($"技能卡{i + 1}：{card.GetCardName()}");
        }
        Debug.Log("===============================================");

    }

    public List<SkillCard> GetSkillDeck()
    {
        return skillDeck;

    }

    public bool HasMovementCard()
    {
        return movementCardInHand;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}
