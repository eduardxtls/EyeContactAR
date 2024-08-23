using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public Vector3 Position { get; set; }
    public bool isImpaired;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, Vector3 position, bool isImpaired)
    {
        // Notify other players
        foreach (Player otherPlayer in list.Values)
        {
            if (otherPlayer.Id != id)
            {
                otherPlayer.SendSpawned(id);
            }
        }
           
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<Player>();
        player.Id = id;
        player.Position = position;
        player.isImpaired = isImpaired;
        player.SendSpawned();
        list.Add(id, player);

        if (player.isImpaired)
        {
            Debug.Log("Player " + player.Id + " spawned as visually impaired player.");
        }
        else
        {
            Debug.Log("Player " + player.Id + " spawned as normal-sighted player.");
        }
        
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.SpawnPlayer)));
    }

    public void SendSpawned(ushort toClient)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.SpawnPlayer)), toClient);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddVector3(new Vector3(0, 0, 0));
        message.AddBool(this.isImpaired);
        return message;
    }


    [MessageHandler((ushort)ClientToServerId.SpawnPlayer)]
    private static void SpawnPlayer(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetVector3(), message.GetBool());
    }

    [MessageHandler((ushort)ClientToServerId.MovePlayer)]
    private static void MovePlayer(ushort fromClientId, Message message)
    {
        ushort playerId = message.GetUShort();
        Vector3 newPosition = message.GetVector3();
        Vector3 forwardDirection = message.GetVector3();

        if (list.TryGetValue(playerId, out Player player))
        {
            player.Position = newPosition;
            player.transform.position = newPosition;

            if (forwardDirection.magnitude >= Mathf.Epsilon)
            {
                player.transform.forward = forwardDirection.normalized;
            }

            // Broadcast the movement to all other clients
            Message moveMessage = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.MovePlayer);
            moveMessage.AddUShort(playerId);
            moveMessage.AddVector3(newPosition);
            moveMessage.AddVector3(forwardDirection);

            // Notify other players
            foreach (Player otherPlayer in list.Values)
            {
                if (otherPlayer.Id != fromClientId)
                {
                    NetworkManager.Singleton.Server.Send(moveMessage, otherPlayer.Id);
                }
            }
        }
    }

    [MessageHandler((ushort)ClientToServerId.InterestPlayer)]
    private static void InterestPlayer(ushort fromClientId, Message message)
    {
        ushort fromId = message.GetUShort();
        bool interest = message.GetBool();
        bool soundActive = message.GetBool();
        bool visualActive = message.GetBool();
        Player player = list[fromId];

        Debug.Log("Received interest message from player " + fromId + " for other player.");

        // If sighted player
        if (!player.isImpaired)
        {
            ushort otherPlayerID = (ushort)(fromId % 2 + 1);

            // Send message to other player
            if (list.TryGetValue(otherPlayerID, out Player otherPlayer) && otherPlayer.isImpaired)
            {
                Message interestMessage = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.InterestPlayer);
                interestMessage.AddUShort(fromId);
                interestMessage.AddBool(interest);
                interestMessage.AddBool(soundActive);
                interestMessage.AddBool(visualActive);

                // Notify other player
                NetworkManager.Singleton.Server.Send(interestMessage, otherPlayerID);
            }
        }
    }
    #endregion
}