using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SteNetworkClient : MonoBehaviour {

    public string ipAddress = "localhost";
    public int port = 1234;
    public string playerName = "UNSET";
    public string role = "UNSET";
    public string game = "UNSET";

    NetworkClient client;
    bool rejectionReceived = false;

	void Start () {
        client = new NetworkClient();
        client.RegisterHandler(MsgType.Connect, OnConnect);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        client.RegisterHandler(MsgType.Error, OnError);
        client.RegisterHandler((short)SteMsgType.Rejection, OnRejection);
        client.Connect(ipAddress, port);
    }

    protected virtual void RegisterGameHandlers()
    {
        Debug.LogWarning("RegisterGameHandlers() was not overriden.");
    }

    void OnConnect(NetworkMessage netMsg)
    {
        Debug.LogFormat("Client connected to server.");
        IdentificationMessage msg = new IdentificationMessage
        {
            playerName = playerName,
            role = role,
            game = game
        };
        client.Send((short) SteMsgType.Identification, msg);
    }

    void OnDisconnect(NetworkMessage netMsg)
    {
        if(rejectionReceived)
        {
            Debug.Log("Client disconnected from server.");
        }
        else
        {
            Debug.LogError("Client unexpectedly disconnected from server.");
        }
    }

    void OnError(NetworkMessage netMsg)
    {
        Debug.LogError("Client connection error.");
    }

    void OnRejection(NetworkMessage netMsg)
    {
        rejectionReceived = true;
        RejectionMessage msg = netMsg.ReadMessage<RejectionMessage>();
        Debug.LogErrorFormat("Client connection rejected ({0}), disconnecting...", msg.reason);
        client.Disconnect();
    }
}
