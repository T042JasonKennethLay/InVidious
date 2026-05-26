using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class Lobbys : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float Timer;
    private float listRefreshTimer = 3f;
    private float lobbyUpdateTimer;
    private string playerName;

    public GameObject panelLobby;
    public GameObject panelMenu;
    public GameObject[] lobbySlots;
    public TextMeshProUGUI[] slotNames;
    public TextMeshProUGUI[] slotCounts;
    public TextMeshProUGUI lobbyname;
    public TextMeshProUGUI playerCount;
    public TextMeshProUGUI lobbycode;
    public TMP_InputField code;
    public TMP_InputField lobbyNameInput;

    public GameObject[] playerCharacter;
    public TextMeshProUGUI[] playerNameTag;
    public TextMeshProUGUI dataManagerNama;

    public static bool isMultiplayer = false;

    private bool isHost = false;
    private bool isLoadingScene = false;
    private float lobbyUpdateTimerMax = 3.1f;
    public PassDataSource passDataScript;

    private void UpdateLobbyUI(Lobby lobby)
    {
        lobbyname.text = lobby.Name;
        playerCount.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        panelLobby.SetActive(false);
        panelMenu.SetActive(false);
    }

    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Uninitialized)
        {
            Debug.Log("Services sudah init, skip");
            return;
        }

        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Services initialized");

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Init failed: " + e.Message);
        }
    }

    public GameObject warningCanvas;
    public async void StartGame()
    {

        if (joinedLobby == null || !isHost || isLoadingScene) return;
        if (joinedLobby.Players.Count < 2)
        {
            if (warningCanvas != null) warningCanvas.SetActive(true);
            return;
        }

        isLoadingScene = true;
        List<ulong> allClients = new List<ulong>();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            allClients.Add(clientId);
        }
        List<ulong> selectedGhosts = new List<ulong>();
        bool isTwoGhostMode = joinedLobby.Data.ContainsKey("GameMode") && joinedLobby.Data["GameMode"].Value == "2 Ghost";

        int ghostCount = (isTwoGhostMode && allClients.Count >= 3) ? 2 : 1;

        for (int i = 0; i < ghostCount; i++)
        {
            int randomIndex = Random.Range(0, allClients.Count);
            selectedGhosts.Add(allClients[randomIndex]);
            allClients.RemoveAt(randomIndex);
        }

        DataManager.instance.GhostClientIds = selectedGhosts;
        DataManager.instance.Nama = dataManagerNama.text;

        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Started", DataObject.IndexOptions.S1) }
            }
            });

            Debug.Log("Lobby Started! Memindahkan semua player ke Gameplay...");
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gagal Start Game: " + e.Message);
            isLoadingScene = false;
        }
    }
    public static class Role
    {
        public static bool IsHost = false;
        public static bool IsMultiplayer = false;

        public static bool IsRelayHost = false;
        public static string RelayJoinCode = "";
        public static byte[] AllocationId;
        public static byte[] AllocationKey;
        public static byte[] ConnectionData;
        public static string RelayServerIp = "";
        public static ushort RelayServerPort;

        public static byte[] JoinAllocationId;
        public static byte[] JoinAllocationKey;
        public static byte[] JoinConnectionData;
        public static byte[] JoinHostConnectionData;
        public static string JoinRelayServerIp = "";
        public static ushort JoinRelayServerPort;
    }

    public async void CreateLobby()    
    {
        playerName = dataManagerNama.text;
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Role.RelayJoinCode = relayJoinCode;
            Role.IsRelayHost = true;
            Role.AllocationId = allocation.AllocationIdBytes;
            Role.AllocationKey = allocation.Key;
            Role.ConnectionData = allocation.ConnectionData;
            Role.RelayServerIp = allocation.RelayServer.IpV4;
            Role.RelayServerPort = (ushort)allocation.RelayServer.Port;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "2 Ghost", DataObject.IndexOptions.S1) },
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            string lobbyName = (lobbyNameInput != null && lobbyNameInput.text != "") ? lobbyNameInput.text : "Sigma";
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 5, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            isHost = true;

            InvokeRepeating(nameof(RefreshRelayCode), 300f, 300f);

            UpdateLobbyUI(lobby);
            lobbycode.text = relayJoinCode;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                Role.RelayServerIp,
                Role.RelayServerPort,
                Role.AllocationId,
                Role.AllocationKey,
                Role.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
            Debug.Log("Netcode Host Started seseat setelah Create Lobby!");
            UpdateLobbyUI(lobby);
        }
        catch (System.Exception e) { Debug.Log(e); }
    }

    private async void RefreshRelayCode()
    {
        if (hostLobby == null) return;

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Role.RelayJoinCode = relayJoinCode;
            Role.AllocationId = allocation.AllocationIdBytes;
            Role.AllocationKey = allocation.Key;
            Role.ConnectionData = allocation.ConnectionData;
            Role.RelayServerIp = allocation.RelayServer.IpV4;
            Role.RelayServerPort = (ushort)allocation.RelayServer.Port;

            await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            }
            });

            Debug.Log("[RELAY] Code refreshed: " + relayJoinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[RELAY] Refresh failed: " + e.Message);
        }
    }

    private async void LobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            Timer -= Time.deltaTime;
            if (Timer < 0f)
            {
                float TimerMax = 30;
                Timer = TimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null && !isLoadingScene)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                try
                {
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    playerCount.text = joinedLobby.Players.Count + "/" + joinedLobby.MaxPlayers;

                    for (int i = 0; i < playerCharacter.Length; i++)
                    {
                        if (playerCharacter[i] == null) continue;
                        if (i < joinedLobby.Players.Count)
                        {
                            playerCharacter[i].SetActive(true);

                            string nama = "Joining...";
                            if (joinedLobby.Players[i].Data != null &&
                                joinedLobby.Players[i].Data.ContainsKey("Player Name"))
                            {
                                nama = joinedLobby.Players[i].Data["Player Name"].Value;
                            }

                            if (playerNameTag[i] != null)
                                playerNameTag[i].text = nama;
                        }
                        else
                        {
                            playerCharacter[i].SetActive(false);
                            if (playerNameTag[i] != null)
                                playerNameTag[i].text = "";
                        }
                    }
                    if (joinedLobby.Data != null && joinedLobby.Data.ContainsKey("GameMode") && joinedLobby.Data["GameMode"].Value == "Started")
                    {
                        if (!isHost) isLoadingScene = true;
                    }
                }
                catch (LobbyServiceException e) { Debug.Log(e); }
            }
        }
    }

    private async void JoinRelayAndStartClient(string relayCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            if (passDataScript != null)
            {
                passDataScript.SaveDataSebelumPindahScene();
                Debug.Log("[LOBBY] Berhasil memanggil SaveData dari PassDataSource");
            }

            string gender = DataManager.instance.Gender;
            DataManager.instance.Nama = dataManagerNama.text;
            Debug.Log($"[CLIENT] Nama disimpan ke DataManager: '{DataManager.instance.Nama}'");
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(gender);

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started with gender: " + gender);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gagal Join Relay: " + e.Message);
            isLoadingScene = false;
        }
    }

    public async void ListLobby()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1,"2 Ghost",QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }

            };


            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies Found : " + queryResponse.Results.Count);
            for (int i = 0; i < lobbySlots.Length; i++)
            {
                lobbySlots[i].SetActive(false);
            }
            for (int i = 0; i < queryResponse.Results.Count && i < lobbySlots.Length; i++)
            {
                lobbySlots[i].SetActive(true);
                slotNames[i].text = queryResponse.Results[i].Name;
                slotCounts[i].text = queryResponse.Results[i].Players.Count + "/" + queryResponse.Results[i].MaxPlayers;

                string lobbyId = queryResponse.Results[i].Id;
                Button btn = lobbySlots[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => JoinLobbyById(lobbyId));
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbycode)
    {

        try
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbycode,new JoinLobbyByCodeOptions { Player = GetPlayer() });joinedLobby = lobby;

            string relayJoinCode = joinedLobby.Data["RelayCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            Role.JoinAllocationId = joinAllocation.AllocationIdBytes;
            Role.JoinAllocationKey = joinAllocation.Key;
            Role.JoinConnectionData = joinAllocation.ConnectionData;
            Role.JoinHostConnectionData = joinAllocation.HostConnectionData;
            Role.JoinRelayServerIp = joinAllocation.RelayServer.IpV4;
            Role.JoinRelayServerPort = (ushort)joinAllocation.RelayServer.Port;

            PrintPlayer(lobby);
            UpdateLobbyUI(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void Update()
    {
        LobbyHeartBeat();
        HandleLobbyPollForUpdates();

        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn)
        {
            listRefreshTimer -= Time.deltaTime;
            if (listRefreshTimer <= 0f)
            {
                listRefreshTimer = 10f;
                ListLobby();
            }
        }
    }
    private bool isJoining = false;

    [SerializeField] private GameObject playerGui;
    public async void QuickJoinLobby()
    {
        if (isJoining) return;
        isJoining = true;
        playerName = dataManagerNama.text;

        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            string relayJoinCode = joinedLobby.Data["RelayCode"].Value;
            JoinRelayAndStartClient(relayJoinCode);

            UpdateLobbyUI(joinedLobby);
            playerGui.SetActive(true);
            lobbycode.text = relayJoinCode;

            isJoining = false;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            isJoining = false;
        }
    }

    private Player GetPlayer()
    {
        string nama = dataManagerNama != null ? dataManagerNama.text : playerName;
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>{
            {"Player Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nama)}
        }
        };
    }

    private void PrintPlayer()
    {
        PrintPlayer(joinedLobby);
    }

    private void PrintPlayer(Lobby lobby)
    {
        Debug.Log("=== PLAYER LIST ===");
        foreach (Player player in lobby.Players)
        {
            if (player == null)
            {
                Debug.LogError("Player NULL");
                continue;
            }

            string playerNameValue = "No Name";
            if (player.Data != null && player.Data.ContainsKey("Player Name"))
            {
                playerNameValue = player.Data["Player Name"].Value;
            }
            Debug.Log("PlayerID: " + player.Id + " Name: " + playerNameValue);
            Debug.Log(lobby.Data["GameMode"].Value);
        }
    }

    public  async void UpdateLobbyGameMode(string gameMode)
    {
        try {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public ,gameMode)}
            }
            });
            joinedLobby = hostLobby;
            PrintPlayer(hostLobby);

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
             await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {"Player Name",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member ,playerName)}
            }
            });
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        isMultiplayer = false;
        for (int i = 0; i < playerCharacter.Length; i++)
            playerCharacter[i].SetActive(false);

        try
        {
            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
        }
        catch (LobbyServiceException e) { Debug.Log(e); }

        CancelInvoke(nameof(RefreshRelayCode));
        joinedLobby = null; 
        hostLobby = null;
        isHost = false;
        panelLobby.SetActive(true);
        panelMenu.SetActive(false);
        playerGui.SetActive(false);
    }

    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void PromoteHostLobbi()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;
            PrintPlayer(hostLobby);


        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobbyById(string lobbyId)
    {
        if (isJoining) return;
        isJoining = true;

        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions { Player = GetPlayer() };
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

            string relayJoinCode = joinedLobby.Data["RelayCode"].Value;
            JoinRelayAndStartClient(relayJoinCode);

            UpdateLobbyUI(joinedLobby);
            playerGui.SetActive(true);
            lobbycode.text = relayJoinCode;
            isJoining = false;
        }
        catch (System.Exception e)
        {
            Debug.Log(e); isJoining = false;
        }
    }

    [SerializeField] private GameObject joinpopout;
    public void JoinByCodeButton()
    {
        JoinLobbyByCode(code.text);
        joinpopout.SetActive(false);
        playerGui.SetActive(true);
    }
}
