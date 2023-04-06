using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class setUpOwnerPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject orientation;
    [SerializeField] private Transform firePoint;

    [SerializeField] private GameObject playerGameOb;
    [SerializeField] private GameObject camObject;
    [SerializeField] private GameObject lookObject;
    [SerializeField] private PlayerMovement myPlayerMovement;


    public override void OnNetworkSpawn()
    {
        {
            camObject.SetActive(IsOwner);
            lookObject.SetActive(!IsOwner);

            if (!IsOwner) {
                foreach (Component comp in playerGameOb.GetComponents<Component>())
                {
                    if (!(comp is Transform))
                    {
                        Destroy(comp);
                    }
                }
                //Destroy(spellCall);
                return; 
            }

            
            this.gameObject.tag = "MyPlayer";
            GameObject hotbar = GameObject.FindGameObjectWithTag("HotBarSystem");
            hotbar.GetComponent<hotBarScript>().enabled = true;
            hotbar.GetComponent<hotBarScript>().Init(orientation, firePoint);



            //temp set random spawnpoint
            transform.position = GameObject.FindGameObjectWithTag("SpawnRoom").transform.position;
                
                

            StartCoroutine(waitAndInstWorld());
        }
    }

    IEnumerator waitAndInstWorld()
    {
        yield return null;
        WorldGenerator.Instance.StartGameReal(myPlayerMovement, this);
    }
    public void setReadyForGame()
    {
        Debug.LogError("what am I?: " + IsClient + " " + IsHost + " " + IsServer + " " + IsOwnedByServer);


        if (testRelay.Instance.isHost == false)
        { //if your not the server --> tell server you're ready
            tellServerImReadyServerRpc();
        }
        else// if you're server --> start waiting for players to be ready
        {
            WorldGenerator.Instance.waitingForPlayersToBeReady();
        }
    }

    [ServerRpc]
    void tellServerImReadyServerRpc()
    {
        WorldGenerator.Instance.playerReadyCount += 1;
        Debug.Log("another client ready: " + WorldGenerator.Instance.playerReadyCount);
    }


    public void tellClientToLoad()
    {
        updateClientModifiers();
        startLoadClientRpc();
    }

    [ClientRpc]
    void startLoadClientRpc()
    {
        WorldGenerator.Instance.loadInChunks();
    }


    public void updateClientModifiers()
    {
        updateModsClientRpc(
            PlayerPrefs.GetInt("START_SPELLS"), 
            PlayerPrefs.GetFloat("DAMAGE_MOD"), 
            PlayerPrefs.GetFloat("KNOCKBACK_MOD"), 
            PlayerPrefs.GetFloat("COOLDOWN_MOD")
        );
    }

    [ClientRpc]
    void updateModsClientRpc(int spellStart, float damageMod, float knockbackMod, float cooldownMod)
    {
        PlayerPrefs.SetFloat("KNOCKBACK_MOD", knockbackMod);
        PlayerPrefs.SetFloat("DAMAGE_MOD", damageMod);
        PlayerPrefs.SetFloat("COOLDOWN_MOD", cooldownMod);

        for(var i = 0; i < spellStart; i++)
        {
            UI_spellHander.Instance.addRandomSpell();
        }
    }


    public void callAnRCP_spawnObject(string objectName, Vector3 pos, Quaternion rot)
    {
        InitCraftedPrefabServerRpc(objectName, pos, rot);
    }

    [ServerRpc]
    private void InitCraftedPrefabServerRpc(string spellPrefab, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = Instantiate(Resources.Load("spells/" + spellPrefab)) as GameObject;
        prefab.transform.position = position;
        prefab.transform.rotation = rotation;
        prefab.GetComponent<NetworkObject>().Spawn();
    }


}
