using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Daftar Prefab")]
    [SerializeField] private GameObject tuyulPrefab;
    [SerializeField] private GameObject maleInvestigatorPrefab;
    [SerializeField] private GameObject femaleInvestigatorPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnSemuaPlayer();
        }
    }

    private void SpawnSemuaPlayer()
    {
        GameSessionManager sessionManager = Object.FindFirstObjectByType<GameSessionManager>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject prefabYangAkanDiSpawn;

            if (sessionManager != null && sessionManager.IsThisPlayerGhost(client.ClientId))
            {
                prefabYangAkanDiSpawn = tuyulPrefab;
            }
            else
            {
                string genderPemain = "Male";
                if (sessionManager != null && sessionManager.ClientGenders.ContainsKey(client.ClientId))
                {
                    genderPemain = sessionManager.ClientGenders[client.ClientId];
                }

                if (genderPemain == "Female")
                {
                    prefabYangAkanDiSpawn = femaleInvestigatorPrefab;
                }
                else
                {
                    prefabYangAkanDiSpawn = maleInvestigatorPrefab;
                }
            }

            Vector3 spawnPos = new Vector3(26f + (client.ClientId * 2f), 2f, 40f);
            GameObject playerObj = Instantiate(prefabYangAkanDiSpawn, spawnPos, Quaternion.identity);
            playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
        }
    }
}