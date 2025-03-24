using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class AccessoryPotionMaker : MonoBehaviour
{
    public BoxCollider triggerCollider;

    [SerializeField] private AccessoryManager accessoryManager;
    [SerializeField] private RemoteAvatarInteractableAttacher remoteAvatarInteractableAttacher;
    private NetworkContext context;
    private TextureMixer textureMixer;

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
        textureMixer = TextureMixer.Instance;
        if (textureMixer == null)
        {
            // We could just set this in the inspector if there's issues
            Debug.LogWarning("Couldnt bind TextureMixer to AccessoryPotionMaker");
        }
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
                    accessoryManager.headSpawner.Despawn(hat.gameObject);
                    break;
                case AccessorySlot.Neck:
                    accessories.neck = hat.idx;
                    accessoryManager.neckSpawner.Despawn(hat.gameObject);
                    break;
                case AccessorySlot.Back:
                    accessories.back = hat.idx;
                    accessoryManager.backSpawner.Despawn(hat.gameObject);
                    break;
                case AccessorySlot.Face:
                    accessories.face = hat.idx;
                    accessoryManager.faceSpawner.Despawn(hat.gameObject);
                    break;
                default:
                    return;
            }

            context.SendJson(new Accessories
            {
                head = accessories.head,
                neck = accessories.neck,
                back = accessories.back,
                face = accessories.face,
                textureBlob = accessories.textureBlob == null ? "" : accessories.textureBlob
            });

            SpawnEffects(ParticlePrefab, transform.position);
            SpawnEffects(AudioPrefab, transform.position);
        }
        else if (head != null)
        {
            Debug.Log("Adding texture ingredient...");
            Texture2D fakeAvatarTexture = head.avatarTexture;
            accessories.textureBlob = textureMixer.AddIngradient(operationNumber, fakeAvatarTexture, accessories.textureBlob);
            remoteAvatarInteractableAttacher.spawner.Despawn(head.gameObject);
            operationNumber++;

            context.SendJson(new Accessories
            {
                head = accessories.head,
                neck = accessories.neck,
                back = accessories.back,
                face = accessories.face,
                textureBlob = accessories.textureBlob == null ? "" : accessories.textureBlob
            });

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
