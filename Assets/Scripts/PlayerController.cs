using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Photon;
using Photon.Pun;
using static Pers.YHY.SuperSplendor.Card;
using Photon.Realtime;
using DG.Tweening;
using System.Numerics;
using System.Linq;

namespace Pers.YHY.SuperSplendor
{
    public class PlayerController : MonoBehaviour, IPunObservable
    {

        #region Private Fields
        public enum OprateEnum
        {
            GetCoins = 0,
            BuyACard,
            OrderACard,
            Other
        }

        public enum StateEnum
        {
            Choosing = 0,
            Check,
            Other
        }

        private enum PickObjType
        {
            Card = 0,
            Coin,
            Noble,
            Other
        }


        private PhotonView view;

        public GameManager localManager;

        private bool hasInitialized = false;

        #region Public Fields
        public string localPlayerId;

        public string nickName;

        public bool isMyturn;

        public bool hasFinished;

        public bool isChoosing;

        public string opLog;

        public int localRound = 0;

        public GameObject choosedObject;


        public OprateEnum operateEnum = OprateEnum.Other;

        public StateEnum stateEnum = StateEnum.Other;

        public int maxChoose = 0;

        public int hasChosen = 0;

        public int score;

        public int[] coinsHave;

        public int[] splendorsHave;

        public int[] coinsChosen;

        int[] moreCoins;

        public Card cardChosen;

        public List<GameObject> orderCardsList;

        public List<GameObject> noblesHaveList;

