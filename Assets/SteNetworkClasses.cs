using UnityEngine.Networking;

public enum SteMsgType : short
{
    Identification = 1000,
    Rejection
}

public class IdentificationMessage : MessageBase
{
    public string playerName;
    public string role;
    public string game;
}

public enum RejectionReason
{
    MismatchedGame,
    NewClientConnected,
    ServerClosing
}

public class RejectionMessage : MessageBase
{
    public RejectionReason reason;
}

public class HelperMethods
{
    public static NetworkConnection GetConnectionById(int id)
    {
        foreach(NetworkConnection nc in NetworkServer.connections)
        {
            if (nc == null) continue;
            if(nc.connectionId == id)
            {
                return nc;
            }
        }
        return null;
    }
}