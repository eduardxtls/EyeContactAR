using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections.Generic;
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

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    public Server Server { get; private set; }

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        Server.ClientConnected += NewPlayerConnected;
        Server.ClientDisconnected += PlayerLeft;

        Server.Start(port, maxClientCount);

        Application.targetFrameRate = 30;
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();

        Server.ClientConnected -= NewPlayerConnected;
        Server.ClientDisconnected -= PlayerLeft;
    }

    private void NewPlayerConnected(object sender, ServerClientConnectedEventArgs e)
    {
        // Notify existing players about the new player
        foreach (Player player in Player.list.Values)
        {
            if (player.Id != e.Client.Id)
            {
                player.SendSpawned(player.Id);
            }
        }
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        // Destroy the player associated with the disconnected client
        Destroy(Player.list[e.Id].gameObject);
    }

    // Method to send cue settings to a specific player
    public void SendCueSettings(bool interest, bool soundActive, bool visualActive)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.InterestPlayer);

        message.AddUShort(0);
        message.AddBool(interest); // No interest message, only settings...
        message.AddBool(soundActive);
        message.AddBool(visualActive);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    // Button methods
    public void OnSoundCueButtonPressed()
    {
        Debug.Log("Sent sound.");
        SendCueSettings(false, true, false); // Enable sound, disable visuals
    }

    public void OnVisualCueButtonPressed()
    {
        Debug.Log("Sent visual cue.");
        SendCueSettings(false, false, true); // Disable sound, enable visuals
    }

    public void OnBothCuesButtonPressed()
    {
        Debug.Log("Sent both cue types.");
        SendCueSettings(false, true, true); // Enable both sound and visuals
    }

    public void OnManualInterestPressed()
    {
        Debug.Log("Sent interest.");
        SendCueSettings(true, false, false); // Enable both sound and visuals
    }
}
