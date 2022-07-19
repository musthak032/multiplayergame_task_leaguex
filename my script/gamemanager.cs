using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using GameCreator.Core;
using GameCreator.Characters;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;




public class gamemanager : MonoBehaviourPunCallbacks,IOnEventCallback
{
    [Header("timersetup")]
    public Text roomtimertext;
    public float starttime;
    //timer setup

    public float matchlength = 180f;
    float currentmatchlength;
    float sendtimer;
    float constarttime;
    [Header("health system")]
    public int health = 3;
    public Text healthtext;
  
    public GameObject gameoverui;
    public Text gameovertext;

    [Header("player action")]
    public Actions ragDollNotDeath;
    public Actions ragDollDeath;
    GameObject eventsystem;

    [Header("LeaderBoard")]
    public GameObject leaderboardui;
    public GameObject leaderboardcontainer;
    public Text rank;

    public leaderboardplayer leaderboardplayerdisplay;
    List<leaderboardplayer> lboardplayer = new List<leaderboardplayer>();

    //public GameObject addnamerankprefab;

 
    [Header("startcount")]
    public float startcountdownnum = 3;
    public Text startcountdownnumtext;

   

    //state setup
  
   public enum gamestate
    {
        playing,
        ending
    }
    public gamestate state = gamestate.playing;
    /// </summary>raise event
    public enum EventCodes : byte
    {
        newplayer,
        listplayers,
        updatestate,
        timersync
    }
    [Header("raise event")]
    public List<playerinfo> allplayer = new List<playerinfo>();
    int index=0;

    public PlayerCharacter currentplayer;

    //finds and list all player
   
    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            newplayersend(PhotonNetwork.NickName);
            eventsystem = FindObjectOfType<EventSystem>().gameObject;
           
            state = gamestate.playing;
            setuptimer();

