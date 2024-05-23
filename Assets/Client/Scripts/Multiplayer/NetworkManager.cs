using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private bool isPlayerTypeSet = false;
    private bool isImpaired;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        if (isPlayerTypeSet)
        {
            Client.Tick();
        }
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    public void SetPlayerType(bool isImpaired)
    {
        this.isImpaired = isImpaired;
        isPlayerTypeSet = true;

        Connect();

        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.SpawnPlayer);
        message.AddVector3(Vector3.zero);
        message.AddBool(isImpaired);
        Singleton.Client.Send(message);
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].gameObject);
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
    foreach (Player player in Player.list.Values)
        Destroy(player);
    }
}