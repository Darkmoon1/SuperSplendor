using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pers.YHY.SuperSplendor
{
    public class Card : MonoBehaviour
    {
        #region Const Fields
        public enum SplendorEnum
        {
            Red = 0,
            Yellow,
            Green,
            Black,
            Blue,
            White
        };

        #endregion

        #region Private Fields
        [SerializeField]
        public int ID;
        [SerializeField]
        int score;
        [SerializeField]
        int[] costList;
        [SerializeField]
        SplendorEnum splendorType;
        [SerializeField]
        public int level;

        public bool publish = false;
        public bool isOrdered = false;
        Canvas canvas;

        Text scoreDisplay;

        [SerializeField]
        GameObject costGroup;

        Text[] splendorCostListDisplay;

        [SerializeField]
        SpriteRenderer splendorTypeDisplay;
        #endregion

        #region public fields
        public int position;

        #endregion

        #region Mono CallBacks

        private void Awake()
        {
            score = 0;
            costList = new int[6];
            splendorType = (int)SplendorEnum.Red;
            canvas = GetComponentInChildren<Canvas>();
            scoreDisplay = canvas.GetComponentInChildren<Text>();
            splendorCostListDisplay = costGroup.GetComponentsInChildren<Text>();
            foreach (var it in splendorCostListDisplay)
            {
                it.color = new Color(255, 255, 255, 0);
            }

        }

        #endregion

        #region Public Functions
        public void Init(int ID, int score, int[] costList, int level, SplendorEnum splendorType)
        {
            if (costList.Length != 6)
            {
                Debug.Log("error init by costList");
                return;
            }
            if (score < 0)
            {
                Debug.Log("error init by score");
                return;
            }
            this.score = score;
            this.costList = costList;
            this.splendorType = splendorType;
            this.level = level;
            this.ID = ID;

            string path = (splendorType).ToString() + " S";
            Texture2D texture = (Texture2D)Resources.Load(path);
            splendorTypeDisplay.sprite = Sprite.Create(texture, splendorTypeDisplay.sprite.textureRect, new Vector2(0.5f, 0.5f));

            scoreDisplay.text = score.ToString();
            if (score == 0)
            {
                scoreDisplay.gameObject.SetActive(false);
            }

            int k = 0;
            int count = 0;
            for (int i = 0; i < 6; i++)
            {
                int cost = costList[i];
                if (cost <= 0)
                {
                    count++;
                    continue;
                }
                switch ((SplendorEnum)i)
                {
                    case SplendorEnum.Red:
                        splendorCostListDisplay[k].color = Color.red;
                        break;
                    case SplendorEnum.Green:
                        splendorCostListDisplay[k].color = Color.green;
                        break;
                    case SplendorEnum.Black:
                        splendorCostListDisplay[k].color = Color.black;
                        break;
                    case SplendorEnum.Blue:
                        splendorCostListDisplay[k].color = Color.blue;
                        break;
                    case SplendorEnum.White:
                        splendorCostListDisplay[k].color = Color.white;
                        break;
                    case SplendorEnum.Yellow:
                        splendorCostListDisplay[k].color = Color.yellow;
                        break;
                }
                splendorCostListDisplay[k].text = cost.ToString();

                k++;
                if (k >= 4)
                {
                    k = 0;
                }
            }
            if (count < 2)
            {
                Debug.LogError("init cost list is illegal");
            }
        }

        public int[] GetCost()
        {
            return costList;
        }

        public int GetScore()
        {
            return score;
        }
        public SplendorEnum GetSpendorType()
        {
            return splendorType;
        }
        #endregion
    }

}