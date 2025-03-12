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
        if (transform.localPosition != lastPosition || transform.localRotation != lastRotation)
        {
            lastPosition = transform.localPosition;
            lastRotation = transform.localRotation;
            context.SendJson(new HatMessage
            {
                position = transform.localPosition,
                rotation = transform.localRotation
            });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<HatMessage>();
        transform.localPosition = msg.position;
        transform.localRotation = msg.rotation;
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
    }

    private struct HatMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
