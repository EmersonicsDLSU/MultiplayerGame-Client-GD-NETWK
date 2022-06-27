using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // singleton instance
    public static UIManager instance;
    // panel UI object
    public GameObject startMenu;
    // panel inGame object
    public GameObject inGamePanel;
    // panel playersInfo
    public GameObject playersInfoPanel;
    // panel playerInfo prefab
    public GameObject playerInfoPanelPrefab;
    // input field UI object
    public InputField usernameField;
    // Timer UI object
    public Text timerText;
    // winner panel
    public GameObject winnerPanel;
    public Text conditionText;
    public Text winnerName;
    public Button ContinueButton;
    // wait Panel
    public GameObject waitPanel;
    // playerInfoPanel list
    [HideInInspector] public List<GameObject> playerInfoList = new List<GameObject>();
    // singleton process    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError($"Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        // play menuSound bgm
        AudioManager.Instance.Play(SoundCode.MAIN_MENU);
        // assign event to continueButton
        ContinueButton.onClick.AddListener(OnClickContinue);
    }

    private void OnClickContinue()
    {
        winnerPanel.SetActive(false);
        waitPanel.SetActive(true);
        ClientSend.PlayAgain(Client.instance.myId);
    }

    // will be called when the player clicks the 'connect' button
    public void ConnectToServer()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            Debug.LogError($"NO TEXT!");
            return;
        }
        startMenu.SetActive(false);
        usernameField.interactable = false;
        inGamePanel.SetActive(true);
        // transition bgm to battle bgm
        AudioManager.Instance.Stop(SoundCode.MAIN_MENU);
        AudioManager.Instance.Play(SoundCode.INGAME_SOUND);
        // the client will now connect to the server
        Client.instance.ConnectToServer();
    }
    public void ShowWinner()
    {
        int highScore = 0;
        PlayerManager winner = null;
        foreach (var pm in GameManager.players.Values)
        {
            if (pm.killCount > highScore)
            {
                highScore = pm.killCount;
                winner = pm;
            }
            pm.Reset();
        }
        // show win panel
        winnerPanel.SetActive(true);
        // if all scores are the same
        if (highScore == 0 && winner == null)
        {
            var player = GameManager.players[Client.instance.myId];
            // change panel color
            winnerPanel.GetComponent<Image>().color = 
                GameManager.SetPanelColor(player.playerColor);
            // change condition
            conditionText.text = $"BETTER LUCK NEXT TIME";
            // change winner text to player name
            winnerName.text = $"No one wins";
        }
        // if this client is the winner
        else if (winner != null && winner.id == Client.instance.myId)
        {
            // change panel color
            winnerPanel.GetComponent<Image>().color = 
                GameManager.SetPanelColor(winner.playerColor);
            // change condition
            conditionText.text = $"YOU WIN";
            // change winner text to player name
            winnerName.text = winner.username;
            // play win sound
            AudioManager.Instance.Stop(SoundCode.INGAME_SOUND);
            AudioManager.Instance.Play(SoundCode.WIN_SOUND);
        }
        // if this client is the loser
        else if (winner != null && winner.id != Client.instance.myId)
        {
            // change panel color
            winnerPanel.GetComponent<Image>().color = 
                GameManager.SetPanelColor(winner.playerColor);
            // change condition
            conditionText.text = $"BETTER LUCK NEXT TIME";
            // change winner text to player name
            winnerName.text = $"CONGRATULATIONS TO: {winner.username}";
            // play win sound
            AudioManager.Instance.Stop(SoundCode.INGAME_SOUND);
            AudioManager.Instance.Play(SoundCode.LOSE_SOUND);
        }
    }

}
