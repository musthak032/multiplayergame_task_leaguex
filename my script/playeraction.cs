using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using GameCreator.Characters;

public class playeraction : MonoBehaviourPun
{
    public CapsuleCollider col;
    gamemanager Gamemanager;
    bool ismineplayer;
    PhotonView photonobject;

    [Header("player last location")]
    public List<Vector3> playerLastPosition;
    public float deltapositioncountdown = 5f;
    float deltapositioncountdowncurrent;
    
    public Vector3 bestdeltaposition;

    bool obtacleattack;
    string winnername;

    PlayerCharacter playerCharacter;
    int rank;
    // Start is called before the first frame update
    void Start()
    {
        deltapositioncountdowncurrent = deltapositioncountdown;
        Gamemanager = FindObjectOfType<gamemanager>();
        ismineplayer = gameObject.GetComponent<PhotonView>().IsMine;
        if (ismineplayer)
        {
            photonobject = gameObject.GetComponent<PhotonView>();
        }
        playerCharacter = gameObject.GetComponent<PlayerCharacter>();
        playerCharacter.enabled = false;
         startnamew();

    }

    

    // Update is called once per frame
    void Update()
    {
        if (ismineplayer)
        {
            Gamemanager.healthtext.text = Gamemanager.health.ToString();
           
         
        }
        playerlastlocation();
        playermomentenabled();
    }

    void playermomentenabled()
    {
        if (Gamemanager.startcountdownnum == 0)
        {
            playerCharacter.enabled = true;
        }

    }
  
    private void OnTriggerEnter(Collider other)
    {
        if (ismineplayer)
        {
            if (other.tag == "obtacles")
            {
                    
              
                if (!gameObject.GetComponent<PlayerCharacter>().IsRagdoll())
                {
                    Gamemanager.ragDollDeath.Execute();
                   
                    Gamemanager.health--;
                    Invoke("teleportplayer", 6f);
                    Gamemanager.healthmanage();
                    Debug.Log("obtacle attack");
                    obtacleattack = true;
                }         

            }
            if(other.tag == "notstage")
            {
                if (!obtacleattack)
                {
                    Gamemanager.ragDollDeath.Execute();

                    Gamemanager.health--;
                    Invoke("teleportplayer", 6f);
                    Gamemanager.healthmanage();
                    Debug.Log("not on stage attack");
                   
                }
            }
           
        }
        if (other.tag == "winner")
        {


            rank++;
            //gameObject.GetComponent<PhotonView>().RPC("fin", RpcTarget.All);

            if (photonobject.IsMine)
             {


                //gameObject.GetComponent<PhotonView>().RPC("fin", RpcTarget.All,photonobject.OwnerActorNr);
                Gamemanager.updatestatesend(photonobject.Owner.ActorNumber, 0, rank);
                FindObjectOfType<PlayerCharacter>().enabled = false;
                //Gamemanager.allplayer[photonobject.Owner.ActorNumber].rank = rank;

                Debug.Log(photonobject.Owner.ActorNumber);

                Gamemanager.showleaderBoard();


            }
            else
            {
                //Gamemanager.listplayersend();
            }
            
        

          



        }

    }


    [PunRPC]
    public void fin(int actor)
    {
              rank++;


        if (photonobject.IsMine)
        {
            Gamemanager.updatestatesend(actor, 0, 1);
            playerCharacter.enabled = false;
            //Gamemanager.listplayersend();
        }
          
         
            //Gamemanager.showleaderBoard();
        
      
    }
 

    void playerlastlocation()
    {

        if (ismineplayer)
        {


            //if (gameObject.GetComponent<PlayerCharacter>().characterState.forwardSpeed != Vector3.zero&& !gameObject.GetComponent<PlayerCharacter>().IsRagdoll())
             if (gameObject.GetComponent<PlayerCharacter>().IsGrounded() && !gameObject.GetComponent<PlayerCharacter>().IsRagdoll()&& gameObject.GetComponent<PlayerCharacter>().characterState.forwardSpeed!=Vector3.zero)
                {

                
                deltapositioncountdowncurrent -= Time.deltaTime;
                if (deltapositioncountdowncurrent < 0)
                {

                    if (playerLastPosition.Count == 10)
                    {

                        playerLastPosition.RemoveAt(0);
                    }
                    playerLastPosition.Add(transform.position);
                    deltapositioncountdowncurrent = deltapositioncountdown;
                    //playerLastPosition[0] = new Vector3(playerLastPosition[0].x, 0f, playerLastPosition[0].z);
                    //bestdeltaposition = playerLastPosition[0];
                  
                    if (playerLastPosition.Count > 6)
                    {
                        //playerLastPosition[1] = new Vector3(playerLastPosition[1].x, 0f, playerLastPosition[1].z);
                        bestdeltaposition = playerLastPosition[5];
                       
                    }

                }



            }
            else
            {
                
                    deltapositioncountdowncurrent = deltapositioncountdown;
                
            }
        }
    }
    void teleportplayer()
    {
        Gamemanager.ragDollNotDeath.Execute();
        //gameObject.GetComponent<PlayerCharacter>().characterLocomotion.Teleport(bestdeltaposition, Quaternion.identity);
        gameObject.transform.position = bestdeltaposition;

        Invoke("notdeath", 2f);
       
    }
    void notdeath()
    {
        obtacleattack = false;
        //Gamemanager.ragDollNotDeath.Execute();
    }

    void startnamew()
    {
       
           foreach(var p in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (ismineplayer)
                {

                    winnername = p.NickName;
                    Debug.Log("playername " + winnername);
                    break;
                 }
               
            
               
            


        }

        
    }
}
