using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;

public class AccessoryManager : MonoBehaviour
{
    public RoomClient RoomClient { get; private set; }
    private AvatarManager avatarManager;
    private NetworkSpawner spawner;

    public PrefabCatalogue accessoryCatalogue;

    private void Awake()
    {
        RoomClient = GetComponentInParent<RoomClient>();
    }

    private void Start()
    {
        spawner = new NetworkSpawner(NetworkScene.Find(this), RoomClient, accessoryCatalogue, "ubiq.accessory.");

        var networkScene = NetworkScene.Find(this);
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
    }

    public void AttachRandomHat(Ubiq.Avatars.Avatar avatar)
    {
        // Verify valid inputs.
        if (accessoryCatalogue.prefabs == null || accessoryCatalogue.prefabs.Count == 0 || avatar == null)
        {
            return;
        }

        // Get the FloatingAvatar component that holds the head reference.
        FloatingAvatar floatingAvatar = avatar.GetComponentInChildren<FloatingAvatar>();
        if (floatingAvatar == null || floatingAvatar.head == null)
        {
            Debug.LogWarning("FloatingAvatar component or head transform not found on avatar");
            return;
        }
        Transform headTransform = floatingAvatar.head;

        // Remove any previously attached hat using network despawn.
        Transform existingHat = headTransform.Find("NetworkHat");
        if (existingHat != null)
        {
            // Use the NetworkSpawner's despawn to remove the networked object.
            spawner.Despawn(existingHat.gameObject);
        }

        // Select a random hat prefab.
        var idx = Random.Range(0, accessoryCatalogue.prefabs.Count);
        GameObject randomHatPrefab = accessoryCatalogue.prefabs[idx];

        // Spawn the hat as a networked object.
        GameObject newHat = spawner.SpawnWithPeerScope(randomHatPrefab);
        
        newHat.name = "NetworkHat"; // so we can easily find and remove it later

        // Parent the hat to the head transform.
        newHat.transform.SetParent(headTransform, false);
        newHat.transform.localPosition = Vector3.zero;
        newHat.transform.localRotation = Quaternion.identity;

        // Disable any physics so it stays fixed to the head.
        Rigidbody rb = newHat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Disable the Collider
        Collider col = newHat.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}
