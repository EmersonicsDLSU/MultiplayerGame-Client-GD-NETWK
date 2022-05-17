using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lets the server to handle the movement calculation of the players
// in the world, server will only gather inputs from the clients
public class PlayerController : MonoBehaviour
{
    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        // array of inputs from the player
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D)
        };

        ClientSend.PlayerMovement(_inputs);
    }
}
