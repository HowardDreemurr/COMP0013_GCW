using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using System.Collections;

public class AccessoryManager : MonoBehaviour
{
    public RoomClient RoomClient { get; private set; }

    public NetworkSpawner headSpawner;
    public NetworkSpawner neckSpawner;
    public NetworkSpawner backSpawner;
    public NetworkSpawner faceSpawner;

    public PrefabCatalogue headCatalogue;
    public PrefabCatalogue neckCatalogue;
    public PrefabCatalogue backCatalogue;
    public PrefabCatalogue faceCatalogue;

    private void Awake()
    {
        RoomClient = GetComponentInParent<RoomClient>();
    }

    private void Start()
    {
        var networkScene = NetworkScene.Find(this);

        headSpawner = new NetworkSpawner(networkScene, RoomClient, headCatalogue, "ubiq.head.");
        neckSpawner = new NetworkSpawner(networkScene, RoomClient, neckCatalogue, "ubiq.neck.");
        backSpawner = new NetworkSpawner(networkScene, RoomClient, backCatalogue, "ubiq.back.");
        faceSpawner = new NetworkSpawner(networkScene, RoomClient, faceCatalogue, "ubiq.face.");
    }

    public void AttachHatOnSpawn(int idx, Ubiq.Avatars.Avatar avatar, AccessorySlot arg_slot)
    {
        PrefabCatalogue catalogue;
        NetworkSpawner spawner;
        string name;
        switch (arg_slot)
        {
            case AccessorySlot.Head:
                catalogue = headCatalogue;
                spawner = headSpawner;
                name = "NetworkHead";
                break;
            case AccessorySlot.Neck:
                catalogue = neckCatalogue;
                spawner = neckSpawner;
                name = "NetworkNeck";
                break;
            case AccessorySlot.Back:
                catalogue = backCatalogue;
                spawner = backSpawner;
                name = "NetworkBack";
                break;
            case AccessorySlot.Face:
                catalogue = faceCatalogue;
                spawner = faceSpawner;
                name = "NetworkFace";
                break;
            default:
                return;
        }

        if (catalogue.prefabs == null || catalogue.prefabs.Count == 0 || avatar == null)
        {
            return;
        }

        GameObject randomHatPrefab = catalogue.prefabs[idx];

        GameObject newHat = spawner.SpawnWithPeerScope(randomHatPrefab);
        newHat.name = name;

        StartCoroutine(attachHatExternal(newHat, avatar, arg_slot, idx));
    }

    public void AttachRandomHat(Ubiq.Avatars.Avatar avatar, AccessorySlot arg_slot)
    {
        PrefabCatalogue catalogue;
        NetworkSpawner spawner;
        string name;
        switch (arg_slot)
        {
            case AccessorySlot.Head: 
                catalogue = headCatalogue;
                spawner = headSpawner;
                name = "NetworkHead";
                break;
            case AccessorySlot.Neck: 
                catalogue = neckCatalogue; 
                spawner = neckSpawner;
                name = "NetworkNeck";
                break;
            case AccessorySlot.Back: 
                catalogue = backCatalogue; 
                spawner = backSpawner;
                name = "NetworkBack";
                break;
            case AccessorySlot.Face: 
                catalogue = faceCatalogue; 
                spawner = faceSpawner;
                name = "NetworkFace";
                break;
            default:
                return;
        }

        if (catalogue.prefabs == null || catalogue.prefabs.Count == 0 || avatar == null)
        {
            return;
        }

        var idx = Random.Range(0, catalogue.prefabs.Count);
        GameObject randomHatPrefab = catalogue.prefabs[idx];

        GameObject newHat = spawner.SpawnWithPeerScope(randomHatPrefab);
        newHat.name = name;

        StartCoroutine(attachHatExternal(newHat, avatar, arg_slot, idx));
    }

