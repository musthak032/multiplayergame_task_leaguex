using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using GameCreator;

public class lobbymanager : MonoBehaviourPunCallbacks 
{
    [Header("----ui screen---")]
    public GameObject roomui;
    public GameObject connectui;
    public GameObject lobbyui;

    [Header("----UI Text---")]
    public Text statusText;
    public Text connectingText;
    public Text startbtnText;
    public Text waitingforplayer;
    public Text lobbyText;
    

    [Header("----Ui input field----")]
    public InputField createroom;
    public InputField joinroom;
    public InputField username;
    public Button startbutton;
    int masteclientcount;
    #region
    private void Awake()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
         PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void connectsever()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnConnectedToMaster()
    {
        connectingText.text = "joining lobby..";
        PhotonNetwork.JoinLobby(TypedLobby.Default);
       
    }
    
    private void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }
    private void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
    #endregion
    public override void OnJoinedLobby()
    {
        connectui.SetActive(false);
        roomui.SetActive(true);
        username.text = "player" + Random.Range(100, 999);
        statusText.text = "joined to lobby";
    }
    public override void OnJoinedRoom()
    {
     
        //team
        //int sizeofPlayer = PhotonNetwork.CountOfPlayersInRooms; 
        int sizeofPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        assignteam(sizeofPlayer);
        //end team

        lobbyui.SetActive(true);
        //add curent player
        //PhotonNetwork.CurrentRoom.Players
        foreach(Player p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            GetComponent<lobbyuimanager>().addplayer(p.NickName);
        }
       // GetComponent<lobbyuimanager>().addplayer(PhotonNetwork.LocalPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            startbtnText.text = "Waiting for player";

           


        }
        else
        {
           
            startbtnText.text = "Ready!";
            //player is automatically ready own edit
            onclick_startbtn();
           
        }
      
        //PhotonNetwork.LoadLevel(1);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {

        connectui.SetActive(true);
        connectingText.text ="disconnected.."+cause.ToString();

       
        roomui.SetActive(false);
    }

    //call back for randomjoinroom
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        int roomName = Random.Range(0, 10000);
        RoomOptions roomoptions = new RoomOptions();
        roomoptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName.ToString(), roomoptions, TypedLobby.Default, null);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        //add new enter player
        GetComponent<lobbyuimanager>().addplayer(newPlayer.NickName);
      


    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        GetComponent<lobbyuimanager>().removeplayer(otherPlayer.NickName);
        sendMsg();
      

    }
    #region buttonclicks
    public void onclick_createBtn()
    {
        RoomOptions roomoptions = new RoomOptions();
        roomoptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(createroom.text,roomoptions, TypedLobby.Default,null);
    }   
    public void onclick_joinBtn()
    {
        RoomOptions roomoptions = new RoomOptions();
        roomoptions.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(joinroom.text, roomoptions, TypedLobby.Default);
    }
    public void onclick_PlayBtn()
    {
        if (string.IsNullOrEmpty(username.text))
        {
            username.text = "user" + Random.Range(100, 999);
        }
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PhotonNetwork.JoinRandomRoom();
        statusText.text = "creating room...please wait....";
    }
    public void onclick_levaelobbybtn()
    {
        
        
        if (PhotonNetwork.IsMasterClient)
        {
            
            
            foreach (Player p in PhotonNetwork.CurrentRoom.Players.Values)
            {
                GetComponent<lobbyuimanager>().removeplayer(p.NickName);
            }
            
            PhotonNetwork.LeaveRoom();
            //PhotonNetwork.LeaveLobby();
            //PhotonNetwork.LoadLevel(0);
            lobbyui.SetActive(false);
           
        }
        else
        {
            foreach (Player p in PhotonNetwork.CurrentRoom.Players.Values)
            {
                GetComponent<lobbyuimanager>().removeplayer(p.NickName);
            }
            PhotonNetwork.LeaveRoom();
            //PhotonNetwork.LeaveLobby();
            //PhotonNetwork.LoadLevel(0);
            lobbyui.SetActive(false);
        }
       
       
        
    }
   
    #endregion

    #region myfunction
    //room member use for race team like room
    void assignteam(int sizeofPlayer)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        if (sizeofPlayer % 2 == 0)
        {
            hash.Add("team", 0);
        }
        else
        {
            hash.Add("team", 1);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        print("team num" + PhotonNetwork.LocalPlayer.CustomProperties["team"]);
    }
    public void onclick_startbtn()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            sendMsg();
           
            startbutton.interactable = false;
            startbtnText.text = "wait for player";
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                startbutton.interactable = true;
                startbtnText.text = "wait for player or start";
            }
            print("not master count " + count);
            waitingforplayer.text = "only" + PhotonNetwork.CurrentRoom.PlayerCount + "/ 4 player are Ready ";
        }
        else
        {
            startbtnText.text = "wait for player";
            if (count >= 2)
            {
                startbtnText.text = "Start";
                PhotonNetwork.LoadLevel(1);
                //lobbyText.text = "All set: Play the Game";
            }
        }
    }
    #endregion

    #region raise_event
    enum EventCodes
    {
        ready=1
    }
    
    int count = 1;
    public void OnEvent(EventData photonEvent)
    {
        
        byte eventcode = photonEvent.Code;
        object content = photonEvent.CustomData;
        EventCodes code = (EventCodes)eventcode;

        if (code == EventCodes.ready)
        {
            object[] data = content as object[];
            if (PhotonNetwork.IsMasterClient)
            {

                //count++;
                count = PhotonNetwork.CurrentRoom.PlayerCount;
               
                if (count >= 2)
                {
                    startbtnText.text = "start !";
                    print(".....................................");

                    waitingforplayer.text = "only" + count + "/ 4 player are Ready ";
                }
                else
                {
                    //startbtnText.text="only"+count+"/4 player are Ready";
                    waitingforplayer.text = "only" + count + "/ 4 player are Ready ";
                }
               
            }
            else
            {
                //count = PhotonNetwork.CurrentRoom.PlayerCount;
                waitingforplayer.text = "only" + PhotonNetwork.CurrentRoom.PlayerCount + "/ 4 player are Ready ";
            }
            
        }
    }
    public void sendMsg()
    {
        string message = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        object[] datas = new object[] { message};
        RaiseEventOptions options = new RaiseEventOptions
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.MasterClient,
        };
        SendOptions sendoption = new SendOptions();
        sendoption.Reliability = true;

        PhotonNetwork.RaiseEvent((byte)EventCodes.ready, datas, options, sendoption);

    }
    #endregion
   
}
