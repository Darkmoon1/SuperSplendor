using DG.Tweening;
using Photon.Pun;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Pers.YHY.SuperSplendor.Card;

namespace Pers.YHY.SuperSplendor
{
    public class UIManager : MonoBehaviour
    {

        #region private Fields
        [SerializeField]
        GameObject UIRoot;
        [SerializeField]
        GameObject mainPlayerUI;
        [SerializeField]
        GameObject player2UI;
        [SerializeField]
        GameObject player3UI;
        [SerializeField]
        GameObject player4UI;
        [SerializeField]
        GameObject[] UIArray;
        [SerializeField]
        GameObject[] observePos;
        [SerializeField]
        GameObject[] orderCardDisplayPos;
        [SerializeField]
        GameObject[] nobleHaveDisplayPos;

        [SerializeField]
        GameObject orderCardPos;

        [SerializeField]
        GameObject roundOpreationUI;
        [SerializeField]
        GameObject checkUI;
        [SerializeField]
        GameObject orderPlace;

        [SerializeField]
        Button trueButton;
        [SerializeField]
        Button falseButton;

        [SerializeField]
        Button buyACardButton;
        [SerializeField]
        Button getCoinsButton;
        [SerializeField]
        Button orderACardButton;

        [SerializeField]
        GameObject backButton;

        Dictionary<string, GameObject> playersUIDic;

        Dictionary<string, Text> playersNickNameDic;

        Dictionary<string, Text> playerScoreDic;
        Dictionary<string, TextMeshProUGUI> playerSplendorDic;

        PlayerController localPlayerController;

        CoinsManager coinsManager;

        CardsManager cardsManager;

        Stack<Vector3> backPositions;
        Stack<Vector3> backRotations;
        Stack<GameObject> backObjects;

        bool UIEnabled = false;

        bool checkEnabled = false;

        private bool hasInitialized = false;

        const string formatSplendor = "<color=red>{0}|{1}</color> <color=green>{2}|{3}</color> <color=blue>{4}|{5}</color> <color=white>{6}|{7}</color> <color=black>{8}|{9}</color> <color=yellow>{10}</color>";

        int roomPlayers;

        #endregion

        #region MonoCallbacks

        private void Awake()
        {
            UIRoot = this.gameObject;
            roundOpreationUI = UIRoot.transform.Find("RoundOpreationUI").gameObject;
            mainPlayerUI = UIRoot.transform.Find("Player1 UI").gameObject;
            player2UI = UIRoot.transform.Find("Player2 UI").gameObject;
            player3UI = UIRoot.transform.Find("Player3 UI").gameObject;
            player4UI = UIRoot.transform.Find("Player4 UI").gameObject;
            roomPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            GameObject[] tmp = { mainPlayerUI, player2UI, player3UI, player4UI };
            UIArray = tmp;
            backButton = UIRoot.transform.Find("BackButton").gameObject;
            buyACardButton = roundOpreationUI.transform.Find("BuyACard").GetComponent<Button>();
            getCoinsButton = roundOpreationUI.transform.Find("GetCoins").GetComponent<Button>();
            orderACardButton = roundOpreationUI.transform.Find("OrderACard").GetComponent<Button>();
            checkUI = UIRoot.transform.Find("CheckUI").gameObject;
            trueButton = checkUI.transform.Find("True").GetComponent<Button>();
            falseButton = checkUI.transform.Find("False").GetComponent<Button>();
            orderPlace.SetActive(false);

        }

        // Start is called before the first frame update
        void Start()
        {
            backPositions = new Stack<Vector3>();
            backObjects = new Stack<GameObject>();
            backRotations = new Stack<Vector3>();
            playersUIDic = new Dictionary<string, GameObject>();
            playerScoreDic = new Dictionary<string, Text>();
            playerSplendorDic = new Dictionary<string, TextMeshProUGUI>();
            playersNickNameDic = new Dictionary<string, Text>();
            for (int i = roomPlayers; i < 4; i++)
            {
                UIArray[i].SetActive(false);
            }
            DisableOpUI();
            DisableCheckUI();
            DisableBackButton();
        }

