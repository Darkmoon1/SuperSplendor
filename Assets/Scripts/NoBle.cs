using UnityEngine;
using UnityEngine.UI;
using static Pers.YHY.SuperSplendor.Card;

namespace Pers.YHY.SuperSplendor
{
    public class NoBle : MonoBehaviour
    {

        [SerializeField]
        Text scoreDisplay;
        [SerializeField]
        Text[] costDiplays;

        [SerializeField]
        public int score;
        public int[] costList;

        // Start is called before the first frame update
        void Awake()
        {
            scoreDisplay.color = Color.white;
            foreach (var it in costDiplays)
            {
                it.color = new Color(255, 255, 255, 0);
            }
        }

        public void Init(int []costList)
        {
            this.score = 3;
            this.scoreDisplay.text = score.ToString();
            this.costList = costList;

            this.costDiplays[0].color = Color.red;

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
                        costDiplays[k].color = Color.red;
                        break;
                    case SplendorEnum.Green:
                        costDiplays[k].color = Color.green;
                        break;
                    case SplendorEnum.Black:
                        costDiplays[k].color = Color.black;
                        break;
                    case SplendorEnum.Blue:
                        costDiplays[k].color = Color.blue;
                        break;
                    case SplendorEnum.White:
                        costDiplays[k].color = Color.white;
                        break;
                    case SplendorEnum.Yellow:
                        costDiplays[k].color = Color.yellow;
                        break;
                }
                costDiplays[k].text = cost.ToString();

                k++;
                if (k >= 3)
                {
                    k = 0;
                }
            }
            if (count < 2)
            {
                Debug.LogError("init cost list is illegal");
            }
        }
    }

}