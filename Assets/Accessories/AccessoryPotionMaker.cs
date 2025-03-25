using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using System.Collections.Generic;

public class AccessoryPotionMaker : MonoBehaviour
{
    public BoxCollider triggerCollider;

    [SerializeField] private AccessoryManager accessoryManager;
    [SerializeField] private RemoteAvatarInteractableAttacher remoteAvatarInteractableAttacher;
    [SerializeField] private TextureMixer textureMixer;
    private NetworkContext context;
    private HashSet<int> usedHeads = new HashSet<int>();

    public int operationNumber;
    public GameObject ParticlePrefab;
    public GameObject AudioPrefab;

    public struct Accessories
    {
        public int head;
        public int neck;
        public int back;
        public int face;
        public string textureBlob;
    }

    public Accessories accessories;

    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (accessoryManager == null)
        {
            Debug.LogWarning("AccessoryManager reference not set in inspector! (AccessoryPotionMaker)");
        }

        context = NetworkScene.Register(this);
        Debug.Log($"{name} on {UnityEngine.SystemInfo.deviceName} has ID {context.Id}");

        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        accessories.head = -1;
        accessories.neck = -1;
        accessories.back = -1;
        accessories.face = -1;
        accessories.textureBlob = null; // NOTE: Do not try sending a message containing a null field, just send "" instead
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        HatNetworkedObject hat = other.GetComponentInParent<HatNetworkedObject>();
        FakeAvatarHead head = other.GetComponentInParent<FakeAvatarHead>();
        
        if (hat != null)
        {
            switch (hat.slot)
            {
                case AccessorySlot.Head:
                    accessories.head = hat.idx;
                    break;
                case AccessorySlot.Neck:
                    accessories.neck = hat.idx;
                    break;
                case AccessorySlot.Back:
                    accessories.back = hat.idx;
                    break;
                case AccessorySlot.Face:
                    accessories.face = hat.idx;
                    break;
                default:
                    return;
            }

            Destroy(hat.gameObject);

            SpawnEffects(ParticlePrefab, transform.position);
            SpawnEffects(AudioPrefab, transform.position);

            Debug.Log("Head=" + accessories.head + " Neck=" + accessories.neck + " Back=" + accessories.back + " Face=" + accessories.face);
        }
        else if (head != null)
        {
            Debug.Log("Adding texture ingredient...");

            int headID = head.GetInstanceID();
            if (usedHeads.Contains(headID))
            {
                // Already processed this head
                return;
            }
            usedHeads.Add(headID);

            Texture2D fakeAvatarTexture = head.avatarTexture;
            if (fakeAvatarTexture == null)
            {
                Debug.LogWarning("fakeAvatarTexture == null");
            }
            if (accessories.textureBlob == null)
            {
                Debug.LogWarning("accessories.textureBlob == null (before calling AddIngradient)");
            }
            if (textureMixer == null)
            {
                // THIS PRINTS
                Debug.LogWarning("textureMixer == null (before calling AddIngradient");
            }

            Debug.Log("Calling textureMixer.AddIngradient (AccessoryPotionMaker.OnTriggerEnter)");
            accessories.textureBlob = textureMixer.AddIngradient(operationNumber, fakeAvatarTexture, accessories.textureBlob);
            Debug.Log("Leaving textureMixer.AddIngradient (AccessoryPotionMaker.OnTriggerEnter)");

            if (accessories.textureBlob == null)
            {
                Debug.LogWarning("accessories.textureBlob == null (after calling AddIngradient)");
            }

            Debug.Log("Destroying head.gameObject");
            Destroy(head.gameObject);
            operationNumber++;

            Debug.Log("Sending updated cauldron state to peers");
            context.SendJson(new Accessories
            {
                head = accessories.head,
                neck = accessories.neck,
                back = accessories.back,
                face = accessories.face,
                textureBlob = accessories.textureBlob == null ? "" : accessories.textureBlob
            });

            Debug.Log("Spawning particles");
            SpawnEffects(ParticlePrefab, transform.position);
            SpawnEffects(AudioPrefab, transform.position);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Accessories>();

        accessories.head = msg.head;
        accessories.neck = msg.neck;
        accessories.back = msg.back;
        accessories.face = msg.face;
        if (!string.IsNullOrEmpty(msg.textureBlob))
        {
            accessories.textureBlob = msg.textureBlob;
        }

        Debug.Log("Head: " + accessories.head + " Neck: " + accessories.neck + " Back: " + accessories.back + " Face: " + accessories.face);
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
