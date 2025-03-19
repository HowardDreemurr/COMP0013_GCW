using System.Collections;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Random = UnityEngine.Random;
public class TestChangeSkin : MonoBehaviour
{
    public GameObject prefab;

    public Texture2D texture;
    
    private XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;
    
    private void Start()
    {
        // Connect up the event for the XRI button.
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(Interactable_SelectEntered);
        
        var networkScene = NetworkScene.Find(this); 
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
    }

    private void OnDestroy()
    {
        // Cleanup the event for the XRI button so it does not get called after
        // we have been destroyed.
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
        }
    }

    private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
    {
        // The button has been pressed.
        
        // Change the local avatar prefab to the default one, because we have
        // a few costumes for that avatar bundled with Ubiq. The AvatarManager
        // will do the work of letting other peers know about the prefab change.
        avatarManager.avatarPrefab = prefab; 
        // Also, set the costume to a new, random one. We use a coroutine to
        // wait one frame to allow the AvatarManager time to spawn the new
        // prefab.
        StartCoroutine(SetRandomCostume());
    }
    
    // This is a coroutine. They can be used in Unity to spread work out over 
    // multiple frames. They can be paused with a 'yield' instruction. When
    // the yield ends, they will pick up again wherever they left off.
    private IEnumerator SetRandomCostume()
    {
        while (true)
        {
            if (!avatarManager)
            {
                // Yield break ends the coroutine.
                yield break;
            }
            
            var avatar = avatarManager.FindAvatar(roomClient.Me);
            if (avatar)
            {    
                var textured = avatar.GetComponentInChildren<TexturedAvatar>();
                if (textured)
                {
                    textured.SetCustomTexture(this.texture);
                    
                    // End the coroutine.
                    yield break;
                }
            }
            
            // Yield return null pauses the coroutine until the next frame. We
            // wait a few frames to allow the prefab to be spawned and to
            // initialise itself.
            yield return null;
            yield return null;
        }
    }
}
