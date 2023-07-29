using System.Collections;
using System.Collections.Generic;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using QFSW;
using UnityEngine;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Command]
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
            return null;
        }
    }

    /// <summary>
    /// Returns bool if the player joined or not.
    /// </summary>
    /// <param name="joinCode"></param>
    /// <returns></returns>
    public async Task<bool> JoinRelay(string joinCode)
    {
        bool Joined;
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("Joined relay with code: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            Joined = true;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
            Joined = false;
        }

        return Joined;
    }
}
