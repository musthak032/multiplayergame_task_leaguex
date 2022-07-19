using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class leaderboardplayer : MonoBehaviour
{
    public Text playername;
    public Text ranktext;

    public void setdetail(string name, int rank)
    {
        playername.text = name;
        ranktext.text = rank.ToString();
        //if (rank == 0)
        //{
        //    ranktext.text = "not finish";
        //}
    }
}
