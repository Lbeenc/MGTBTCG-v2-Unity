using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem.XR;

public class UIController : MonoBehaviour
{
    public Character player;
    public Enemy enemy;

    [Header("Player UI")]
    public Image playerHPBar;
    public TextMeshProUGUI playerHPText;

    public Image playerXPBar;
    public TextMeshProUGUI playerXPText;

    public TextMeshProUGUI goldText;

    // Stat text
    public TextMeshProUGUI intText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI luckText;
    public TextMeshProUGUI charismaText;
    public TextMeshProUGUI DMGText;

    [Header("Message UI")]
    public Image MessageBox;
    public TextMeshProUGUI MessageText;

    private readonly List<string> messageQueue = new List<string>();
    public bool waitingForInput => messageQueue.Count > 0;

    // NEW: merchant (or anything else) can subscribe to know when dialog is finished
    public event Action OnMessageQueueFinished;

    void Update()
    {
        if (player != null)
            UpdatePlayerUI();

        if (messageQueue.Count > 0 && Input.GetKeyDown(KeyCode.Space))
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            // remove current message
            messageQueue.RemoveAt(0);

            // show next or clear
            if (messageQueue.Count > 0)
            {
                MessageText.text = messageQueue[0];
            }
            else
            {
                MessageText.text = "";
                OnMessageQueueFinished?.Invoke(); // <- IMPORTANT
            }
        }
    }

    public void SaveGame()
    {
        if (player == null)
        {
            Debug.LogWarning("SaveGame called but player is null.");
            return;
        }

        int slot = PlayerCreationData.CurrentSlot;

        PlayerSaveData data = player.ToSaveData();
        SaveSystem.SaveSlot(slot, data);

        UpdateGameUI($"Saved to Slot {slot}!");
    }


    void UpdatePlayerUI()
    {
        // HP
        float hpPercent = (float)player.char_current_Health / player.char_max_Health;
        playerHPBar.fillAmount = hpPercent;
        playerHPText.text = player.char_current_Health + " / " + player.char_max_Health;

        // Gold
        goldText.text = "Gold: " + player.char_Gold;

        // XP
        float XPPercent = (float)player.char_current_XP / player.char_max_XP;
        playerXPBar.fillAmount = XPPercent;

        // Pick ONE style (I’ll keep yours but fix it so it doesn’t overwrite itself)
        playerXPText.text = "XP: " + player.char_current_XP + " / " + player.char_max_XP;

        // Stats
        if (intText != null) intText.text = "INT: " + player.char_INT;
        if (spdText != null) spdText.text = "SPD: " + player.char_SPD;
        if (DMGText != null) DMGText.text = "DMG: " + player.char_Damage;
        if (luckText != null) luckText.text = "Luck: " + player.char_Luck;
        if (charismaText != null) charismaText.text = "CHA: " + player.char_Charisma;
    }

    public void UpdateGameUI(string newMessage)
    {
        messageQueue.Add(newMessage);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // if this is the only message, show immediately
        if (messageQueue.Count == 1)
            MessageText.text = messageQueue[0];
    }

    // Optional helpers (nice for merchant/shop systems)
    public void ClearMessages()
    {
        messageQueue.Clear();
        if (MessageText != null) MessageText.text = "";
    }

    public bool IsQueueEmpty()
    {
        return messageQueue.Count == 0;
    }

    public void SetCurrentEnemy(Enemy newEnemy)
    {
        enemy = newEnemy;
    }
}
