using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class AutoDestroyParticles : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    private NetworkContext context;
    private ParticleSystem particles;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool boardcasted = false;


    private void Start()
    {
        if (NetworkId)
        {
            context = NetworkScene.Register(this);
        }
        particles = GetComponentInChildren<ParticleSystem>();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }


    private void Update()
    {
        if (NetworkId && context.Equals(default(NetworkContext)))
        {
            context = NetworkScene.Register(this);
        }
        if (!boardcasted)
        {
            if ((transform.position - Vector3.zero).sqrMagnitude > 0.1f)
            {
                particles.Play();
                Invoke(nameof(SendTransformMessage), 0.2f);

                float duration = particles.main.duration + particles.main.startLifetime.constantMax + 0.5f;
                Invoke(nameof(DestroyNetworked), duration);
                boardcasted = true;
            }
        }
    }

    private void DestroyNetworked()
    {
        NetworkSpawnManager.Find(this).Despawn(gameObject);
    }

    private void SendTransformMessage()
    {
        context.SendJson(new ParticleMessage
        {
            position = transform.position,
            rotation = transform.rotation
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        boardcasted = true;
        var msg = message.FromJson<ParticleMessage>();
        transform.position = msg.position;
        transform.rotation = msg.rotation;
        particles.Play();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private struct ParticleMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}