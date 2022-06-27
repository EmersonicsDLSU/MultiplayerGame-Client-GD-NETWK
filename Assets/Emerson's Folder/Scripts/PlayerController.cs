using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;
    private PlayerManager playerManagerSc;


    void Start()
    {
        playerManagerSc = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !GameManager.instance._isGameFinished)
        {
            ClientSend.PlayerShoot(camTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && !GameManager.instance._isGameFinished)
        {
            ClientSend.PlayerThrowItem(camTransform.forward);
        }
    }
    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>Sends player input to the server.</summary>
    private void SendInputToServer()
    {
        if (GameManager.instance._isGameFinished)
        {
            bool[] inputs = new bool[]{false, false, false, false};
            ClientSend.PlayerMovement(inputs);
            return;
        }
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };

        ClientSend.PlayerMovement(_inputs);
    }
}