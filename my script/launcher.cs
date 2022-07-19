using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class launcher : MonoBehaviourPunCallbacks
{
    [Header("menu mange")]
    public GameObject checkconnection;
    public GameObject firsttimenameget;
    public GameObject lobby;
    public GameObject settingpannel;

    public void checkinternet()
    {
        checkconnection.SetActive(false);
    }
    public void checkfirstnameget()
    {
        firsttimenameget.SetActive(false);
    }
    public void offsettingpanel()
    {
        settingpannel.SetActive(false);
    }
    public void onsettingpanel()
    {
        settingpannel.SetActive(true);
    }
    ///if play butoon is press waiting a in lobby for a player
    public void playbutton()
    {
        lobby.SetActive(true);
    }
    public void exitplaybutton()
    {
        lobby.SetActive(false);
    }

   
    public void setplayername()
    {

    }

}
