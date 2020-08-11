using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pers.YHY.SuperSplendor.Card;

namespace Pers.YHY.SuperSplendor
{
    public class CoinsManager : MonoBehaviour
    {
        [SerializeField]
        GameObject[] coinsPositions;

        [SerializeField]
        GameObject[] coinsPrefabs;

        [SerializeField]
        int[] splendorCoinsCounts;

        Stack<GameObject>[] coinsStacks;


        private void Awake()
        {

        }
        // Start is called before the first frame update
        void Start()
        {
            splendorCoinsCounts = new int[6];
            coinsStacks = new Stack<GameObject>[6];
            for (int i = 0; i < 6; i++)
            {
                coinsStacks[i] = new Stack<GameObject>(7);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void CreateCoins()
        {
            for (int i = 0; i < 6; i++)
            {
                splendorCoinsCounts[i] = 7;
            }
            splendorCoinsCounts[(int)SplendorEnum.Yellow] = 5;

            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < splendorCoinsCounts[i]; k++)
                {
                    GameObject coin = Instantiate(coinsPrefabs[i], coinsPositions[i].transform.position, Quaternion.identity, coinsPositions[i].transform);
                    coinsStacks[i].Push(coin);
                }
            }
        }

        public int GetCoinsCountBy(SplendorEnum splendor)
        {
            return coinsStacks[(int)splendor].Count;
        }

        public GameObject PickACoin(SplendorEnum splendor)
        {
            if (GetCoinsCountBy(splendor) > 0)
                return coinsStacks[(int)splendor].Peek();
            return null;
        }

        public void PopCoins(int[] cost)
        {
            for (int i = 0; i < cost.Length; i++)
            {
                for (int k = 0; k < cost[i]; k++)
                {
                    var coin = coinsStacks[i].Pop();
                    Destroy(coin);
                }
            }
        }

        public void PushCoins(int[] more)
        {
            for (int i = 0; i < more.Length; i++)
            {
                for (int k = 0; k < more[i]; k++)
                {
                    GameObject coin = Instantiate(coinsPrefabs[i], coinsPositions[i].transform.position, Quaternion.identity, coinsPositions[i].transform);
                    coinsStacks[i].Push(coin);
                }
            }
        }
    }

}