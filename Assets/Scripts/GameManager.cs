using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Pers.YHY.SuperSplendor.Card;
using static Pers.YHY.SuperSplendor.PlayerController;

namespace Pers.YHY.SuperSplendor
{
    public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region Public Fields
        public enum GameStateEnum
        {
            GameInit = 0,
            RoundPrepare,
            RoundRun,
            RoundEnd,
            GameOver
        }
        public GameStateEnum currentGameState;

        public string currentPlayerId;

        public PlayerController currentPlayerController;
        #endregion

        public TextMeshProUGUI gameWin;

        #region Private Fields



        [SerializeField]
        GameObject playerPosition;


        [SerializeField]
        GameObject playerPrefab;

        [SerializeField]
        GameObject[] noblePos;

        [SerializeField]
        UIManager uiManager;

        [SerializeField]
        CardsManager cardsManager;

        [SerializeField]
        public CoinsManager coinsManager;

        Dictionary<string, PlayerController> playersControllers;

        [SerializeField]
        string[] playerIds;
        [SerializeField]
        int playerIdIndex;
        [SerializeField]
        int localPlayerIdIndex;

        GameObject[] playerObjects;

        string syncLog;

        PlayerController localPlayerController;

        [SerializeField]
        int masterRound;

        int roomPlayers;

        List<GameObject> noblesList;
        #endregion

        #region Mono Callbacks
        private void Awake()
        {
            roomPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            noblesList = new List<GameObject>();
            masterRound = 0;
            playersControllers = new Dictionary<string, PlayerController>();

            playerIds = new string[roomPlayers];
            for (int i = 0; i < roomPlayers; i++)
            {
                playerIds[i] = "";
            }
            gameWin.gameObject.SetActive(false);


        }

        // Start is called before the first frame update
        void Start()
        {

            InitDesk();

            InitLocalPlayer();

            if (PhotonNetwork.IsMasterClient)
            {
                currentGameState = GameStateEnum.GameInit;
                currentPlayerId = PhotonNetwork.LocalPlayer.UserId;
                masterRound++;
            }


            #region Test part

            #endregion

        }

        private void FixedUpdate()
        {
            #region Game state machine
            if (currentGameState == GameStateEnum.GameInit)
            {
                if (localPlayerController.localRound >= masterRound)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (CheckOutSync())
                        {
                            masterRound++;
                            currentGameState = GameStateEnum.RoundPrepare;
                        }
                    }
                    return;
                }

                var playerList = PhotonNetwork.PlayerList;
                playerObjects = GameObject.FindGameObjectsWithTag("Player");
                if (playerObjects == null || playerObjects.Length != roomPlayers || playerList.Length != roomPlayers)
                {
                    return;
                }

                foreach (var player in playerList)
                {
                    if (string.IsNullOrEmpty(player.UserId))
                        return;
                }


                for (int i = 0; i < roomPlayers; i++)
                {
                    playerObjects[i].transform.SetParent(playerPosition.transform);
                    var playerController = playerObjects[i].GetComponent<PlayerController>();
                    Debug.Log(playerController.localPlayerId);
                    playersControllers.Add(playerController.localPlayerId, playerController);

                    playerIds[i] = playerList[i].UserId;

                    if (currentPlayerId == playerIds[i])
                    {
                        playerIdIndex = i;
                    }
                    if (localPlayerController.localPlayerId == playerIds[i])
                    {
                        localPlayerIdIndex = i;
                    }
                }

                for (int i = localPlayerIdIndex, k = 0; k < roomPlayers; i++, k++)
                {
                    if (i >= roomPlayers)
                    {
                        i = 0;
                    }
                    uiManager.InitUI(playersControllers[playerIds[i]].nickName, playerIds[i], k);

                }
                uiManager.InitUIManager(localPlayerController, coinsManager, cardsManager);