        // Update is called once per frame
        void Update()
        {
            if (!hasInitialized)
                return;
            if (localPlayerController.choosedObject == null)
                return;
            ChooseObjectToObserve(localPlayerController.choosedObject, observePos[localPlayerController.hasChosen - 1]);
            if (localPlayerController.operateEnum == PlayerController.OprateEnum.OrderACard)
            {
                ChooseObjectToObserve(coinsManager.PickACoin(SplendorEnum.Yellow), observePos[2]);
            }
            localPlayerController.choosedObject = null;

        }

        private void FixedUpdate()
        {

            if (!hasInitialized || !localPlayerController.isMyturn)
                return;

            if (localPlayerController.localManager.currentGameState != GameManager.GameStateEnum.RoundRun)
                return;


            if (localPlayerController.stateEnum == PlayerController.StateEnum.Other)
            {
                if (localPlayerController.isMyturn && UIEnabled == false && localPlayerController.hasFinished == false)
                {
                    UIEnabled = true;
                    EnableOpUI();
                }
                if (!localPlayerController.isMyturn && UIEnabled == true)
                {
                    UIEnabled = false;
                    DisableOpUI();
                }

                if (checkEnabled == true)
                {
                    DisableCheckUI();
                    checkEnabled = false;
                }

                if (coinsManager.GetCoinsCountBy(SplendorEnum.Yellow) == 0 && orderACardButton.interactable == true)
                {
                    orderACardButton.interactable = false;
                }
                else if (coinsManager.GetCoinsCountBy(SplendorEnum.Yellow) > 0 && orderACardButton.interactable == false)
                {
                    orderACardButton.interactable = true;
                }

                if (getCoinsButton.interactable == true && localPlayerController.coinsHave.Sum() >= 10)
                {
                    getCoinsButton.interactable = false;
                }
                else if (getCoinsButton.interactable == false && localPlayerController.coinsHave.Sum() < 10)
                {
                    getCoinsButton.interactable = true;
                }

            }
            else if (localPlayerController.stateEnum == PlayerController.StateEnum.Choosing)
            {


            }
            else if (localPlayerController.stateEnum == PlayerController.StateEnum.Check)
            {
                if (checkEnabled == false)
                {
                    EnableCheckUI();
                    checkEnabled = true;
                }

                if (localPlayerController.operateEnum == PlayerController.OprateEnum.BuyACard)
                {
                    if (localPlayerController.CanBuyACard(localPlayerController.cardChosen))
                    {
                        trueButton.interactable = true;
                    }
                    else
                    {
                        trueButton.interactable = false;
                    }
                }
                else if (localPlayerController.operateEnum == PlayerController.OprateEnum.GetCoins)
                {
                    if (localPlayerController.isChoosing == false && localPlayerController.CanGetCoins(localPlayerController.coinsChosen))
                    {
                        trueButton.interactable = true;
                    }
                    else
                    {
                        trueButton.interactable = false;
                    }
                }
                else if (localPlayerController.operateEnum == PlayerController.OprateEnum.OrderACard)
                {
                    if (localPlayerController.CanOrderACard())
                    {
                        trueButton.interactable = true;
                    }
                    else
                    {
                        trueButton.interactable = false;
                    }
                }

            }




        }

        #endregion

        #region Public functions

        public void InitUIManager(PlayerController localPlayerController, CoinsManager coinsManager, CardsManager cardsManager)
        {
            this.localPlayerController = localPlayerController;
            this.coinsManager = coinsManager;
            this.cardsManager = cardsManager;
            this.hasInitialized = true;
        }

