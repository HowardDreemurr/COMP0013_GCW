using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class HatNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Start()
    {
        context = NetworkScene.Register(this);
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
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

    private struct HatMessage
    {
        public Vector3 position;    // global position
        public Quaternion rotation; // global rotation
    }
}