                currentPlayerController = playersControllers[currentPlayerId];
                localPlayerController.localRound++;
            }

            else if (currentGameState == GameStateEnum.RoundPrepare)
            {
                if (localPlayerController.localRound >= masterRound)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (CheckOutSync())
                        {
                            masterRound++;
                            currentGameState = GameStateEnum.RoundRun;
                        }
                    }
                    return;
                }
                currentPlayerController = playersControllers[currentPlayerId];
                uiManager.RefreshUI();
                localPlayerController.localRound++;

            }

            else if (currentGameState == GameStateEnum.RoundRun)
            {
                if (localPlayerController.localRound >= masterRound)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (CheckOutSync())
                        {
                            masterRound++;
                            currentGameState = GameStateEnum.RoundEnd;
                        }
                    }
                    return;
                }

                if (currentPlayerController.hasFinished)
                {
                    syncLog = currentPlayerController.opLog;
                    localPlayerController.localRound++;
                    return;
                }

            }

            else if (currentGameState == GameStateEnum.RoundEnd)
            {
                if (localPlayerController.localRound >= masterRound)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (CheckOutSync())
                        {
                            if (currentPlayerController.score >= 15)
                            {
                                currentGameState = GameStateEnum.GameOver;
                                return;
                            }
                            currentPlayerId = GetNextPlayerId();
                            masterRound++;
                            currentGameState = GameStateEnum.RoundPrepare;
                        }
                    }
                    return;
                }

                SynchronizeOpLog();

                bool couldGetNoble = false;

                foreach (var noble in noblesList)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (currentPlayerController.splendorsHave[i] < noble.GetComponent<NoBle>().costList[i])
                            break;
                        if (i == 5)
                            couldGetNoble = true;
                    }
                    if (couldGetNoble == true)
                    {
                        uiManager.MoveCardOutScreen(noble);
                        noblesList.Remove(noble);
                        if (currentPlayerId == localPlayerController.localPlayerId)
                        {
                            localPlayerController.score += noble.GetComponent<NoBle>().score;
                            localPlayerController.noblesHaveList.Add(noble);
                        }
                        break;
                    }
                }

                currentPlayerController.EndMyRound();
                localPlayerController.localRound++;

            }
            else if (currentGameState == GameStateEnum.GameOver)
            {
                if(gameWin.gameObject.activeSelf == false)
                {
                    gameWin.text = $"Player {currentPlayerController.nickName} <color=red>WIN!!!</color>";
                    gameWin.gameObject.SetActive(true);
                }
            }
            #endregion

        }

        #endregion

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext((int)currentGameState);
                stream.SendNext(currentPlayerId);
                stream.SendNext(masterRound);
                stream.SendNext(playerIdIndex);
            }
            if (stream.IsReading)
            {
                currentGameState = (GameStateEnum)stream.ReceiveNext();
                currentPlayerId = (string)stream.ReceiveNext();
                masterRound = (int)stream.ReceiveNext();
                playerIdIndex = (int)stream.ReceiveNext();
            }
        }

        #region Private Functions
        private void InitDesk()
        {

            cardsManager.CreateCards(0);
            cardsManager.CreateCards(1);
            cardsManager.CreateCards(2);

            cardsManager.InitBattleCards();

            coinsManager.CreateCoins();
            int[][] c = new int[5][];

            int[] c1 = { 2, 0, 0, 0, 2, 2 };
            int[] c2 = { 0, 0, 2, 2, 2, 0 };
            int[] c3 = { 2, 0, 2, 2, 0, 0 };
            int[] c4 = { 0, 0, 3, 3, 0, 0 };
            int[] c5 = { 0, 0, 0, 0, 3, 3 };

            c[0] = c4;
            c[1] = c2;
            c[2] = c3;
            c[3] = c1;
            c[4] = c5;

            for (int i = 0; i < 5; i++)
            {
                var prefab = Resources.Load<GameObject>($"noble {i}");
                var noble = Instantiate(prefab, noblePos[i].transform.position, Quaternion.Euler(new Vector3(0, 180, 0)), noblePos[i].transform);
                noble.GetComponent<NoBle>().Init(c[i]);
                noblesList.Add(noble);
            }

        }



        private void InitLocalPlayer()
        {
            GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
            localPlayerController = localPlayer.GetComponent<PlayerController>();
            localPlayerController.InitController(this);
        }


        private void SynchronizeOpLog()
        {
            if (string.IsNullOrEmpty(syncLog))
                return;
            string[] logPramas = syncLog.Split('-');
            var operate = Enum.Parse(typeof(OprateEnum), logPramas[0]);
            switch (operate)
            {
                case OprateEnum.BuyACard:
                    var cardId = logPramas[1];
                    var level = logPramas[2];
                    var ordered = bool.Parse(logPramas[3]);

                    List<int> more = new List<int>();
                    for (int i = 4; i < logPramas.Length; i++)
                    {
                        more.Add(int.Parse(logPramas[i]));
                    }

                    if (ordered == true)
                    {
                        uiManager.RefreshUI();
                        coinsManager.PushCoins(more.ToArray());
                        break;
                    }

                    GameObject card = cardsManager.PopACard(cardId, level);
                    GameObject posObj = card.transform.parent.gameObject;
                    int pos = int.Parse(posObj.name.Substring(3));
                    Destroy(card);
                    uiManager.RefreshUI();
                    cardsManager.DrawABattleCard(int.Parse(level), pos);
                    coinsManager.PushCoins(more.ToArray());
                    break;
                case OprateEnum.GetCoins:
                    List<int> cost = new List<int>();
                    for (int i = 1; i < logPramas.Length; i++)
                    {
                        cost.Add(int.Parse(logPramas[i]));
                    }
                    coinsManager.PopCoins(cost.ToArray());
                    uiManager.RefreshUI();
                    break;
                case OprateEnum.OrderACard:
                    cardId = logPramas[1];
                    level = logPramas[2];
                    card = cardsManager.PopACard(cardId, level);
                    posObj = card.transform.parent.gameObject;
                    Debug.Log(posObj.name.Substring(3));
                    pos = int.Parse(posObj.name.Substring(3));
                    uiManager.MoveCardOutScreen(card);
                    uiManager.RefreshUI();
                    cardsManager.DrawABattleCard(int.Parse(level), pos);
                    int[] coinCost = { 0, 1, 0, 0, 0, 0 };
                    coinsManager.PopCoins(coinCost);
                    break;
                case OprateEnum.Other:
                    break;
            }

            syncLog = "";


        }



        private bool CheckOutSync()
        {
            foreach (var controller in playersControllers)
            {
                if (controller.Value.localRound < masterRound)
                {
                    //not finish synchronization
                    return false;
                }
            }
            return true;
        }

        private string GetNextPlayerId()
        {
            playerIdIndex++;
            if (playerIdIndex >= roomPlayers)
            {
                playerIdIndex = 0;
            }
            return playerIds[playerIdIndex];
        }

        #endregion

        #region Public Functions


        #endregion
    }

}