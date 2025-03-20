using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

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

    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Vector3 initTranslation;
    private Quaternion initRotation;

    void Awake()
    {
        // Store the initial transform values when the GameObject is first instantiated
        initTranslation = transform.position;
        initRotation = transform.rotation;
    }

    private void Start()
    {
        context = NetworkScene.Register(this);

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        Debug.Log("initTranslation " + initTranslation);
        Debug.Log("initRotation " + initRotation);

        Debug.Log("Adding Rigid Body and Box Collider to hat");
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        if (bc == null)
        {
            bc = gameObject.AddComponent<BoxCollider>();
        }

        // Create a separate trigger collider for putting the hat on players
        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size *= 1.2f;

        if (!collisionsEnabled)
        {
            DisablePhysics();
        }

        // Grabbing
        var grab = gameObject.GetComponent<XRGrabInteractable>();
        if (!grab)
        {
            grab = gameObject.AddComponent<XRGrabInteractable>();
        }
        grab.selectEntered.AddListener((SelectEnterEventArgs args) =>
        {
            Debug.Log("Hat was selected!");
        });
        grab.selectExited.AddListener((SelectExitEventArgs args) =>
        {
            Debug.Log("Hat was dropped!");
        });
    }

    private void Update()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            lastPosition = transform.position + initTranslation;
            lastRotation = transform.rotation * initRotation;

            context.SendJson(new HatMessage
            {
                position = transform.position,       // world position
                rotation = transform.rotation        // world rotation
            });
        }
    }

    // TODO: I should put an enumerator on this class specifying slot (hat, back, etc.) and switch behaviour here appropriately
    private void OnTriggerEnter(Collider other)
    {
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

        if (existingAccessory != null)
        {
            Debug.Log("Found an existing accessory attached to the avatar");
            accessoryManager.headSpawner.Despawn(existingAccessory.gameObject);
        }

        transform.SetParent(avatarTransform, false);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localPosition = initTranslation;
        transform.localRotation = initRotation;

        DisablePhysics();

        Debug.Log("Hat attached to " + avatar.name);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<HatMessage>();
        transform.position = msg.position;
        transform.rotation = msg.rotation;

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    public void DisablePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    private struct HatMessage
    {
        public Vector3 position;    // global position
        public Quaternion rotation; // global rotation
    }
}
