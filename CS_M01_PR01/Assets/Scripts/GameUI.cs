using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public PlayerUIContainer[] playerContainers;
    public TextMeshProUGUI winText;

    // Instance
    public static GameUI instance;

    private void Awake()
    {
        // Set the instance to this script
        instance = this;
    }

    private void Start()
    {
        InitializePlayerUI();
    }

    void InitializePlayerUI()
    {
        // Loop through all containers
        for (int x = 0; x < playerContainers.Length; ++x)
        {
            PlayerUIContainer container = playerContainers[x];

            // Only enable and modifey UI containers we need
            if (x < PhotonNetwork.PlayerList.Length)
            {
                container.obj.SetActive(true);
                container.nameText.text = PhotonNetwork.PlayerList[x].NickName;
                container.hatTimeSlider.maxValue = GameManager.instance.timeToWin;
            }
            else
                container.obj.SetActive(false);
        }
    }

    private void Update()
    {
        UpdatePlayerUI();
    }

    void UpdatePlayerUI()
    {
        // Loop through all players
        for (int x = 0; x < GameManager.instance.players.Length; ++x)
        {
            if (GameManager.instance.players[x] != null)
                playerContainers[x].hatTimeSlider.value = GameManager.instance.players[x].curHatTime;
        }
    }

    public void SetWinText(string winnerName)
    {
        winText.gameObject.SetActive(true);
        winText.text = winnerName + " wins!";
    }
}

[System.Serializable]
public class PlayerUIContainer
{
    public GameObject obj;
    public TextMeshProUGUI nameText;
    public Slider hatTimeSlider;
}
