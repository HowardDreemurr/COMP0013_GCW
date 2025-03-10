using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class HatNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
}
