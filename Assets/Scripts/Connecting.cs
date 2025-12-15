using Mirror;
using UnityEngine;

public class Connecting : MonoBehaviour
{
    private NetworkManager manager;

    public void Connect()
    {
        manager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();

        manager.networkAddress = ServerFound.ip;
        if (Transport.active is PortTransport portTransport) portTransport.Port = ServerFound.port;

        if (ServerFound.isHost && !ServerFound.isServer) manager.StartHost();
        else if (ServerFound.isHost && ServerFound.isServer) manager.StartServer();
        else if (!ServerFound.isHost) manager.StartClient();
    }
}