using UnityEngine;

public class SteNetworkClientObject : MonoBehaviour
{
    public bool initializated = false;
    public string playerName;
    public string role;
    public int connectionId;
    public bool destroyIfDisconnected = true;

    public virtual void OnDisconnect()
    {
        Debug.LogWarning("OnDisconnect() was not overridden.");
    }
}
