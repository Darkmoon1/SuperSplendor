using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pers.YHY.SuperSplendor.Card;
using DG.Tweening;
using Photon.Pun;

namespace Pers.YHY.SuperSplendor
{
    public class CardsManager : MonoBehaviour
    {

        #region private fields
        [SerializeField]
        GameObject[] cardPositions;

        GameObject[][] cardsBattlePositions;

        [SerializeField]
        Transform[] cardsBattlePostionsFather;

        [SerializeField]
        GameObject card1Prefab;
        [SerializeField]
        GameObject card2Prefab;
        [SerializeField]
        GameObject card3Prefab;

        Stack<GameObject>[] cardsStacks;

        Queue<Tuple<Transform, Vector3>> moveObjectQueue;

        Tuple<Transform, Vector3> movingObject;

        public Dictionary<int, GameObject>[] posDictionaries;
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            posDictionaries = new Dictionary<int, GameObject>[3];
            cardsStacks = new Stack<GameObject>[3];
            cardsBattlePositions = new GameObject[3][];
            moveObjectQueue = new Queue<Tuple<Transform, Vector3>>();
            for (int i = 0; i < 3; i++)
            {
                posDictionaries[i] = new Dictionary<int, GameObject>();
                cardsStacks[i] = new Stack<GameObject>();
                cardsBattlePositions[i] = new GameObject[4];
                for (int k = 0; k < 4; k++)
                {
                    cardsBattlePositions[i][k] = cardsBattlePostionsFather[i].Find($"Pos{k}").gameObject;
                }
            }

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            //DoObjectMove();
        }

        #region private functions


        private void DoObjectMove()
        {
            //移动使用了Dotween，此方法已经弃用
            var tartgetRotation = Quaternion.Euler(180f, 0, 180f);

            if (movingObject == null && moveObjectQueue.Count() != 0)
            {
                movingObject = moveObjectQueue.Dequeue();
            }

            if (movingObject == null)
            {
                return;
            }

            if (Quaternion.Angle(movingObject.Item1.rotation, tartgetRotation) > 1)
            {
                movingObject.Item1.rotation = Quaternion.Slerp(movingObject.Item1.rotation, tartgetRotation, 0.1f);
            }
            else
            {
                movingObject.Item1.rotation = tartgetRotation;
            }


            var dis = movingObject.Item1.position - movingObject.Item2;

            if (Math.Abs(dis.x) > 0.1 || Math.Abs(dis.y) > 0.1 || Math.Abs(dis.z) > 0.1)
            {
                movingObject.Item1.position = Vector3.MoveTowards(movingObject.Item1.position, movingObject.Item2, 0.8f);
            }
            else
            {
                movingObject.Item1.rotation = tartgetRotation;
                movingObject.Item1.position = movingObject.Item2;
                movingObject = null;
            }
        }

        #endregion



        #region public functions
        public void InitBattleCards()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    var card = cardsStacks[i].Pop();
                    var pos = cardsBattlePositions[i][k];
                    card.transform.SetParent(pos.transform);
                    card.GetComponent<Card>().position = k;
                    card.GetComponent<Card>().publish = true;
                    card.transform.DOMove(pos.transform.position, 1f);
                    card.transform.DORotate(new Vector3(180, 0, 180), 1.5f);
                    //moveObjectQueue.Enqueue(new Tuple<Transform, Vector3>(card.transform, pos.transform.position));
                }
            }
        }

        public void DrawABattleCard(int level, int pos)
        {
            if (level >= 3 || level < 0)
            {
                return;
            }
            if (cardsStacks[level].Count == 0)
            {
                return;
            }
            var card = cardsStacks[level].Pop();
            var posObj = cardsBattlePositions[level][pos];
            card.transform.SetParent(posObj.transform);
            card.GetComponent<Card>().position = pos;
            card.GetComponent<Card>().publish = true;
            card.transform.DOMove(posObj.transform.position, 1f);
            card.transform.DORotate(new Vector3(180, 0, 180), 1.5f);
            Debug.Log($"Draw {card.GetComponent<Card>().ID}");
            //moveObjectQueue.Enqueue(new Tuple<Transform, Vector3>(card.transform, posObj.transform.position));
        }

        public GameObject PopACard(string cardId, string level)
        {
            GameObject card = posDictionaries[int.Parse(level)][int.Parse(cardId)];
            posDictionaries[int.Parse(level)].Remove(int.Parse(cardId));
            Debug.Log($"destory {cardId}");
            return card;
        }

        public void CreateCards(int level)
        {
            TextAsset levelCards;
            GameObject cardPrefab;
            if (level == 0)
            {
                levelCards = Resources.Load<TextAsset>("level1Cards");
                cardPrefab = card1Prefab;
            }
            else if (level == 1)
            {
                levelCards = Resources.Load<TextAsset>("level2Cards");
                cardPrefab = card2Prefab;
            }
            else if (level == 2)
            {
                levelCards = Resources.Load<TextAsset>("level3Cards");
                cardPrefab = card3Prefab;
            }
            else
            {
                levelCards = Resources.Load<TextAsset>("level3Cards");
                cardPrefab = card3Prefab;
            }
            var cardsProperties = CSVSerializer.Deserialize<CardSerializer>(levelCards.text);
            var iter = cardsProperties.OrderByDescending(card => card.Id);
            foreach (var cardP in iter)
            {
                //GameObject card = PhotonNetwork.Instantiate(cardPrefab.name, cardPositions[level].transform.position, Quaternion.identity);
                GameObject card = Instantiate(cardPrefab, cardPositions[level].transform.position, Quaternion.identity, cardPositions[level].transform);
                int[] costList = { cardP.Red_splendor_cost, cardP.Yellow_splendor_cost, cardP.Green_splendor_cost, cardP.Black_splendor_cost, cardP.Blue_splendor_cost, cardP.White_splendor_cost };
                card.GetComponent<Card>().Init(cardP.Id, cardP.Score, costList, level, (SplendorEnum)cardP.Splendor_type);
                card.transform.Rotate(new Vector3(180, 0, 0));
                cardsStacks[level].Push(card);
                posDictionaries[level].Add(cardP.Id, card);
            }

        }

        #endregion
    }

}