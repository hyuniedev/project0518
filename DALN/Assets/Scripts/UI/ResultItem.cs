using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResultItem : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text rankText;
        [SerializeField] private Text winText;
        
        public void SetUp(bool isWin, string name, int rank)
        {
            winText.text = isWin ? "Win" : "Lose";
            winText.color = isWin ? Color.green : Color.red;
            nameText.text = name;
            rankText.text = "Rank: " + rank;
        }
    }
}