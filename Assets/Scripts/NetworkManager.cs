using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    public Text username;
    public float RespawnTimer = 0f;
    public GameObject RespawnCamera;

    private Button multiplayer;

    // Use this for initialization
    void Start()
    {
        PhotonNetwork.player.NickName = PlayerPrefs.GetString("Username", "");
    }

    void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("v0.0.0");
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("Username", PhotonNetwork.player.NickName);
    }

    public void OnButtonClick()
    {
        PhotonNetwork.player.NickName = username.text;
        Connect();
    }

    void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GameObject ui = GameObject.Find("UI");
        if(ui != null)
        {
            ui.SetActive(false);
        }
        GameObject Player = (GameObject)PhotonNetwork.Instantiate("Player", MapGen.playerSpawnPoint, Quaternion.identity, 0);
        Player.GetComponent<PhotonView>().RPC("setPlayerName", PhotonTargets.AllBuffered, PhotonNetwork.player.NickName);
        Player.transform.FindChild("RecoilHolder").transform.FindChild("PlayerCamera").gameObject.SetActive(true);
        Player.GetComponentInChildren<PlayerController>().enabled = true;
        Player.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        if(RespawnTimer > 0)
        {
            Debug.Log("UPDATING");
            RespawnTimer -= Time.deltaTime;

            GameObject.Find("RespawnTimer").GetComponent<Text>().text = ((int)RespawnTimer).ToString();

            if(RespawnTimer <= 0)
            {
                RespawnCamera.SetActive(false);
                SpawnPlayer();
            }
        }
    }

}
