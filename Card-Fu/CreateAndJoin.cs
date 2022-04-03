using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/************************************************
 * Author: Noah Judge
 * Summary: Creates and joins rooms using input
 * fields and buttons
 * Date Created: Feb 2nd, 2022
 * Date Last Edited: Mar 15th, 2022
 ************************************************/

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    //Script variables
    public TMP_InputField username;
    public Button createButton;
    public TMP_InputField createCode;
    public Button joinButton;
    public TMP_InputField joinCode;

    //Return Codes (from Photon Engine)
    const int GAME_DOES_NOT_EXIST = 32758;
    const int GAME_ALREADY_EXISTS = 32766;

    //Initiate room creation (Auto-join on success)
    public void CreateRoom()
    {
        DisableInteractivity();
        GameObject.FindGameObjectWithTag("Feedback").GetComponent<TextMeshPro>().text = "Creating Room...";
        RoomOptions settings = new RoomOptions();
        settings.MaxPlayers = 2;
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PhotonNetwork.CreateRoom(createCode.text, settings);
        
    }

    //Room creation errors
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //Room with given code already exists
        if(returnCode == GAME_ALREADY_EXISTS)
        {
            GameObject.FindGameObjectWithTag("Feedback").GetComponent<TextMeshPro>().text = "Code already in use! Try another.";
            GameObject.FindGameObjectWithTag("NameCheck").GetComponent<CheckUsername>().loading = false;
        }
        //Default error
        else
        {
            GameObject.FindGameObjectWithTag("Feedback").GetComponent<TextMeshPro>().text = "Unexpected Error!";
            GameObject.FindGameObjectWithTag("NameCheck").GetComponent<CheckUsername>().loading = false;
        }
    }

    //Initiate room join
    public void JoinRoom()
    {
        DisableInteractivity();
        GameObject.FindGameObjectWithTag("Feedback").GetComponent<TextMeshPro>().text = "Joining Room...";
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PhotonNetwork.JoinRoom(joinCode.text);
    }

    //Room found; load into waiting screen
    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Waiting", LoadSceneMode.Single);
    }

    //Room joining errors
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //Uncreated game error
        if(returnCode == GAME_DOES_NOT_EXIST)
        {
            GameObject.FindGameObjectWithTag("Feedback").GetComponent<TextMeshPro>().text = "Game not found!";
            GameObject.FindGameObjectWithTag("NameCheck").GetComponent<CheckUsername>().loading = false;
        }
        //Default error
        else
        {
            GameObject.FindGameObjectWithTag("Feedback").GetComponent<TextMeshPro>().text = "Unexpected Error!";
            GameObject.FindGameObjectWithTag("NameCheck").GetComponent<CheckUsername>().loading = false;
        }
    }

    //Disables other buttons while loading
    void DisableInteractivity()
    {
        GameObject.FindGameObjectWithTag("NameCheck").GetComponent<CheckUsername>().loading = true;
        username.interactable = false;
        createButton.interactable = false;
        createCode.interactable = false;
        joinButton.interactable = false;
        joinCode.interactable = false;
    }
}
