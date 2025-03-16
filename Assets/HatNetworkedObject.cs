using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

// NB: This is called 'HatNetworkedObject' as a holdover, it can be thrown onto any accessory, not just hats
public class HatNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public bool collisionsEnabled;
    public Rigidbody rb;
    public BoxCollider bc;
    public BoxCollider triggerCollider;

    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Start()
    {
        context = NetworkScene.Register(this);

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        if (collisionsEnabled)
        {
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
        }
    }

    private void Update()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

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
                AttachHat(avatar);
            }
        }
    }

    private void AttachHat(Ubiq.Avatars.Avatar avatar)
    {
        Debug.Log("Hat attached to " + avatar.name);

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
            Debug.Log("Found an existing hat attached to the avatar\'s head");
            // TODO: Despawn hat
        }

        transform.SetParent(headTransform, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        DisablePhysics();
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