            updatestatdisplay();
            PhotonNetwork.CurrentRoom.IsOpen = false;
           
          
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        if (startcountdownnum != 0)
        {
            
            startcountdown();

        }
        else
        {
            playerenterroomtimestart();
        }
       
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (leaderboardui.activeInHierarchy)
            {
                leaderboardui.SetActive(false);
            }
            else
            {
                showleaderBoard();

            }
        }
      

        
    }
    void startcountdown()
    {
        startcountdownnum -= Time.deltaTime;
        startcountdownnumtext.text = startcountdownnum.ToString("0");
        if (startcountdownnum <= 0)
        {
            startcountdownnum = 0;
            startcountdownnumtext.gameObject.SetActive(false);
           

        }

    }
    void playerenterroomtimestart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentmatchlength > 0 && state == gamestate.playing)
            {
                currentmatchlength -= Time.deltaTime;
                if (currentmatchlength <= 0)
                {
                    currentmatchlength = 0;
                   
                   
                        listplayersend();
                        showleaderBoard();
                    Debug.Log("show leaderboard");
                    state = gamestate.ending;

                }
                updatetimerdisplay();

                sendtimer -= Time.deltaTime;
                if (sendtimer <= 0)
                {
                    sendtimer += 1f;
                    timersend();
                }
            }
        }
        if (currentmatchlength == 0)
        {
            //listplayersend();
            showleaderBoard();
        }
        //else
        //{
        //    listplayersend();
        //}

    }


   public void exitroom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            {
                PhotonNetwork.DestroyAll();
            }
            
        }
        else
        {
           
            
        }


        Destroy(eventsystem);



        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();

        StartCoroutine(stopco());

    }
    IEnumerator stopco()
    {
        
        yield  return  new WaitForSecondsRealtime(.5f);
        SceneManager.LoadScene(0);

    }

    //trigger system

  public  void showgameoverUi()
    {
     
        
            gameoverui.SetActive(true);
       
        
    }
 


    public void healthmanage()
    {
        healthtext.text = health.ToString();
        if (health <= 0)
        {
            ragDollDeath.Execute();
            health = 0;
            
            showgameoverUi();
        }
    }

    //leaderboard
 

    //raise event system in photon



    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            
            switch (theEvent)
            {
                case EventCodes.newplayer:
                    newplayerreceive(data);
                    break;

                case EventCodes.listplayers:
                    listplayerreceive(data);
                    break;

                case EventCodes.updatestate:
                    updatestatereceive(data);
                    break;
                case EventCodes.timersync:
                    timerreceive(data);
                    break;
            }
        }
    }

    public void newplayersend(string username)
    {
        object[] package = new object[3];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.newplayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }
    public void newplayerreceive(object[] datareceive)
    {
        playerinfo player = new playerinfo((string)datareceive[0],(int) datareceive[1],(int) datareceive[2]);
        allplayer.Add(player);
        listplayersend();
    }
    public void listplayersend()
    {
        object[] package = new object[allplayer.Count];
        for(int i = 0; i < allplayer.Count; i++)
        {
            object[] piece = new object[3];
            piece[0] = allplayer[i].name;
            piece[1] = allplayer[i].actor;
            piece[2] = allplayer[i].rank;
            package[i] = piece;

        
        }
        PhotonNetwork.RaiseEvent(
        (byte)EventCodes.listplayers,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
        );
    }
    public void listplayerreceive(object[] datareceive)
    {
        allplayer.Clear();
        for(int i = 0; i < datareceive.Length; i++)
        {
            object[] piece = (object[])datareceive[i];
            playerinfo player = new playerinfo(
                  (string)piece[0],
                  (int)piece[1],
                  (int)piece[2]
                );
            allplayer.Add(player);
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
    }
    
    public void updatestatesend(int actorsend, int statetoupdate,int amounttochange)
    {
        

        
            object[] package = new object[] { actorsend, statetoupdate, amounttochange };
            PhotonNetwork.RaiseEvent(
          (byte)EventCodes.updatestate,
          package,
          new RaiseEventOptions { Receivers = ReceiverGroup.All },
          new SendOptions { Reliability = true }
      );


    }
    public void updatestatereceive(object[] datareceive)
    {
        int actor = (int)datareceive[0];
        int statetype = (int)datareceive[1];
        int amount = (int)datareceive[2];
        
        for(int i = 0; i < allplayer.Count; i++)
        {
            if (allplayer[i].actor==actor)
            {
                switch (statetype)
                {
                    case 0:
                         
                        allplayer[i].rank += amount;


                        break;
                    case 1:
                       
                        allplayer[i].rank = 0;
                        break;


                }
            }
            if (i == index)
            {
                updatestatdisplay();
            }
            if (leaderboardui.activeInHierarchy)
            {
                showleaderBoard();
            }
           
            break;
        }
    }
    public void updatestatdisplay()
    {

        //addleaderboard(allplayer.Add[index].rank);
        if (allplayer.Count > index)
        {
            rank.text = "rank: " + allplayer[index].rank;
            Debug.Log(rank.text);
        }
        //else
        //{
        //    rank.text = "not finish";
        //}
    }

 

  public  void showleaderBoard()
    {
        leaderboardui.SetActive(true);
        foreach(leaderboardplayer lp in lboardplayer)
        {
            Destroy(lp.gameObject);
        }
        lboardplayer.Clear();
        leaderboardplayerdisplay.gameObject.SetActive(false);

        //List<playerinfo> sorted = sortplayer(allplayer);

        foreach(playerinfo player in allplayer)
        {
            leaderboardplayer newplayerDisplay = Instantiate(leaderboardplayerdisplay, leaderboardcontainer.transform.parent);
            newplayerDisplay.transform.parent = leaderboardcontainer.transform;
           
            newplayerDisplay.setdetail(player.name, player.rank);
            newplayerDisplay.SetActive(true);
            lboardplayer.Add(newplayerDisplay);
        }
    }

    private List<playerinfo> sortplayer(List<playerinfo> players)
    {
        List<playerinfo> sorted = new List<playerinfo>();

        while (sorted.Count > players.Count)
        {
            int highes = 5;
            playerinfo selectedplayer = players[0];
            foreach(playerinfo player in players)
            {
                if (!sorted.Contains(player))
                {

                    if (player.rank <highes)
                    {
                        selectedplayer = player;
                        highes = player.rank;
                    }
                }
            }
            sorted.Add(selectedplayer);
        }

        return sorted;
    }

    public void setuptimer()
    {
        if (matchlength > 0)
        {
            currentmatchlength = matchlength;
            updatetimerdisplay();
        }
    }
    public void updatetimerdisplay()
    {
        var timetodisplay = System.TimeSpan.FromSeconds(currentmatchlength);
        roomtimertext.text = timetodisplay.Minutes.ToString("00")+":"+timetodisplay.Seconds.ToString("00");
    }

    public void timersend()
    {
        object[] package = new object[] { (int)currentmatchlength, state };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.timersync,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );

    }

    public void timerreceive(object[] datareceived)
    {
        currentmatchlength = (int)datareceived[0];
        state = (gamestate)datareceived[1];
        updatetimerdisplay();
    }
}
[System.Serializable]
public class playerinfo
{
    public string name;
    public int actor,rank;

    public playerinfo(string _name, int _actor, int _rank)
    {
        name = _name;
        actor = _actor;
        rank = _rank;
    }
}
