using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    private void Update()
    {
        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            // Track the amount of time we're wearing the hat
            if (hatObject.activeInHierarchy)
                curHatTime += Time.deltaTime;
        }

        // The host will check if the player has won
        if (PhotonNetwork.IsMasterClient)
        {
            if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Called when the Player object is instantiated
    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player; ;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        // Give the first player the hat
        if (id == 1)
            GameManager.instance.GiveHat(id, true);

        // If this isn't out local player, disable physics as that's
        //   controlled by the user and synce to all other clients
        if(!photonView.IsMine)
            rig.isKinematic = true;
    }

    // Set the player's hat avtive or not
    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        // Did we hit another player?
        if (collision.gameObject.CompareTag("Player"))
        {
            // Do they have the hat?
            if (GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {
                // Can we get the hat?
                if (GameManager.instance.CanGetHat())
                {
                    // Give us the hat
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(curHatTime);

        else if (stream.IsReading)
            curHatTime = (float)stream.ReceiveNext();
    }
}
