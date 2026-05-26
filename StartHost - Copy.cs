using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class StartHost : MonoBehaviour
{
    private int _spawnIndex = 0;
    private Vector3[] _spawnPoints = new Vector3[] {
        new Vector3(26f, 1.2f, 40f),
        new Vector3(28f, 1.2f, 40f),
        new Vector3(30f, 1.2f, 40f),
    };

    private void Awake()
    {
        Debug.Log("AWAKE: StartHost script is alive on " + gameObject.name);
        Inisialisasi();
    }

    private void Inisialisasi()
    {
        Debug.Log("START: Mencoba inisialisasi Network...");

        if (Lobbys.Role.IsRelayHost)
        {
            Debug.Log("Status: Host Terdeteksi. Menyiapkan Relay & Start Host...");
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            if (GameSessionManager.Instance != null && DataManager.instance != null)
            {
                GameSessionManager.Instance.ClientGenders[NetworkManager.Singleton.LocalClientId] = DataManager.instance.Gender;
            }

            Debug.Log("Netcode: Host Started!");
        }
        else if (!string.IsNullOrEmpty(Lobbys.Role.JoinRelayServerIp))
        {
            Debug.Log("Status: Client Terdeteksi. Menyiapkan Relay Join...");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                Lobbys.Role.JoinRelayServerIp,
                Lobbys.Role.JoinRelayServerPort,
                Lobbys.Role.JoinAllocationId,
                Lobbys.Role.JoinAllocationKey,
                Lobbys.Role.JoinConnectionData,
                Lobbys.Role.JoinHostConnectionData
            );

            string gender = DataManager.instance.Gender;
            byte[] payload = Encoding.UTF8.GetBytes(gender);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;
            NetworkManager.Singleton.StartClient();
            Debug.Log("StartClient dipanggil!");
        }
        else
        {
            Debug.LogWarning("Data Relay kosong, StartHost biasa (Local)...");
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }
    }

    public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string gender = System.Text.Encoding.UTF8.GetString(request.Payload);
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.ClientGenders[request.ClientNetworkId] = gender;
        }
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        float offset = (float)(clientId * 3);
        return new Vector3(26f + offset, 1.2f, 40f);
    }
}