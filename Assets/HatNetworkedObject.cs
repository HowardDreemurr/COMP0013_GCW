using UnityEngine;
using System.Collections;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Windows;
using static Ubiq.Avatars.AvatarInput;

// NB: This is called 'HatNetworkedObject' as a holdover, it can be thrown onto any accessory, not just hats
public class HatNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public bool collisionsEnabled;
    public Rigidbody rb;
    public BoxCollider bc;
    public BoxCollider triggerCollider;
    public AccessoryManager accessoryManager; // This is the accessoryManager that spawned this 'hat'
    public int idx;

    public AccessorySlot slot;

    public GameObject AudioPrefab;

    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Vector3 initTranslation;
    private Quaternion initRotation;

    private bool physicsOwner;
    private bool isParented;

    private enum CollisionState
    {
        Unset,
        Enabled,
        Disabled
    }

    void Awake()
    {
        // Store the initial transform values when the GameObject is first instantiated
        initTranslation = transform.position;
        initRotation = transform.rotation;

        // Add or retrieve the Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Add or retrieve the main BoxCollider
        bc = GetComponent<BoxCollider>();
        if (bc == null)
        {
            bc = gameObject.AddComponent<BoxCollider>();
        }

        // Create a separate trigger collider
        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size *= 1.2f;

        // Grabbing
        XRGrabInteractable grab = gameObject.GetComponent<XRGrabInteractable>();
        if (!grab)
        {
            grab = gameObject.AddComponent<XRGrabInteractable>();
        }
        grab.selectEntered.AddListener((SelectEnterEventArgs args) =>
        {
            Debug.Log("Hat was selected!");
            DisablePhysics(null);
            physicsOwner = true;
        });
        grab.selectExited.AddListener((SelectExitEventArgs args) =>
        {
            Debug.Log("Hat was dropped!");
            EnablePhysics();
            physicsOwner = false;
        });

        // Disable physics locally
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    private void Start()
    {
        // Register with the network scene
        context = NetworkScene.Register(this);

        // If collisionsEnabled is true on my client, it must've been explicitly set after construction
        // Let's tell everyone else, since they spawned the hat through a different code path (NetworkSpawner) that won't mirror this
        if (collisionsEnabled)
        {
            EnablePhysics();
            physicsOwner = true;
        }

        // Initialize our last known position and rotation
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        Debug.Log("initTranslation " + initTranslation);
        Debug.Log("initRotation " + initRotation);
    }

    private void Update()
    {
        if ((transform.position != lastPosition || transform.rotation != lastRotation))
        {
            lastPosition = transform.position + initTranslation;
            lastRotation = transform.rotation * initRotation;

            if (physicsOwner)
            {
                context.SendJson(new HatMessage
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    collisions = CollisionState.Unset,
                    parentNameOrId = ""
                });
            }
        }
    }

    // TODO: I should put an enumerator on this class specifying slot (hat, back, etc.) and switch behaviour here appropriately
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HatNetworkedObject.OnTriggerEnter(Collider other) was called");

        if (collisionsEnabled)
        {
            Ubiq.Avatars.Avatar avatar = other.GetComponentInParent<Ubiq.Avatars.Avatar>();

            if (avatar != null)
            {
                AttachHat(avatar, slot);
            }
        }
    }

    public void AttachHat(Ubiq.Avatars.Avatar avatar, AccessorySlot arg_slot)
    {
        FloatingAvatar floatingAvatar = avatar.GetComponentInChildren<FloatingAvatar>();
        if (floatingAvatar == null)
        {
            Debug.LogWarning("FloatingAvatar component or head transform not found on avatar");
            return;
        }

        Transform avatarTransform;
        Transform existingAccessory;
        switch (arg_slot)
        {
            case AccessorySlot.Head:
                avatarTransform = floatingAvatar.head;
                existingAccessory = avatarTransform.Find("NetworkHead");
                break;
            case AccessorySlot.Neck:
                avatarTransform = floatingAvatar.head;
                existingAccessory = avatarTransform.Find("NetworkNeck");
                break;
            case AccessorySlot.Back:
                avatarTransform = floatingAvatar.torso;
                existingAccessory = avatarTransform.Find("NetworkBack");
                break;
            case AccessorySlot.Face:
                avatarTransform = floatingAvatar.head;
                existingAccessory = avatarTransform.Find("NetworkFace");
                break;
            default:
                return;
        }

        if (avatarTransform == null)
        {
            Debug.LogWarning("Transform not found on avatar");
            return;
        }

        // Retrieve the singleton AccessoryManager (idk if it's actually a singleton but it's design-time and there's only one)
        AccessoryManager localAccessoryManager = FindFirstObjectByType<AccessoryManager>();
        if (localAccessoryManager == null)
        {
            Debug.LogWarning("Local AccessoryManager not found in the scene");
            return;
        }

        // Choose the correct spawner based on the accessory slot
        NetworkSpawner spawner = null;
        switch (arg_slot)
        {
            case AccessorySlot.Head:
                spawner = localAccessoryManager.headSpawner;
                break;
            case AccessorySlot.Neck:
                spawner = localAccessoryManager.neckSpawner;
                break;
            case AccessorySlot.Back:
                spawner = localAccessoryManager.backSpawner;
                break;
            case AccessorySlot.Face:
                spawner = localAccessoryManager.faceSpawner;
                break;
        }

        // If an existing accessory is found (and it isn't this hat itself), despawn it
        if (existingAccessory != null && existingAccessory != transform && spawner != null)
        {
            Debug.Log("Found an existing accessory attached to the avatar");
            spawner.Despawn(existingAccessory.gameObject);
        }

        // Parent this hat to the avatar's transform and reset its transform values to the axis conversion ones
        transform.SetParent(avatarTransform, false);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localPosition = initTranslation;
        transform.localRotation = initRotation;

        // Extract UUID from avatar name
        string[] parts = avatar.name.Split(' ');
        string parentAvatarId = parts[parts.Length - 1];
        DisablePhysics(parentAvatarId);
        isParented = true;

        Debug.Log("Hat attached to " + avatar.name);

        SpawnEffects(AudioPrefab, avatar.GetComponentInChildren<FloatingAvatar>().torso.position);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<HatMessage>();

        if (!physicsOwner)
        {
            transform.position = msg.position;
            transform.rotation = msg.rotation;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        if (msg.collisions == CollisionState.Enabled && !collisionsEnabled)
        {
            EnablePhysics();
            physicsOwner = false;
        }
        else if (msg.collisions == CollisionState.Disabled && collisionsEnabled)
        {
            DisablePhysics(null);
            physicsOwner = false;
        }

        if (!string.IsNullOrEmpty(msg.parentNameOrId) && !isParented)
        {
            // NOTE: To unparent the hat, I guess you could send a non-null transform which is just the root of the scene?
            GameObject parentObj = GameObject.Find("Remote Avatar " + msg.parentNameOrId);
            if (parentObj == null)
            {
                // Try the same UUID but with "My Avatar #<UUID>", since it could be us
                parentObj = GameObject.Find("My Avatar " + msg.parentNameOrId);
            }

            if (parentObj != null)
            {
                Debug.Log("Attempting to parent Hat to " + parentObj.name);

                Ubiq.Avatars.Avatar parentAvatar = parentObj.GetComponentInChildren<Ubiq.Avatars.Avatar>();

                if (parentAvatar != null)
                {
                    AttachHat(parentAvatar, slot);
                }
                else
                {
                    Debug.Log("Found GameObject but could not find child Avatar component");
                }
            }
            else
            {
                Debug.Log("Could not find parent with UUID: " + msg.parentNameOrId);
            }
            
        }
    }

    public void DisablePhysics(string parentAvatarId)
    {
        Debug.Log("Disabling Physics (parent = " + parentAvatarId + ")");

        collisionsEnabled = false;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        context.SendJson(new HatMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            collisions = CollisionState.Disabled,
            parentNameOrId = parentAvatarId
        });
    }

    public void EnablePhysics()
    {
        Debug.Log("Enabling Physics");

        collisionsEnabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        context.SendJson(new HatMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            collisions = CollisionState.Enabled,
            parentNameOrId = ""
        });
    }

    private struct HatMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public CollisionState collisions;
        public string parentNameOrId;
    }

    private void SpawnEffects(GameObject prefab, Vector3 position)
    {
        if (prefab)
        {
            var instance = NetworkSpawnManager.Find(this).SpawnWithPeerScope(prefab);

            instance.transform.position = position;
        }
    }
}
