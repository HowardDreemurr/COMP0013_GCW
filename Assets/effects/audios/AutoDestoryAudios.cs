using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class AutoDestoryAudios : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    private NetworkContext context;
    private AudioSource audios;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool boardcasted = false;

    void Start()
    {
        context = NetworkScene.Register(this);
        audios = GetComponentInChildren<AudioSource>();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void Update()
    {
        if (!boardcasted)
        {
            if ((transform.position - Vector3.zero).sqrMagnitude > 0.1f)
            {
                Debug.Log("Trigger!");
                audios.Play();
                Invoke(nameof(SendTransformMessage), 0.2f);

                float duration = audios.clip.length + 0.5f;
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
        context.SendJson(new AudioMessage
        {
            position = transform.position,
            rotation = transform.rotation
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        boardcasted = true;
        var msg = message.FromJson<AudioMessage>();
        transform.position = msg.position;
        transform.rotation = msg.rotation;
        audios.Play();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private struct AudioMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
