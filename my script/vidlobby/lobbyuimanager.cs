using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lobbyuimanager : MonoBehaviour
{
    public GameObject playercontainer;
    public GameObject playerobjectprefab;
    public void addplayer(string playername)
    {
       GameObject pop= Instantiate(playerobjectprefab, Vector3.zero, Quaternion.identity);
        pop.transform.GetChild(0).GetComponent<Text>().text = playername;
        pop.transform.parent = playercontainer.transform;
        pop.transform.localScale = Vector3.one;
        pop.name = playername;
    }
    public void removeplayer(string playername)
    {
        int popcount = playercontainer.transform.childCount;
        for(int i = 0; i < popcount; i++)
        {
            if (playercontainer.transform.GetChild(i).name == playername) 
            {
                Destroy(playercontainer.transform.GetChild(i).gameObject);
                return;
            }

        }
    }
}
