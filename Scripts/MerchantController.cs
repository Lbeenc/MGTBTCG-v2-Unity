using UnityEngine;
using UnityEngine.UI;

public class MerchantController : MonoBehaviour
{
    [Header("Refs")]
    public UIController ui;
    public Character player;
    public GameManager Game;

    [Header("Choice UI")]
    public GameObject choicePanel;
    public Button buyButton;
    public Button restButton;
    public Button leaveButton;

    [Header("Rest Settings")]
    public int restHealAmount = 5;
    public int restCostGold = 0;

    [Header("Shop (One Random Offer)")]
    public ItemData itemDatabase;

    public GameObject offerPanel;
    public Text offerNameText;
    public Text offerDescText;
    public Text offerCostText;
    public Image offerIcon;
    public Button offerPurchaseButton;
    public Button offerCloseButton;

    private ItemDef currentOffer;
    private bool offerRolledThisVisit;
    private bool purchasedThisVisit;

    private bool choicesShown;

    void Start()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        if (offerPanel != null) offerPanel.SetActive(false);

        if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
        if (restButton != null) restButton.onClick.AddListener(OnRest);
        if (leaveButton != null) leaveButton.onClick.AddListener(OnLeave);

        if (offerPurchaseButton != null) offerPurchaseButton.onClick.AddListener(BuyCurrentOffer);
        if (offerCloseButton != null) offerCloseButton.onClick.AddListener(CloseOffer);

        if (ui != null) ui.OnMessageQueueFinished += ShowChoices;

        StartMerchantIntro();
    }

    void OnDestroy()
    {
        if (ui != null)
            ui.OnMessageQueueFinished -= ShowChoices;
    }

    void StartMerchantIntro()
    {
        // NEW VISIT => NEW OFFER
        offerRolledThisVisit = false;
        purchasedThisVisit = false;
        currentOffer = null;

        choicesShown = false;

        if (choicePanel != null) choicePanel.SetActive(false);
        if (offerPanel != null) offerPanel.SetActive(false);

        if (ui != null)
        {
            ui.UpdateGameUI("Merchant: Well well well... a traveler!");
            ui.UpdateGameUI("Merchant: Rest by the fire, or browse my wares.");
            ui.UpdateGameUI("Merchant: What’ll it be? (Press Space)");
        }
    }

    void ShowChoices()
    {
        if (choicesShown) return;
        choicesShown = true;

        if (choicePanel != null) choicePanel.SetActive(true);
    }

    void HideChoices()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
    }

    void OnBuy()
    {
        HideChoices();

        if (ui != null) ui.UpdateGameUI("Merchant: Take a look!");

        if (offerPanel != null) offerPanel.SetActive(true);

        if (!offerRolledThisVisit)
        {
            RollOffer();
            offerRolledThisVisit = true;
            purchasedThisVisit = false;
        }

        RefreshOfferUI();
    }

    private void RollOffer()
    {
        currentOffer = null;

        if (itemDatabase == null || itemDatabase.items == null || itemDatabase.items.Count == 0)
            return;

        // Try to find an offer that is actually obtainable
        for (int tries = 0; tries < 25; tries++)
        {
            var pick = itemDatabase.items[Random.Range(0, itemDatabase.items.Count)];
            if (pick == null) continue;

            int owned = CountOwned(pick.id);
            if (!pick.canStack && owned > 0) continue;
            if (owned >= pick.maxStacks) continue;

            currentOffer = pick;
            return;
        }

        // Fallback: any item
        currentOffer = itemDatabase.items[Random.Range(0, itemDatabase.items.Count)];
    }

    private int CountOwned(string id)
    {
        int count = 0;
        if (PlayerCreationData.Traits == null) return 0;

        for (int i = 0; i < PlayerCreationData.Traits.Count; i++)
        {
            if (string.Equals(PlayerCreationData.Traits[i], id, System.StringComparison.OrdinalIgnoreCase))
                count++;
        }

        return count;
    }

    private void RefreshOfferUI()
    {
        if (offerNameText != null)
            offerNameText.text = currentOffer != null ? currentOffer.displayName : "No offer";

        if (offerDescText != null)
            offerDescText.text = currentOffer != null ? currentOffer.description : "";

        if (offerCostText != null)
            offerCostText.text = currentOffer != null ? $"Cost: {currentOffer.cost}" : "";

        if (offerIcon != null)
        {
            offerIcon.sprite = currentOffer != null ? currentOffer.icon : null;
            offerIcon.enabled = (currentOffer != null && currentOffer.icon != null);
        }

        if (offerPurchaseButton != null)
            offerPurchaseButton.interactable = CanBuyCurrentOffer();
    }

    private bool CanBuyCurrentOffer()
    {
        if (currentOffer == null) return false;
        if (purchasedThisVisit) return false;
        if (player == null) return false;

        int owned = CountOwned(currentOffer.id);
        if (!currentOffer.canStack && owned > 0) return false;
        if (owned >= currentOffer.maxStacks) return false;

        if (player.char_Gold < currentOffer.cost) return false;

        return true;
    }

    public void BuyCurrentOffer()
    {
        if (!CanBuyCurrentOffer())
        {
            if (ui != null) ui.UpdateGameUI("Merchant: You can't buy that.");
            RefreshOfferUI();
            return;
        }

        // Pay
        player.char_Gold -= currentOffer.cost;
        PlayerCreationData.Gold = player.char_Gold;

        // Apply passive immediately (make sure Character has ApplyItemEffects(ItemDef))
        player.ApplyItemEffects(currentOffer);

        // Persist ownership (your save/load already stores Traits)
        PlayerCreationData.Traits.Add(currentOffer.id);

        purchasedThisVisit = true;

        if (ui != null) ui.UpdateGameUI($"You bought: {currentOffer.displayName}!");
        RefreshOfferUI();
    }

    public void CloseOffer()
    {
        if (offerPanel != null) offerPanel.SetActive(false);

        // return to merchant choices after messages clear
        choicesShown = false;
        if (ui != null) ui.UpdateGameUI("Merchant: Anything else? (Press Space)");
    }

    void OnRest()
    {
        HideChoices();

        if (player == null)
        {
            if (ui != null) ui.UpdateGameUI("Merchant: ...Where did you go?");
            choicesShown = false;
            return;
        }

        if (restCostGold > 0 && player.char_Gold < restCostGold)
        {
            if (ui != null)
            {
                ui.UpdateGameUI("Merchant: You don't have enough gold to rest.");
                ui.UpdateGameUI("Merchant: Anything else? (Press Space)");
            }
            choicesShown = false;
            return;
        }

        if (restCostGold > 0)
        {
            player.char_Gold -= restCostGold;
            PlayerCreationData.Gold = player.char_Gold;
        }

        player.char_current_Health = Mathf.Min(player.char_max_Health, player.char_current_Health + restHealAmount);

        if (ui != null)
        {
            ui.UpdateGameUI("You rest by the fire...");
            ui.UpdateGameUI("HP restored. (Press Space)");
        }

        choicesShown = false;
    }

    void OnLeave()
    {
        HideChoices();

        if (ui != null) ui.UpdateGameUI("Merchant: Safe travels.");

        if (Game != null) Game.ReturnFromMerchant();
    }
}