        public void InitUI(string nickName, string playerId, int playerPos)
        {
            Text nickNameField;
            Text scoreField;
            TextMeshProUGUI splendorField;
            GameObject playerRoot;
            switch (playerPos)
            {
                case 0:
                    playerRoot = mainPlayerUI;
                    break;
                case 1:
                    playerRoot = player2UI;
                    break;
                case 2:
                    playerRoot = player3UI;
                    break;
                case 3:
                    playerRoot = player4UI;
                    break;
                default:
                    return;
            }
            playersUIDic.Add(playerId, playerRoot);
            nickNameField = playerRoot.transform.Find("PlayerName").GetComponent<Text>();
            scoreField = playerRoot.transform.Find("PlayerScore").GetComponent<Text>();
            splendorField = playerRoot.transform.Find("PlayerSplendors").GetComponent<TextMeshProUGUI>();
            playersNickNameDic.Add(playerId, nickNameField);
            playerScoreDic.Add(playerId, scoreField);
            playerSplendorDic.Add(playerId, splendorField);
            nickNameField.text = nickName;
            scoreField.text = "0";
            splendorField.text = string.Format(formatSplendor, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }


        public void RefreshUI()
        {
            var refreshPlayer = localPlayerController.localManager.currentPlayerController;
            var score = playerScoreDic[refreshPlayer.localPlayerId];
            var splendor = playerSplendorDic[refreshPlayer.localPlayerId];
            var coinsHave = refreshPlayer.coinsHave;
            var splendorHave = refreshPlayer.splendorsHave;
            int[] order = { (int)SplendorEnum.Red, (int)SplendorEnum.Green, (int)SplendorEnum.Blue, (int)SplendorEnum.White, (int)SplendorEnum.Black, (int)SplendorEnum.Yellow };
            System.Object[] fillInt = { };
            List<System.Object> tmp = new List<object>();
            for (int i = 0; i < 5; i++)
            {
                tmp.Add(coinsHave[order[i]]);
                tmp.Add(splendorHave[order[i]]);
            }
            tmp.Add(coinsHave[order[5]]);
            fillInt = tmp.ToArray();
            score.text = refreshPlayer.score.ToString();
            splendor.text = string.Format(formatSplendor, fillInt);
        }

        public void OnOrderButton()
        {
            if (orderPlace.activeSelf == false)
            {
                orderPlace.SetActive(true);
                for (int i = 0; i < localPlayerController.orderCardsList.Count; i++)
                {
                    localPlayerController.orderCardsList[i].transform.position = orderCardDisplayPos[i].transform.position;

                }

                for (int i = 0; i < localPlayerController.noblesHaveList.Count; i++)
                {
                    localPlayerController.noblesHaveList[i].transform.position = nobleHaveDisplayPos[i].transform.position;
                }
            }
            else
            {
                orderPlace.SetActive(false);
                for (int i = 0; i < localPlayerController.orderCardsList.Count; i++)
                {
                    MoveCardOutScreen(localPlayerController.orderCardsList[i]);
                }
                for (int i = 0; i < localPlayerController.noblesHaveList.Count; i++)
                {
                    MoveCardOutScreen(localPlayerController.noblesHaveList[i]);
                }
            }
        }

        public void MoveCardOutScreen(GameObject gameObject)
        {
            gameObject.transform.position = orderCardPos.transform.position;
        }

        public void OnGetBack()
        {
            EnableOpUI();
            DisableBackButton();
            MoveObserveObjectBack(0.5f);
            localPlayerController.maxChoose = 0;
            localPlayerController.hasChosen = 0;
            localPlayerController.operateEnum = PlayerController.OprateEnum.Other;
            localPlayerController.stateEnum = PlayerController.StateEnum.Other;
            Array.Clear(localPlayerController.coinsChosen, 0, localPlayerController.coinsChosen.Length);
            localPlayerController.cardChosen = null;
        }

        public void OnGetCoins()
        {
            DisableOpUI();
            EnableBackButton();
            localPlayerController.maxChoose = 3;
            localPlayerController.hasChosen = 0;
            localPlayerController.operateEnum = PlayerController.OprateEnum.GetCoins;
            localPlayerController.stateEnum = PlayerController.StateEnum.Choosing;
        }

        public void OnBuyACard()
        {
            DisableOpUI();
            EnableBackButton();
            localPlayerController.maxChoose = 1;
            localPlayerController.hasChosen = 0;
            localPlayerController.operateEnum = PlayerController.OprateEnum.BuyACard;
            localPlayerController.stateEnum = PlayerController.StateEnum.Choosing;
        }

        public void OnOrderACard()
        {
            DisableOpUI();
            EnableBackButton();
            localPlayerController.maxChoose = 1;
            localPlayerController.hasChosen = 0;
            localPlayerController.operateEnum = PlayerController.OprateEnum.OrderACard;
            localPlayerController.stateEnum = PlayerController.StateEnum.Choosing;
        }

        public void OnCheckTrue()
        {
            DisableOpUI();
            DisableBackButton();
            DisableCheckUI();
            UIEnabled = false;
            checkEnabled = false;
            MoveObserveObjectBack(0f);
            localPlayerController.CheckCorrect();
            localPlayerController.maxChoose = 0;
            localPlayerController.hasChosen = 0;
            localPlayerController.operateEnum = PlayerController.OprateEnum.Other;
            localPlayerController.stateEnum = PlayerController.StateEnum.Other;
            Array.Clear(localPlayerController.coinsChosen, 0, localPlayerController.coinsChosen.Length);
            localPlayerController.cardChosen = null;
        }



        #endregion


        #region Private Functions
        private void EnableOpUI()
        {
            roundOpreationUI.GetComponent<CanvasGroup>().alpha = 1;
            roundOpreationUI.GetComponent<CanvasGroup>().interactable = true;
            roundOpreationUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        private void DisableOpUI()
        {
            roundOpreationUI.GetComponent<CanvasGroup>().alpha = 0;
            roundOpreationUI.GetComponent<CanvasGroup>().interactable = false;
            roundOpreationUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        private void EnableBackButton()
        {
            backButton.GetComponent<CanvasGroup>().alpha = 1;
            backButton.GetComponent<CanvasGroup>().interactable = true;
            backButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        private void DisableBackButton()
        {
            backButton.GetComponent<CanvasGroup>().alpha = 0;
            backButton.GetComponent<CanvasGroup>().interactable = false;
            backButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        private void DisableCheckUI()
        {
            checkUI.GetComponent<CanvasGroup>().alpha = 0;
            checkUI.GetComponent<CanvasGroup>().interactable = false;
            checkUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        private void EnableCheckUI()
        {
            checkUI.GetComponent<CanvasGroup>().alpha = 1;
            checkUI.GetComponent<CanvasGroup>().interactable = true;
            checkUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        private void ChooseObjectToObserve(GameObject gameObject, GameObject targetObject)
        {
            Debug.Log("move to observe");
            //Vector3 _pos = Camera.main.WorldToScreenPoint(targetObject.transform.position);
            //gameObject.transform.localScale = new Vector3(1, 1, 1);
            backPositions.Push(gameObject.transform.position);
            backRotations.Push(gameObject.transform.eulerAngles);
            backObjects.Push(gameObject);
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.transform.DORotate(new Vector3(20, 45, 0) + gameObject.transform.eulerAngles, 0.9f);
            gameObject.transform.DOMove(targetObject.transform.position, 1f).OnComplete(() =>
            {
                if (localPlayerController.hasChosen >= localPlayerController.maxChoose)
                {
                    localPlayerController.stateEnum = PlayerController.StateEnum.Check;
                }
            });


        }

        private void MoveObserveObjectBack(float time)
        {
            var size = backObjects.Count;
            for (int i = 0; i < size; i++)
            {
                var obj = backObjects.Pop();
                var pos = backPositions.Pop();
                var rot = backRotations.Pop();
                obj.transform.DOMove(pos, time);
                obj.transform.DORotate(rot, time);
                obj.GetComponent<Rigidbody>().isKinematic = false;
                obj.GetComponent<BoxCollider>().enabled = true;

            }
        }

        private void ClearObserveObject()
        {
            backObjects.Clear();
            backPositions.Clear();
            backRotations.Clear();
        }

        #endregion
    }

}