using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Ubiq.Avatars;
using Ubiq.Rooms;
using Ubiq.Messaging;
using Ubiq.Dictionaries;
using Ubiq.Spawning;
using System.Collections;

public class RemoteAvatarInteractableAttacher : MonoBehaviour
{
    private AvatarManager avatarManager;
    private NetworkSpawner spawner;
    public PrefabCatalogue avatarCatalogue; // This will just contain Floating Avatar
    public RoomClient RoomClient { get; private set; }

    private void Awake()
    {
        RoomClient = GetComponentInParent<RoomClient>();
    }

    private void Start()
    {
        avatarManager = FindFirstObjectByType<AvatarManager>();
        if (avatarManager != null)
        {
            avatarManager.OnAvatarCreated.AddListener(OnAvatarCreated);
        }
        else
        {
            Debug.LogWarning("No AvatarManager found in scene.");
        }

        spawner = new NetworkSpawner(NetworkScene.Find(this), RoomClient, avatarCatalogue, "ubiq.fakeavatars.");
    }

    private void OnDestroy()
    {
        if (avatarManager != null)
        {
            avatarManager.OnAvatarCreated.RemoveListener(OnAvatarCreated);
        }
    }

    private void OnAvatarCreated(Ubiq.Avatars.Avatar avatar)
    {
        if (!avatar.IsLocal)
        {
            Debug.Log("Adding interactable to " + avatar.name + "...");
            Transform head = avatar.transform.Find("Body/Floating_Head");
            if (head == null)
            {
                Debug.LogWarning("Could not find head for avatar: " + avatar.name);
                return;
            }

            UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = head.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (interactable == null)
            {
                Debug.Log("\tAdding XRSimpleInteractable...");
                interactable = head.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            }

            BoxCollider boxCollider = head.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.Log("\tAdding head BoxCollider...");
                boxCollider = head.gameObject.AddComponent<BoxCollider>();
            }
            boxCollider.isTrigger = false; // Unfortunately it needs to have physical collisions to be clicked on...

            if (!interactable.colliders.Contains(boxCollider))
            {
                Debug.Log("\tAdding BoxCollider to XRSimpleInteractable colliders...");
                interactable.colliders.Add(boxCollider);
            }

            XRPokeFilter pokeFilter = head.GetComponent<XRPokeFilter>();
            if (pokeFilter == null)
            {
                Debug.Log("\tAdding XRPokeFilter...");
                pokeFilter = head.gameObject.AddComponent<XRPokeFilter>();
            }
            pokeFilter.pokeInteractable = interactable;
            pokeFilter.pokeCollider = boxCollider;

            XRPokeFollowAffordance pokeFollowAffordance = head.GetComponent<XRPokeFollowAffordance>();
            if (pokeFollowAffordance == null)
            {
                Debug.Log("\tAdding XRPokeFollowAffordance...");
                pokeFollowAffordance = head.gameObject.AddComponent<XRPokeFollowAffordance>();
            }
            pokeFollowAffordance.pokeFollowTransform = head;

            Debug.Log("\tAdding selectEntered listener...");
            interactable.selectEntered.AddListener(OnHeadSelectEntered);

            // I don't get Unity. Why does this help
            interactable.enabled = false;
            interactable.enabled = true;
        }
    }

    private void OnHeadSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("Selected head");
        var interactableObject = args.interactableObject.transform; // Floating_Head
        var interactorObject = args.interactorObject.transform; // Near-Far Interactor

        TexturedAvatar texturedAvatar = interactableObject.GetComponentInParent<TexturedAvatar>();
        if (texturedAvatar == null)
        {
            Debug.Log("\tCouldnt find TexturedAvatar");
            return;
        }

        Texture2D avatarTexture = texturedAvatar.GetTexture();
        if (avatarTexture == null)
        {
            Debug.Log("\tCouldnt find Texture2D on TexturedAvatar");
            return;
        }

        Debug.Log("\tSpawning fake avatar");
        var fakeAvatar = spawner.SpawnWithPeerScope(avatarCatalogue.prefabs[0]);

        Renderer renderer = fakeAvatar.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = avatarTexture;
        }
        else
        {
            Debug.Log("\tNo Renderer found on the fake avatar");
        }

        StartCoroutine(syncFakeAvatarHeadState(fakeAvatar, avatarTexture));
    }

    private IEnumerator syncFakeAvatarHeadState(GameObject fakeAvatar, Texture2D avatarTexture)
    {
        yield return new WaitForSeconds(0.2f); // Arbitrary delay (wait for it to spawn on other clients)

        FakeAvatarHead fakeAvatarHead = fakeAvatar.GetComponentInChildren<FakeAvatarHead>();
        if (fakeAvatarHead != null)
        {
            fakeAvatarHead.syncState(avatarTexture);
        }
        else
        {
            Debug.LogWarning("FakeAvatarHead component not found on spawned head");
        }
    }
}
