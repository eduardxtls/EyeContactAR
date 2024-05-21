using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Dictionary to store all players by their IDs
    public static Dictionary<ushort,Player> list = new Dictionary<ushort, Player>();

    // Player ID and position
    private Vector3 position;
    public ushort id { get; private set; }

    private bool isObserved;
    private TimeTracker timeTracker;

    // Boolean to track whether this player is local
    private bool isLocal;

    void Start()
    {
        this.isObserved = false;
        // Determine if this player is local or remote based on its ID and the local client's ID
        isLocal = id == NetworkManager.Singleton.Client.Id;
    }

    void Update()
    {
        // If this player is local, update its position based on local input
        if (this.isLocal)
        {
            // Update position based on local input, e.g., keyboard or touch controls
            position = Camera.main.transform.position;

            // Send the updated position to the server
            SendPosition();

            // Check whether the threshold for interest is reached
            if (this.timeTracker.timeObserved >= 10f)
            {
                // Notify other player
                SendInterest();
            }

            // Check if player is being observed
            if (this.isObserved)
            {
                // Show observer / visual and audio cues
            }
        }
    }

    public static void Spawn(ushort id, Vector3 position)
    {
        Player player;

        // Instantiate prefabs based on local/remote
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.isLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.isLocal = false;
        }

        // Set id + position and add player to the dictionary
        player.id = id;
        player.position = position;
        list.Add(player.id, player);
    }

    private void SendInterest()
    {
        // Create a message containing the player's interest
        Message message = Message.Create((MessageSendMode)0, ClientToServerId.InterestPlayer);
        message.AddUShort(id);
        message.AddBool(true);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendPosition()
    {
        // Create a message containing the player's position
        Message message = Message.Create((MessageSendMode)0, ClientToServerId.MovePlayer);
        message.AddUShort(id);
        message.AddVector3(position);
        message.AddVector3(Camera.main.transform.forward);
        NetworkManager.Singleton.Client.Send(message);
    }

    // Message handler for updating the position of remote players
    [MessageHandler((ushort)ServerToClientId.MovePlayer)]
    private static void MovePlayer(Message message)
    {
        ushort id = message.GetUShort();
        Vector3 position = message.GetVector3();
        
        // Update the position of the remote player
        if (NetworkManager.Singleton.Client.Id != id)
        {
            if (list.TryGetValue(id, out Player player))
            {
                player.position = position;
                player.transform.forward = message.GetVector3();
            }
        }
    }

    // Message handler for spawning remote players
    [MessageHandler((ushort)ServerToClientId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetVector3());
        Debug.Log("Spawned player successfully!");
    }

    // Message handler for interest cues
    [MessageHandler((ushort)ServerToClientId.InterestPlayer)]
    private static void InterestPlayer(Message message)
    {
        ushort id = message.GetUShort();
        bool interest = message.GetBool();

        if (interest)
        {
            // Enable audio feedback which coincides with position
        }
    }
}
