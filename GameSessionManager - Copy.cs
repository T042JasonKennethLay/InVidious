using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameSessionManager : NetworkBehaviour
{
    public static GameSessionManager Instance;
    public List<ulong> GhostClientIds = new List<ulong>();
    public Dictionary<ulong, string> ClientGenders = new Dictionary<ulong, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Instance = this;
        if (DataManager.instance != null)
        {
            GhostClientIds = DataManager.instance.GhostClientIds;
        }
    }

    public bool IsThisPlayerGhost(ulong clientId)
    {
        return GhostClientIds.Contains(clientId);
    }
}