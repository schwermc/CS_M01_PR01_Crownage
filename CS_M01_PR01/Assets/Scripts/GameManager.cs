using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false;      // Has the game ended?
    public float timeToWin;             // Time a player needs to hold the hat to win
    public float invincibleDuration;    // How long after a player gets the hat, are they invincible
    private float hatPickupTime;        // The time the hat was picked up by the current holder

    [Header("Players")]
    public string playerPrefabLocation; // Path in Resources folder to the Player prefab
    public Transform[] spawnPoints;     // Array of all available spawn points
    public PlayerController[] players;  // Array of all the players
    public int playerWithHat;           // ID of the player with the hat
    public int playersInGame;           // Number of players in the game

    // instance
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.All);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }

    // Spawns a player and initialized it
    void SpawnPlayer()
    {
        // Instantiate the player across the nertwork
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        // Get the player script
        PlayerController playerScript = playerObj.GetComponent<PlayerController>();

        // Initialize the player
        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerId)
    {
        return players.First(x => x.id == playerId);
    }

    public PlayerController GetPlayer(GameObject playerObject)
    {
        return players.First(x => x.gameObject == playerObject); // page 40 + 38 for gamemanager
    }


    // Called when a player hits the hatted player - giving them the hat
    [PunRPC]
    public void GiveHat(int playerId, bool initialGive)
    {
        // Remove the hat from the currently hatted player
        if (!initialGive)
            GetPlayer(playerWithHat).SetHat(false);

        // Give the hat to the new player
        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickupTime = Time.time;
    }

    // Is the player able to take the hat at this current time?
    public bool CanGetHat()
    {
        if (Time.time > hatPickupTime + invincibleDuration)
            return true;
        else
            return false;
    }

    [PunRPC]
    void WinGame(int playerId)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerId);

        // Set the UI to show who's won
        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        Invoke("GoBackToMenu", 3.0f);
    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}
