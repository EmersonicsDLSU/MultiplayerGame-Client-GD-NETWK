using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // singleton instance
    public static UIManager instance;
    // panel UI object
    public GameObject startMenu;
    // input field UI object
    public InputField usernameField;
    // singleton process    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError($"Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    // will be called when the player clicks the 'connect' button
    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        // the client will now connect to the server
        Client.instance.ConnectToServer();
    }
}
