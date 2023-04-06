using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class initializePlayers : MonoBehaviour
{
    void Start()
    {
        initPlayers();
    }

    private void initPlayers()
    {
        if(testRelay.Instance == null)
        {
            Debug.LogError("NO RELAY, TESTING AGAIN");
            Invoke("initPlayers", 1);
            return;
        }

        if (testRelay.Instance.isHost)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
    }
}
