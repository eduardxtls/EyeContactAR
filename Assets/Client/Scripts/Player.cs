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
    private AudioSource audioSource;

    private TimeTracker timeTracker;
    private LineRenderer lineRenderer;

    private bool isObserved;

    void Start()
    {
        this.isObserved = false;
        this.sentInterest = false;
        this.timeTracker = GetComponent<TimeTracker>();
        this.lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (isLocal)
        {
            /*
            if (TransformCam.Singleton.transformed)
            {
                Position = TransformCam.Singleton.RelativeRotation * Camera.main.transform.position + TransformCam.Singleton.RelativePos;
                SendPosition(Position, new Vector3(0, 0, 0));

                Debug.Log("Position: " + Position);
            }
            */

            
            Position = Camera.main.transform.position;
            SendPosition(Position, new Vector3(0, 0, 0));

            if (isImpaired)
            {
                ushort observerID = (ushort)((id % 2) + 1);
                if (list.TryGetValue(observerID, out Player observer))
                {
                    Vector3 pos1 = observer.transform.position;
                    Vector3 pos2 = Camera.main.transform.position;
                    pos2.y -= 0.1f;

                    // Update the LineRenderer positions
                    observer.lineRenderer.SetPosition(0, pos1);
                    observer.lineRenderer.SetPosition(1, pos2);
                }
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

            // Mute the AudioSource for the local player
            AudioSource audioSource = player.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.mute = true;
                Debug.Log("Disabled Audio Source component for local player.");
            }
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
        Message message = Message.Create((MessageSendMode)0, ClientToServerId.InterestPlayer);
        ushort observerID = (ushort)(id % 2 + 1);
        message.AddUShort(observerID);
        message.AddBool(true); // Interest only
        message.AddBool(false); // Sound cues
        message.AddBool(false); // Visual cues
        NetworkManager.Singleton.Client.Send(message);
        sentInterest = true;
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
        ushort playerID = message.GetUShort();
        Vector3 position = message.GetVector3();
        Vector3 forward = message.GetVector3();

        if (list.TryGetValue(playerID, out Player player))
        {
            // player.transform.position = TransformCam.Singleton.RelativeRotation * position + TransformCam.Singleton.RelativePos;
            player.transform.position = position;
        }
    }

    [MessageHandler((ushort)ServerToClientId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        ushort playerID = message.GetUShort();
        Vector3 position = message.GetVector3();
        bool isImpaired = message.GetBool();

        if (!list.ContainsKey(playerID))
        {
            Spawn(playerID, position, isImpaired);
            Debug.Log("Spawned player successfully!");
        }
    }

    [MessageHandler((ushort)ServerToClientId.InterestPlayer)]
    private static void InterestPlayer(Message message)
    {
        ushort observerID = message.GetUShort();
        bool interest = message.GetBool();
        bool soundActive = message.GetBool();
        bool visualActive = message.GetBool();

        if (observerID != 0)
        {
            if (interest)
            {
                Debug.Log("Received interest message from player " + observerID);
                NotificationManager.Instance.ShowScreenOverlay(new Color(1, 0, 0.8f, 0.5f));
            }
        }
        else
        {
            Debug.Log("Received server cues settings...");
            if (interest)
            {
                // Show screen overlay with color
                Debug.Log("Showing animation...");
                NotificationManager.Instance.ShowScreenOverlay(new Color(1, 0, 0.8f, 0.5f));
            }

            foreach (var player in list.Values)
            {
                if (player.isImpaired && player.isLocal)
                {
                    HandleSoundCue(player, soundActive);
                    HandleVisualCue(player, visualActive);
                }
            }
        }
    }

    private static void HandleSoundCue(Player player, bool soundActive)
    {
        if (player.isImpaired && player.isLocal)
        {
            ushort observerID = (ushort)(player.id % 2 + 1);
            if (list.TryGetValue(observerID, out Player observer))
            {
                AudioSource audioSource = observer.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.mute = !soundActive;
                    Debug.Log(soundActive ? "Enabled other player's sound." : "Disabled other player's sound.");
                }
            }
        }
    }

    private static void HandleVisualCue(Player player, bool visualActive)
    {
        if (player.isImpaired && player.isLocal)
        {
            ushort observerID = (ushort)(player.id % 2 + 1);
            if (list.TryGetValue(observerID, out Player observer))
            {
                LineRenderer lineRenderer = observer.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.enabled = visualActive;
                    Debug.Log(visualActive ? "Enabled line renderer." : "Disabled line renderer.");
                }
            }
        }
    }
}