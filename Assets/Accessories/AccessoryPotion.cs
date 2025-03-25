using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using NUnit.Framework;
using System.Collections.Generic;
using Ubiq.Rooms;
using Ubiq.Avatars;

public class AccessoryPotion : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }

    public GameObject ParticlePrefab;
    public GameObject AudioPrefab;

    public bool collisionsEnabled;
    public Rigidbody rigidBody;
    public SphereCollider triggerCollider; // Implements 'splash' logic
    public SphereCollider boxCollider; // Implements actual collisions

    public float expansionDuration = 5f;
    public float expansionSize = 20f;
    private float timer = 0f;

    private bool smashed = false;
    private bool expanding = false;
    public bool destroyed = false;

    private List<string> affectedAvatars = new List<string>();

    private float initialSize;

    private AccessoryManager accessoryManager; // Injected by design-time button that holds reference to accessoryManager

    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private bool owner = false;

    private TextureMixer textureMixer;

    private RoomClient roomClient;
    private AvatarManager avatarManager;

    public struct Accessories
    {
        public int head;
        public int neck;
        public int back;
        public int face;
        public string textureBlob;
    }

    public Accessories accessories;

    private enum CollisionState
    {
        Unset,
        Enabled,
        Disabled
    }

    void Awake()
    {
        var networkScene = NetworkScene.Find(this);
        if (networkScene != null)
        {
            roomClient = networkScene.GetComponentInChildren<RoomClient>();
            avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        }
        if (roomClient == null)
        {
            Debug.Log("Couldnt set RoomClient for AccessoryPotion");
        }
        if (avatarManager == null)
        {
            Debug.Log("Couldnt set AvatarManager for AccessoryPotion");
        }

        textureMixer = TextureMixer.Instance;
        if (textureMixer == null)
        {
            Debug.LogWarning("Couldnt bind TextureMixer to AccessoryPotion");
        }

        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false; // Start with gravity disabled
            rigidBody.isKinematic = false;
        }

        boxCollider = GetComponent<SphereCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<SphereCollider>();
        }

        if (triggerCollider == null)
        {
            // Create a separate trigger collider for testing velocity on collision and breaking potion if other some threshold
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
        }

        // Grabbing
        XRGrabInteractable grab = gameObject.GetComponent<XRGrabInteractable>();
        if (!grab)
        {
            grab = gameObject.AddComponent<XRGrabInteractable>();
        }
        grab.selectEntered.AddListener((SelectEnterEventArgs args) =>
        {
            Debug.Log("(Potion) Grabed");
            DisablePhysics();
            owner = true;
        });
        grab.selectExited.AddListener((SelectExitEventArgs args) =>
        {
            Debug.Log("(Potion) Dropped");
            EnablePhysics();
            owner = false;
        });

        // Retrieve AccessoryManager
        accessoryManager = FindFirstObjectByType<AccessoryManager>();
        if (accessoryManager == null)
        {
            Debug.LogWarning("(Potion) Local AccessoryManager not found");
            return;
        }
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        collisionsEnabled = true;

        if (rigidBody != null)
        {
            // Attempt 2 at disabling gravity, because apparently it won't work in Awake
            rigidBody.useGravity = false;
            rigidBody.isKinematic = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyed) return; // We're waiting for the NetworkSpawner to despawn this object

        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            if (owner)
            {
                context.SendJson(new TransformMessage
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    smashed = smashed,
                    collisions = CollisionState.Unset,
                    headIdx = -2,
                    neckIdx = -2,
                    backIdx = -2,
                    faceIdx = -2,
                    textureBlob = ""
                });
            }
        }

        if (smashed && !expanding)
        {
            // The potion smashed on someone elses client but not our one - take their word for it
            SmashPotion();
        }

        if (expanding)
        {
            timer += Time.deltaTime;
            if (timer >= expansionDuration)
            {
                Debug.Log("Potion stopped expanding");
                destroyed = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore collisions with other AccessoryPotions
        if (other.GetComponentInParent<AccessoryPotion>() != null && other.gameObject != this.gameObject)
        {
            return;
        }

        if (collisionsEnabled)
        {
            float velocity = rigidBody.linearVelocity.magnitude;

            Debug.Log("Potion collided with velocity " + velocity);

            if (velocity > 0.1)
            {
                Debug.Log("Head: " + accessories.head + " Neck :" + accessories.neck + " Face: " + accessories.face + " Back: " + accessories.back);
                SmashPotion();
            }
        }
        if (expanding)
        {
            Debug.Log($"OnTriggerEnter called by collider: {other.gameObject.name}");
            Ubiq.Avatars.Avatar avatar = other.GetComponentInParent<Ubiq.Avatars.Avatar>();

            if (avatar != null && avatar == avatarManager.FindAvatar(roomClient.Me) && !affectedAvatars.Contains(avatar.name))
            {
                Debug.Log("Potion touched avatar " + avatar.name);

                if (accessories.head > -1)
                {
                    accessoryManager.AttachHatOnSpawn(accessories.head, avatar, AccessorySlot.Head);
                }
                if (accessories.back > -1)
                {
                    accessoryManager.AttachHatOnSpawn(accessories.back, avatar, AccessorySlot.Back);
                }
                if (accessories.face > -1)
                {
                    accessoryManager.AttachHatOnSpawn(accessories.face, avatar, AccessorySlot.Face);
                }
                if (!string.IsNullOrEmpty(accessories.textureBlob))
                {
                    Debug.Log("blob: " + accessories.textureBlob);
                    Debug.Log("Applying texture to " + avatar.name);
                    Texture2D avatarTexture = textureMixer.Base64ToTexture2D(accessories.textureBlob);
                    TexturedAvatar texturedAvatar = avatar.GetComponent<TexturedAvatar>();
                    texturedAvatar.SetCustomTexture(avatarTexture);
                }

                affectedAvatars.Add(avatar.name); // don't apply to this avatar again or else it just freezes the game
            }
        }
    }

    private void SmashPotion()
    {
        Debug.Log("Potion smashed");

        owner = false;

        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
            rigidBody.useGravity = true;
            rigidBody.detectCollisions = true;
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            //renderer.enabled = false;
        }

        if (triggerCollider != null)
        {
            initialSize = triggerCollider.radius;
            expanding = true; // Start expansion
        }

        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = false;
            Destroy(grab);
        }

        boxCollider.enabled = false;
        collisionsEnabled = false;

        triggerCollider.radius = initialSize * expansionSize;
        rigidBody.WakeUp();

        //
        context.SendJson(new TransformMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            smashed = true,
            collisions = CollisionState.Disabled,
            headIdx = -2,
            neckIdx = -2,
            backIdx = -2,
            faceIdx = -2,
            textureBlob = ""
        });

        SpawnEffects(ParticlePrefab, transform.position);
        SpawnEffects(AudioPrefab, transform.position);
    }

    void OnDrawGizmos()
    {
        if (triggerCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(triggerCollider.center, triggerCollider.radius);
        }
    }

    private struct TransformMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool smashed;
        public CollisionState collisions;
        public int headIdx; // -2 => no change, -1 => no accessory
        public int neckIdx;
        public int backIdx;
        public int faceIdx;
        public string textureBlob; // "" => no texture (DO NOT SEND NULL)
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<TransformMessage>();

        if (!owner)
        {
            transform.position = msg.position;
            transform.rotation = msg.rotation;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
        if (msg.collisions == CollisionState.Enabled && !collisionsEnabled)
        {
            EnablePhysics();
            owner = false;
        }
        else if (msg.collisions == CollisionState.Disabled && collisionsEnabled)
        {
            DisablePhysics();
            owner = false;
        }

        if (msg.smashed && !expanding)
        {
            // If !msg.smashed, just keep it as is; we want to hold true until we finish calling SmashPotion locally
            smashed = true;
        }

        if (msg.headIdx != -2)
        {
            accessories.head = msg.headIdx;
        }
        if (msg.neckIdx != -2)
        {
            accessories.neck = msg.neckIdx;
        }
        if (msg.backIdx != -2)
        {
            accessories.back = msg.backIdx;
        }
        if (msg.faceIdx != -2)
        {
            accessories.face = msg.faceIdx;
        }
        if (!string.IsNullOrEmpty(msg.textureBlob)) // Only update this if msg.textureBlob != ""
        {
            accessories.textureBlob = msg.textureBlob;
        }
    }

    public void DisablePhysics()
    {
        Debug.Log("(Potion) Disabling Physics");

        collisionsEnabled = false;
        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
            rigidBody.detectCollisions = false;
            rigidBody.useGravity = false;
        }

        context.SendJson(new TransformMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            smashed = smashed,
            collisions = CollisionState.Disabled,
            headIdx = -2,
            neckIdx = -2,
            backIdx = -2,
            faceIdx = -2,
            textureBlob = ""
        });
    }

    public void EnablePhysics()
    {
        Debug.Log("(Potion) Enabling Physics");

        collisionsEnabled = true;
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
            rigidBody.detectCollisions = true;
            rigidBody.useGravity = true;
        }

        context.SendJson(new TransformMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            smashed = smashed,
            collisions = CollisionState.Enabled,
            headIdx = -2,
            neckIdx = -2,
            backIdx = -2,
            faceIdx = -2,
            textureBlob = ""
        });
    }

    public void syncState()
    {
        // Only the guy who pressed the button will be calling this function
        // It's called 200ms after spawning on his end to (hopefully) prevent the message being lost due to NetworkSpawner latency
        context.SendJson(new TransformMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            smashed = smashed,
            collisions = CollisionState.Unset,
            headIdx = accessories.head,
            neckIdx = accessories.neck,
            backIdx = accessories.back,
            faceIdx = accessories.face,
            textureBlob = accessories.textureBlob
        });
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
