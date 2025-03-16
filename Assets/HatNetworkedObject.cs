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
                gameObject.AddComponent<BoxCollider>();
            }
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