        const int MAX_ORDER_CARD_COUNT = 3;
        #endregion

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(localPlayerId);
                stream.SendNext(isMyturn);
                stream.SendNext(hasFinished);
                stream.SendNext(hasInitialized);
                stream.SendNext(localRound);
                stream.SendNext(nickName);
                stream.SendNext(score);
                stream.SendNext(opLog);
                stream.SendNext(coinsHave.Clone());
                stream.SendNext(splendorsHave.Clone());
            }
            if (stream.IsReading)
            {
                localPlayerId = (string)stream.ReceiveNext();
                isMyturn = (bool)stream.ReceiveNext();
                hasFinished = (bool)stream.ReceiveNext();
                hasInitialized = (bool)stream.ReceiveNext();
                localRound = (int)stream.ReceiveNext();
                nickName = (string)stream.ReceiveNext();
                score = (int)stream.ReceiveNext();
                opLog = (string)stream.ReceiveNext();
                coinsHave = (int[])stream.ReceiveNext();
                splendorsHave = (int[])stream.ReceiveNext();
            }
        }
        #endregion

        #region Mono Callbacks


        // Start is called before the first frame update
        void Start()
        {
            coinsChosen = new int[6];
            coinsHave = new int[6];
            splendorsHave = new int[6];
            view = GetComponent<PhotonView>();
            isMyturn = false;
            hasFinished = false;
            isChoosing = false;
            orderCardsList = new List<GameObject>();
            noblesHaveList = new List<GameObject>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!view.IsMine || hasInitialized == false)
            {
                return;
            }
            if (!isMyturn || operateEnum == OprateEnum.Other || stateEnum != StateEnum.Choosing)
            {
                return;
            }

            if (isChoosing)
                return;

            if (hasChosen >= maxChoose)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isChoosing = true;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject go = hit.collider.gameObject;    //获得选中物体
                    string goName = go.name;    //获得选中物体的名字，使用hit.transform.name也可以
                    var type = CheckoutPickUp(goName);
                    switch (type)
                    {
                        case PickObjType.Other:
                            isChoosing = false;
                            return;
                        case PickObjType.Noble:
                            isChoosing = false;
                            return;
                        case PickObjType.Card:
                            if (go.GetComponent<Card>().publish == false)
                            {
                                isChoosing = false;
                                return;
                            }
                            if (operateEnum == OprateEnum.GetCoins)
                            {
                                isChoosing = false;
                                return;
                            }
                            if (operateEnum == OprateEnum.OrderACard && go.GetComponent<Card>().isOrdered == true)
                            {
                                isChoosing = false;
                                return;
                            }
                            choosedObject = go;
                            cardChosen = go.GetComponent<Card>();
                            break;
                        case PickObjType.Coin:
                            if (operateEnum != OprateEnum.GetCoins)
                            {
                                isChoosing = false;
                                return;
                            }
                            if (goName.Contains("Yellow"))
                            {
                                isChoosing = false;
                                return;
                            }
                            goName = goName.Substring(9).Replace("(Clone)", "");
                            var coinType = Enum.Parse(typeof(SplendorEnum), goName, true);
                            coinsChosen[(int)coinType]++;
                            choosedObject = go;
                            if (coinsChosen.Sum() + coinsHave.Sum() >= 10)
                            {
                                hasChosen = 3;
                                isChoosing = false;
                                return;
                            }

                            if (coinsChosen[(int)coinType] > 1)
                            {
                                hasChosen++;
                                if (hasChosen >= maxChoose)
                                {
                                    hasChosen--;
                                    choosedObject = null;
                                    isChoosing = false;
                                    return;
                                }
                            }

                            break;
                    }
                    hasChosen++;
                }
                isChoosing = false;

            }
        }

        private void FixedUpdate()
        {
            if (!view.IsMine || hasInitialized == false)
            {
                return;
            }

            if (localManager.currentPlayerId == localPlayerId && localManager.currentGameState == GameManager.GameStateEnum.RoundRun && !isMyturn)
            {
                Debug.Log("Start my turn");
                isMyturn = true;
                hasFinished = false;
            }

        }
        #endregion

        #region Private Functions



        private PickObjType CheckoutPickUp(string objName)
        {
            if (objName.Contains("Card"))
                return PickObjType.Card;
            else if (objName.Contains("Splendor"))
                return PickObjType.Coin;
            else if (objName.Contains("Noble"))
                return PickObjType.Noble;
            else
                return PickObjType.Other;
        }


        private void FinishMyOp()
        {
            hasFinished = true;
        }

        #endregion

        #region Public functions

        public void InitController(GameManager localManager)
        {
            this.localPlayerId = PhotonNetwork.LocalPlayer.UserId;
            this.nickName = PhotonNetwork.LocalPlayer.NickName;
            this.localManager = localManager;
            hasInitialized = true;
        }

        public void CheckCorrect()
        {
            if (stateEnum != StateEnum.Check)
            {
                return;
            }

            if (operateEnum == OprateEnum.BuyACard)
            {
                if (cardChosen == null)
                {
                    Debug.Log("error with cardChoesn is null when buy a card");
                }
                var cost = cardChosen.GetCost();
                moreCoins = (int[])coinsHave.Clone();
                for (int i = 0; i < 6; i++)
                {
                    var left = cost[i] - splendorsHave[i];
                    coinsHave[i] -= left > 0 ? left : 0;
                    if (coinsHave[i] < 0)
                    {
                        coinsHave[(int)SplendorEnum.Yellow] += coinsHave[i];
                        coinsHave[i] = 0;
                    }

                }
                for (int i = 0; i < 6; i++)
                {
                    moreCoins[i] -= coinsHave[i];
                }

                opLog = ProduceOpLog();
                var type = cardChosen.GetSpendorType();
                splendorsHave[(int)type] += 1;
                score += cardChosen.GetScore();
                if (cardChosen.isOrdered == true)
                {
                    score += cardChosen.GetScore();
                    orderCardsList.Remove(cardChosen.gameObject);
                    Destroy(cardChosen.gameObject);
                }

            }
            else if (operateEnum == OprateEnum.GetCoins)
            {
                for (int i = 0; i < 6; i++)
                {
                    coinsHave[i] += coinsChosen[i];
                }
                opLog = ProduceOpLog();
            }
            else if (operateEnum == OprateEnum.OrderACard)
            {
                if (cardChosen == null)
                {
                    Debug.Log("error with cardChoesn is null when order a card");
                }
                coinsHave[(int)SplendorEnum.Yellow] += 1;
                cardChosen.isOrdered = true;
                orderCardsList.Add(cardChosen.gameObject);
                score -= cardChosen.GetScore();
                opLog = ProduceOpLog();
            }
            else
            {
                return;
            }
            FinishMyOp();
        }

        public string ProduceOpLog()
        {
            string log;

            if (operateEnum == OprateEnum.BuyACard)
            {
                var cost = cardChosen.GetCost();
                string[] more = new string[6];
                for (int i = 0; i < 6; i++)
                {
                    more[i] = moreCoins[i].ToString();
                }
                string[] joinString = { operateEnum.ToString(), cardChosen.ID.ToString(), cardChosen.level.ToString(), cardChosen.isOrdered.ToString() };
                log = string.Join("-", joinString.Concat(more).ToArray());
                moreCoins = null;
            }
            else if (operateEnum == OprateEnum.GetCoins)
            {
                string[] joinString = { operateEnum.ToString(), coinsChosen[0].ToString(), coinsChosen[1].ToString(), coinsChosen[2].ToString(), coinsChosen[3].ToString(), coinsChosen[4].ToString(), coinsChosen[5].ToString() };
                log = string.Join("-", joinString);
            }
            else if (operateEnum == OprateEnum.OrderACard)
            {
                string[] joinString = { operateEnum.ToString(), cardChosen.ID.ToString(), cardChosen.level.ToString() };
                log = string.Join("-", joinString);
            }
            else
            {
                return "";
            }
            return log;
        }

        public void EndMyRound()
        {
            isMyturn = false;
            hasFinished = false;
        }

        public bool CanBuyACard(Card card)
        {
            var costList = card.GetCost();
            var yellowCoins = coinsHave[(int)SplendorEnum.Yellow];
            for (int i = 0; i < 6; i++)
            {
                var left = costList[i] - splendorsHave[i] - coinsHave[i];
                if (left > 0)
                {
                    yellowCoins -= left;
                    if (yellowCoins >= 0)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    continue;
                }
            }

            return true;
        }

        public bool CanGetCoins(int[] coinsList)
        {
            if (coinsList.Sum() + coinsHave.Sum() > 10)
            {
                return false;
            }
            for (int i = 0; i < 6; i++)
            {
                if (coinsList[i] >= 2 && localManager.coinsManager.GetCoinsCountBy((SplendorEnum)i) < 4)
                    return false;
                if (coinsList[i] >= 2 && coinsList.Sum() >= 3)
                    return false;
            }
            return true;
        }

        public bool CanOrderACard()
        {
            if (orderCardsList.Count >= MAX_ORDER_CARD_COUNT)
            {
                return false;
            }
            if (coinsHave.Sum() >= 10)
            {
                return false;
            }
            return true;
        }
        #endregion
    }

}