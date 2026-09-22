using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private ActionCard card;

    [SerializeField]
    private Button button;

    private HandUIManager handUIManager;

    private void Awake()
    {
        handUIManager =
            FindFirstObjectByType<HandUIManager>();

        button.onClick.AddListener(OnClick);
    }

    public void SetCard(ActionCard card)
    {
        this.card = card;

        gameObject.SetActive(card != null);

        if (card == null)
            return;

        Debug.Log($"œ‘ æø®≈∆£∫{card.GetCardName()}");
    }

    public void ClearCard()
    {
        card = null;
        gameObject.SetActive(false);
    }

    private void OnClick()
    {
        if (card == null)
            return;

        handUIManager.SelectCard(card);
    }

    public ActionCard GetCard()
    {
        return card;
    }
}