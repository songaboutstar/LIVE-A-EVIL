using UnityEngine;

public class HandUIManager : MonoBehaviour
{
    [SerializeField]
    private CardUI[] cardSlots;

    private ActionCard selectedCard;

    public void SetHand(ActionCard[] cards)
    {
        ClearHand();

        if (cards == null)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            if (i >= cardSlots.Length)
                break;

            cardSlots[i].SetCard(cards[i]);
        }
    }

    public void SelectCard(ActionCard card)
    {
        if (card == null)
            return;

        selectedCard = card;

        Debug.Log(
            $"Ñ¡Ôñ¿¨ÅÆ£º{card.GetCardName()}"
        );
    }

    public ActionCard GetSelectedCard()
    {
        return selectedCard;
    }

    public void ClearSelectedCard()
    {
        selectedCard = null;
    }

    private void ClearHand()
    {
        foreach (CardUI slot in cardSlots)
        {
            if (slot != null)
                slot.ClearCard();
        }
    }
}