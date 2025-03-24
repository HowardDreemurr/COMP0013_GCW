using UnityEngine;
using System.Collections;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class FakeAvatarHead : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    private NetworkContext context;

    public Texture2D avatarTexture; // The cauldron will read this (hopefully)
    private Renderer cachedRenderer;
    private TextureMixer textureMixer;

    private struct FakeAvatarHeadMessage
    {
        public string blob;
    }

    void Awake()
    {
        textureMixer = TextureMixer.Instance;
        if (textureMixer == null)
        {
            Debug.LogWarning("Couldnt bind TextureMixer to FakeAvatarHead");
        }
        cachedRenderer = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        context = NetworkScene.Register(this);
    }

    void Update()
    {
        
    }

    // Called externally after some arbitrary delay
    public void syncState(Texture2D texture)
    {
        if (textureMixer != null)
        {
            // Tell everyone else to set theirs
            context.SendJson(new FakeAvatarHeadMessage
            {
                blob = textureMixer.Texture2DToBase64(texture)
            });
        }

        // Set our own one
        avatarTexture = texture;

        // Also set it on the renderer
        if (cachedRenderer != null)
        {
            cachedRenderer.material.mainTexture = avatarTexture;
        }
        else
        {
            Debug.LogWarning("No Renderer found during syncState");
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<FakeAvatarHeadMessage>();
        if (textureMixer != null)
        {
            avatarTexture = textureMixer.Base64ToTexture2D(msg.blob);
        }
    }
}
