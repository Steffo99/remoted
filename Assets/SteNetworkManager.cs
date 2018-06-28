using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SteNetworkManager : MonoBehaviour {

    public int port = 1234;
    public string game = "UNSET";
    public GameObject clientGameObject;

    protected Dictionary<int, GameObject> clientObjects;

    void Start()
    {
        clientObjects = new Dictionary<int, GameObject>();
        StartNetworkServer();
        RegisterGameHandlers();
    }

	void StartNetworkServer () {
        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
        NetworkServer.RegisterHandler((short) SteMsgType.Identification, OnClientIdentification);
    }

    protected virtual void RegisterGameHandlers()
    {
        Debug.LogWarning("RegisterGameHandlers() was not overriden.");
    }

    void OnClientConnected(NetworkMessage netMsg)
    {
        Debug.Log("Client [" + netMsg.conn.address.ToString() + "] connected to the server.");
    }

    void OnClientDisconnected(NetworkMessage netMsg)
    {
        if(clientObjects.ContainsKey(netMsg.conn.connectionId))
        {
            Debug.LogErrorFormat("Client [{0}] unexpectedly disconnected from the server.", netMsg.conn.address);
            SteNetworkClientObject clientObject = clientObjects[netMsg.conn.connectionId].GetComponent<SteNetworkClientObject>();
            clientObject.OnDisconnect();
            if(clientObject.destroyIfDisconnected) Destroy(clientObjects[netMsg.conn.connectionId]);
            clientObjects.Remove(netMsg.conn.connectionId);
        }
        else
        {
            Debug.LogFormat("Client [{0}] disconnected from the server.", netMsg.conn.address);
        }
    }

    void OnError(NetworkMessage netMsg)
    {
        Debug.LogError("An error occoured on the server.");
    }

    void OnClientIdentification(NetworkMessage netMsg)
    {
        IdentificationMessage identification = netMsg.ReadMessage<IdentificationMessage>();
        //Reject mismatched games
        if (identification.game != game)
        {
            Debug.LogErrorFormat("Client [{0}] connected with a mismatched game ({1}), connection refused.", netMsg.conn.address, identification.game);
            netMsg.conn.Send((short)SteMsgType.Rejection, new RejectionMessage { reason = RejectionReason.MismatchedGame });
            netMsg.conn.Disconnect();
            return;
        }
        //Remove duplicate player names
        foreach(GameObject gO in clientObjects.Values)
        {
            SteNetworkClientObject cO = gO.GetComponent<SteNetworkClientObject>();
            if(cO.playerName == identification.playerName)
            {
                //Force close the same-name player
                Debug.LogErrorFormat("Client [{0}] connected with a duplicate name ({1}), dropping the other connection.", netMsg.conn.address, identification.playerName);
                foreach(NetworkConnection nC in NetworkServer.connections)
                {
                    if(nC == null) continue;
                    if(nC.connectionId == cO.connectionId)
                    {
                        nC.Send((short)SteMsgType.Rejection, new RejectionMessage { reason = RejectionReason.NewClientConnected });
                        break;
                    }
                }
                clientObjects.Remove(cO.connectionId);
                cO.connectionId = netMsg.conn.connectionId;
                clientObjects.Add(cO.connectionId, gO);
                return;
            }
        }
        //Successfully connect
        Debug.LogFormat("Client [{0}] identified itself as \"{1}\" ({2}).", netMsg.conn.address, identification.playerName, identification.role);
        GameObject created = Instantiate(clientGameObject);
        clientObjects.Add(netMsg.conn.connectionId, created);
        SteNetworkClientObject clientObject = created.GetComponent<SteNetworkClientObject>();
        clientObject.playerName = identification.playerName;
        clientObject.role = identification.role;
        clientObject.connectionId = netMsg.conn.connectionId;
        created.name = string.Format("{0} ({1}, {2})", created.name, identification.playerName, identification.role);
        ClientObjectInitialization(created);
        clientObject.initializated = true;
    }

    protected void ClientObjectInitialization(GameObject created)
    {
        Debug.LogWarning("ClientObjectInitialized() was not overriden.");
    }
}
