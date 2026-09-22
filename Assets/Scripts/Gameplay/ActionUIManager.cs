
using UnityEngine;
using UnityEngine.UI;

public class ActionUIManager : MonoBehaviour
{
    private BattleManager battleManager;

    [SerializeField]
    private GameObject actionPanel;
    [SerializeField]
    private Button  moveButton;
    [SerializeField]
    private Button attackButton;
    [SerializeField]
    private Button endActionButton;

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindFirstObjectByType<BattleManager>();
        HideActionPanel();
    }

    //显示行动菜单
    public void ShowActionPanel()
    {
        if (actionPanel != null)
        {
            actionPanel.SetActive(true);
        }
        if (battleManager == null)
        {
            battleManager =
                FindFirstObjectByType<BattleManager>();
        }

        Character character =
            (battleManager != null)
                ? battleManager.GetCurrentCharacter()
                : null;

        if (character != null)
        {
            if (moveButton != null)
            {
                moveButton.interactable = !character.HasMoved();
            }
            if (attackButton != null)
            {
                attackButton.interactable = !character.HasAttacked();
            }
            if (endActionButton != null)
            {
                endActionButton.interactable = true;
            }
        }

        Debug.Log("显示行动菜单");
    }
    //隐藏行动菜单
    public void HideActionPanel()
    {
        if (actionPanel != null)
        {
            actionPanel.SetActive(false);
        }
    }

    //点击攻击
    public void OnAttackButtonClick()
    {
        // 面板交给 BattleManager 在真正进入攻击阶段时隐藏。
        // 若进入失败（没有角色/已经攻击过），面板保持显示，避免卡死。
        if (battleManager != null)
        {
            battleManager.EnterAttackPhase();
        }
    }
    //点击移动
    public void OnMoveButtonClicked()
    {
        if (battleManager != null)
        {
            battleManager.EnterMovePhase();
        }
    }
    //点击结束行动
    public void OnEndActionButtonClicked()
    {
        // 面板由 BattleManager.EndCharacterAction() 统一隐藏
        if (battleManager != null)
        {
            battleManager.EndCharacterAction();
        }
    }
  
}
