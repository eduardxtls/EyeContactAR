using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort id { get; private set; }
    public Vector3 Position { get; private set; }
    public bool isLocal { get; private set; }
    public bool isImpaired { get; private set; }
    private bool sentInterest;

    private TimeTracker timeTracker;

    private bool isObserved;

    void Start()
    {
        this.isObserved = false;
        this.sentInterest = false;
        this.timeTracker = GetComponent<TimeTracker>();
    }

    void Update()
    {
        if (isLocal)
        {
            if (TransformCam.Singleton.transformed)
            {
                Position = TransformCam.Singleton.RelativeRotation * Camera.main.transform.position + TransformCam.Singleton.RelativePos;
                SendPosition(Position, new Vector3(0, 0, 0));

                Debug.Log("Position: " + Position);
     
            }

            if (isImpaired && isObserved)
            {
                // Show observer / visual and audio cues
            }
        }
        else
        {
            if (isImpaired && !sentInterest && timeTracker.timeObserved >= timeTracker.thresholdTime)
            {
                SendInterest();
            }
        }
    }

    public static void Spawn(ushort id, Vector3 position, bool isImpaired)
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
            if (!isImpaired)
            {
                player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
                player.isLocal = false;
            }
            else
            {
                player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
                player.isLocal = false;
            }
        }

        // Set player attributes and add them to list
        player.id = id;
        player.Position = position;
        player.isImpaired = isImpaired;

        list.Add(player.id, player);
        Debug.Log("Player impaired: " + player.isImpaired + "\nPlayer local: " + player.isLocal);
    }

    // Sending Messages -------------

    private void SendInterest()
    {
        sentInterest = true;
        Message message = Message.Create((MessageSendMode)0, ClientToServerId.InterestPlayer);
        ushort observerID = (ushort) (id % 2 + 1);
        message.AddUShort(observerID);
        message.AddBool(true);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendPosition(Vector3 position, Vector3 forward)
    {
        Message message = Message.Create((MessageSendMode)0, ClientToServerId.MovePlayer);
        message.AddUShort(id);
        message.AddVector3(position);
        message.AddVector3(forward);
        NetworkManager.Singleton.Client.Send(message);
    }

    // Receiving Messages -------------

    [MessageHandler((ushort)ServerToClientId.MovePlayer)]
    private static void MovePlayer(Message message)
    {
        ushort id = message.GetUShort();
        Vector3 position = message.GetVector3();
        Vector3 forward = message.GetVector3();

        if (list.TryGetValue(id, out Player player))
        {
            player.transform.position = TransformCam.Singleton.RelativeRotation * position + TransformCam.Singleton.RelativePos;
            Debug.Log("New position of player: " + player.transform.position);
            // player.transform.forward = forward;
        }
    }

    [MessageHandler((ushort)ServerToClientId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        ushort id = message.GetUShort();
        Vector3 position = message.GetVector3();
        bool isImpaired = message.GetBool();

        if (!list.ContainsKey(id))
        {
            Spawn(id, position, isImpaired);
            Debug.Log("Spawned player successfully!");
        }
    }

    [MessageHandler((ushort)ServerToClientId.InterestPlayer)]
    private static void InterestPlayer(Message message)
    {
        ushort id = message.GetUShort();
        bool interest = message.GetBool();

        if (list.TryGetValue(id, out Player player) && player.isImpaired)
        {
            player.isObserved = interest;
        }

        Debug.Log("Received interest message from observer " + id);
    }
}