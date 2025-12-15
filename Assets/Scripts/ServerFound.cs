using System;
using UnityEngine;

public class ServerFound : MonoBehaviour
{
    public static string ip = "localhost";
    public static ushort port = 7777;
    public static bool isHost;
    public static bool isServer;

    public void SetServerIp(string IP) => ip = IP;

    public void SetServerPort(string PORT) => port = Convert.ToUInt16(PORT);

    public void host(bool is_host) => isHost = is_host;

    public void server(bool is_server) => isServer = is_server;
}