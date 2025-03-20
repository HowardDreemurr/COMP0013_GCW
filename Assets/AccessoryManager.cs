using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;

public class AccessoryManager : MonoBehaviour
{
    public RoomClient RoomClient { get; private set; }

    public NetworkSpawner headSpawner;
    public NetworkSpawner neckSpawner; // TODO
    public NetworkSpawner backSpawner; // TODO
    public NetworkSpawner faceSpawner; // TODO

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

    public void AttachRandomHat(Ubiq.Avatars.Avatar avatar)
    {
        if (headCatalogue.prefabs == null || headCatalogue.prefabs.Count == 0 || avatar == null)
        {
            return;
        }

        var idx = Random.Range(0, headCatalogue.prefabs.Count);
        GameObject randomHatPrefab = headCatalogue.prefabs[idx];

        GameObject newHat = headSpawner.SpawnWithPeerScope(randomHatPrefab);
        newHat.name = "NetworkHat";

        HatNetworkedObject newHatNetObj = newHat.GetComponent<HatNetworkedObject>();
        if (newHatNetObj != null)
        {
            newHatNetObj.accessoryManager = this;
            newHatNetObj.collisionsEnabled = false;
            newHatNetObj.AttachHat(avatar);
            newHatNetObj.idx = idx;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat.");
        }
    }

    public void SpawnRandomHat()
    {
        if (headCatalogue.prefabs == null || headCatalogue.prefabs.Count == 0)
        {
            Debug.Log("SpawnRandomHat call invalid");
            return;
        }

        var idx = Random.Range(0, headCatalogue.prefabs.Count);
        GameObject randomHatPrefab = headCatalogue.prefabs[idx];

        GameObject newHat = headSpawner.SpawnWithPeerScope(randomHatPrefab);
        newHat.transform.localPosition = new Vector3(1, 3, 1);
        newHat.transform.localRotation = Quaternion.identity;
        newHat.name = "NetworkHat";

        HatNetworkedObject hatNetworkedObject = newHat.GetComponent<HatNetworkedObject>();
        if (hatNetworkedObject != null)
        {
            hatNetworkedObject.accessoryManager = this;
            hatNetworkedObject.collisionsEnabled = true;
            hatNetworkedObject.idx = idx;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat.");
        }
    }

    public HatNetworkedObject SpawnHat(int idx, bool collisions)
    {
        if (idx < 0 || headCatalogue.prefabs == null || idx > (headCatalogue.prefabs.Count-1))
        {
            Debug.Log("SpawnHat call invalid");
            return null;
        }

        GameObject hatPrefab = headCatalogue.prefabs[idx];

        GameObject hat = headSpawner.SpawnWithPeerScope(hatPrefab);
        hat.transform.localPosition = new Vector3(1, 3, 1);
        hat.transform.localRotation = Quaternion.identity;
        hat.name = "NetworkHat";

        HatNetworkedObject hatNetworkedObject = hat.GetComponent<HatNetworkedObject>();
        if (hatNetworkedObject != null)
        {
            hatNetworkedObject.accessoryManager = this;
            hatNetworkedObject.collisionsEnabled = collisions;
            hatNetworkedObject.idx = idx;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat.");
        }

        return hatNetworkedObject;
    }
}
