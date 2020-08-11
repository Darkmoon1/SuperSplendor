using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pers.YHY.SuperSplendor
{
    public class Launcher : MonoBehaviourPunCallbacks
    {

        #region Private Fields
        const string playerNamePrefKey = "PlayerName";

        [SerializeField]
        private InputField nickNameField;
        [SerializeField]
        private Button startButton;
        [SerializeField]
        private GameObject controlPanel;
        [SerializeField]
        private GameObject loadingText;


        private string gameVersion = "0.1";
        [SerializeField]
        private byte maxPlayersPerRoom = 3;
        bool isConnecting;

        #endregion

        #region Mono Callbacks

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            loadingText.SetActive(false);
            string defaultName = string.Empty;
            if (nickNameField != null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    nickNameField.text = defaultName;
                }
            }


            PhotonNetwork.NickName = defaultName;
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion


        #region PUN Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            if (isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
            if (controlPanel)
            {
                controlPanel.SetActive(true);
            }
            if (loadingText)
            {
                loadingText.SetActive(false);
            }
            isConnecting = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom, PublishUserId = true });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            if (maxPlayersPerRoom == 1)
            {
                PhotonNetwork.LoadLevel("Room");
            }

        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
            {
                PhotonNetwork.LoadLevel("Room");
            }
        }

        #endregion

        #region Public functions

        public void OnStartButtonClick()
        {
            controlPanel.SetActive(false);
            loadingText.SetActive(true);
            Connect();

        }

        public void SetPlayerName(string value)
        {
            // #Important
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }



        #endregion

        #region Private Functions
        private void Connect()
        {
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }

        }

        #endregion

    }
}