    private IEnumerator attachHatExternal(GameObject hat, Ubiq.Avatars.Avatar avatar, AccessorySlot arg_slot, int idx)
    {
        yield return new WaitForSeconds(0.2f); // Ubiq does not give me a way of telling if the hat has spawned for other users, it seems?

        HatNetworkedObject hatNetworkedObject = hat.GetComponent<HatNetworkedObject>();
        if (hatNetworkedObject != null)
        {
            hatNetworkedObject.accessoryManager = this;
            hatNetworkedObject.collisionsEnabled = false;
            hatNetworkedObject.AttachHat(avatar, arg_slot);
            hatNetworkedObject.idx = idx;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat.");
        }
    }

    public void SpawnRandomHat(AccessorySlot arg_slot)
    {
        PrefabCatalogue catalogue;
        NetworkSpawner spawner;
        string name;
        switch (arg_slot)
        {
            case AccessorySlot.Head:
                catalogue = headCatalogue;
                spawner = headSpawner;
                name = "NetworkHead";
                break;
            case AccessorySlot.Neck:
                catalogue = neckCatalogue;
                spawner = neckSpawner;
                name = "NetworkNeck";
                break;
            case AccessorySlot.Back:
                catalogue = backCatalogue;
                spawner = backSpawner;
                name = "NetworkBack";
                break;
            case AccessorySlot.Face:
                catalogue = faceCatalogue;
                spawner = faceSpawner;
                name = "NetworkFace";
                break;
            default:
                return;
        }

        if (catalogue.prefabs == null || catalogue.prefabs.Count == 0)
        {
            Debug.Log("SpawnRandomHat call invalid");
            return;
        }

        var idx = Random.Range(0, catalogue.prefabs.Count);
        GameObject accessoryPrefab = catalogue.prefabs[idx];

        GameObject accessory = spawner.SpawnWithPeerScope(accessoryPrefab);
        accessory.transform.localPosition += new Vector3(0, 3, 4);
        accessory.name = name;

        HatNetworkedObject hatNetworkedObject = accessory.GetComponent<HatNetworkedObject>();
        if (hatNetworkedObject != null)
        {
            hatNetworkedObject.accessoryManager = this;
            hatNetworkedObject.collisionsEnabled = true;
            hatNetworkedObject.idx = idx;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat");
        }
    }

    public HatNetworkedObject SpawnHat(int idx, bool collisions, AccessorySlot arg_slot)
    {
        PrefabCatalogue catalogue;
        NetworkSpawner spawner;
        string name;
        switch (arg_slot)
        {
            case AccessorySlot.Head:
                catalogue = headCatalogue;
                spawner = headSpawner;
                name = "NetworkHead";
                break;
            case AccessorySlot.Neck:
                catalogue = neckCatalogue;
                spawner = neckSpawner;
                name = "NetworkNeck";
                break;
            case AccessorySlot.Back:
                catalogue = backCatalogue;
                spawner = backSpawner;
                name = "NetworkBack";
                break;
            case AccessorySlot.Face:
                catalogue = faceCatalogue;
                spawner = faceSpawner;
                name = "NetworkFace";
                break;
            default:
                return null;
        }

        if (catalogue.prefabs == null || catalogue.prefabs.Count == 0)
        {
            Debug.Log("SpawnRandomHat call invalid");
            return null;
        }

        GameObject accessoryPrefab = catalogue.prefabs[idx];

        GameObject accessory = spawner.SpawnWithPeerScope(accessoryPrefab);
        accessory.transform.localPosition += new Vector3(0, 3, 4);
        accessory.name = name;

        HatNetworkedObject hatNetworkedObject = accessory.GetComponent<HatNetworkedObject>();
        if (hatNetworkedObject != null)
        {
            hatNetworkedObject.accessoryManager = this;
            hatNetworkedObject.collisionsEnabled = collisions;
            hatNetworkedObject.idx = idx;
            return hatNetworkedObject;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat");
            return null;
        }
    }
}
