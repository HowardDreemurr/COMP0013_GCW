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

        // Get the FloatingAvatar component that holds the head reference
        FloatingAvatar floatingAvatar = avatar.GetComponentInChildren<FloatingAvatar>();
        if (floatingAvatar == null || floatingAvatar.head == null)
        {
            Debug.LogWarning("FloatingAvatar component or head transform not found on avatar");
            return;
        }
        Transform headTransform = floatingAvatar.head;

        // Remove any previously attached hat using network despawn
        Transform existingHat = headTransform.Find("NetworkHat"); // TODO: Append UUID of avatar
        if (existingHat != null)
        {
            headSpawner.Despawn(existingHat.gameObject);
        }

        // Select a random hat prefab
        var idx = Random.Range(0, headCatalogue.prefabs.Count);
        GameObject randomHatPrefab = headCatalogue.prefabs[idx];

        // Spawn the hat as a networked object
        GameObject newHat = headSpawner.SpawnWithPeerScope(randomHatPrefab);
        newHat.name = "NetworkHat"; // TODO: Append UUID of avatar

        // Parent the hat to the head transform.
        newHat.transform.SetParent(headTransform, false);
        newHat.transform.localPosition = Vector3.zero;
        newHat.transform.localRotation = Quaternion.identity;

        // Try to get the HatNetworkedObject component and disable physics
        HatNetworkedObject hatNetworkedObject = newHat.GetComponent<HatNetworkedObject>();
        if (hatNetworkedObject != null)
        {
            hatNetworkedObject.accessoryManager = this;
            hatNetworkedObject.collisionsEnabled = false;
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat.");
        }

        // TODO: Disable rendering of accessory for avatar it is assigned to
    }

    public void SpawnRandomHat()
    {
        if (headCatalogue.prefabs == null || headCatalogue.prefabs.Count == 0)
        {
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
        }
        else
        {
            Debug.LogWarning("HatNetworkedObject component not found on spawned hat.");
        }
    }
}
