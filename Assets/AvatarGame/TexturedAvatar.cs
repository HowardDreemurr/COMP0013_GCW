using System;
using Ubiq.Spawning;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;
using System.Collections;

/// <summary>
/// This class sets the avatar to use a specific texture. It also handles
/// syncing the currently active texture over the network using properties.
/// </summary>
public class TexturedAvatar : MonoBehaviour
{
    public AvatarTextureCatalogue Textures;
    public bool RandomTextureOnSpawn;
    public bool SaveTextureSetting;
    public GameObject Particles;
    public GameObject Audio;

    [Serializable]
    public class TextureEvent : UnityEvent<Texture2D> { }
    public TextureEvent OnTextureChanged;

    private Avatar avatar;
    private FloatingAvatar floatingAvatar;
    private string uuid;
    private string blob;
    private RoomClient roomClient;

    private Texture2D cached; // Cache for GetTexture. Do not do anything else with this; use the uuid

    private void Start()
    {
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();

        avatar = GetComponent<Avatar>();
        floatingAvatar = avatar.GetComponentInChildren<FloatingAvatar>();

        if (avatar.IsLocal)
        {
            var hasSavedSettings = false;
            if (SaveTextureSetting)
            {
                hasSavedSettings = LoadSettings();
            }
            if (!hasSavedSettings && RandomTextureOnSpawn)
            {
                SetTexture(Textures.Get(UnityEngine.Random.Range(0, Textures.Count)));
            }
        }

        if (!avatar.IsLocal)
        {
            var peer = avatar.Peer;

            var uuid = peer["ubiq.avatar.texture.uuid"];
            var blob = peer["ubiq.avatar.texture.blob"];

            if (!string.IsNullOrWhiteSpace(blob))
            {
                SetCustomTexture(blob);
            }
            else if (!string.IsNullOrWhiteSpace(uuid))
            {
                SetTexture(uuid);
            }
        }

        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
    }

    private void OnDestroy()
    {
        // Cleanup the event for new properties so it does not get called after
        // we have been destroyed.
        if (roomClient)
        {
            roomClient.OnPeerUpdated.RemoveListener(RoomClient_OnPeerUpdated);
        }
    }

    void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        if (!string.IsNullOrWhiteSpace(peer["ubiq.avatar.texture.blob"]))
        {
            SetCustomTexture(peer["ubiq.avatar.texture.blob"]);
        }
        else
        {
            SetTexture(peer["ubiq.avatar.texture.uuid"]);
        }
    }

    // The method is LOCAL, use SetCustomTexture for custom skin swampping.
    public void SetCustomTextureTest(Texture2D texture)
    {
        roomClient.Me["ubiq.avatar.texture.blob"] = Texture2DToBase64(texture);
    }


    public void SetCustomTexture(Texture2D texture)
    {
        SetCustomTexture(Texture2DToBase64(texture));
    }

    public void SetCustomTexture(string blob)
    {
        if (String.IsNullOrWhiteSpace(blob))
        {
            return;
        }

        if (this.blob != blob)
        {
            this.blob = blob;
            this.cached = Base64ToTexture2D(blob);

            OnTextureChanged.Invoke(this.cached);

            if (avatar.IsLocal)
            {
                roomClient.Me["ubiq.avatar.texture.blob"] = blob;
                SpawnEffects(Particles);
                SpawnEffects(Audio);
            }

            if (avatar.IsLocal && SaveTextureSetting)
            {
                SaveSettings();
            }
        }
    }


    /// <summary>
    /// Try to set the Texture by reference to a Texture in the Catalogue. If the Texture is not in the
    /// catalogue then this method has no effect, as Texture2Ds cannot be streamed yet.
    /// </summary>
    /// 

    public void SetTexture(Texture2D texture)
    {
        SetTexture(Textures.Get(texture));
    }

    public void SetTexture(string uuid)
    {
        if (String.IsNullOrWhiteSpace(uuid))
        {
            return;
        }

        if (this.uuid != uuid)
        {
            var texture = Textures.Get(uuid);
            this.uuid = uuid;
            this.blob = null;
            this.cached = texture;

            OnTextureChanged.Invoke(texture);

            if (avatar.IsLocal)
            {
                roomClient.Me["ubiq.avatar.texture.blob"] = null;
                roomClient.Me["ubiq.avatar.texture.uuid"] = this.uuid;

                SpawnEffects(Particles);
                SpawnEffects(Audio);
            }

            if (avatar.IsLocal && SaveTextureSetting)
            {
                SaveSettings();
            }
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetString("ubiq.avatar.texture.uuid", uuid);
        PlayerPrefs.SetString("ubiq.avatar.texture.blob", blob);
    }

    private bool LoadSettings()
    {
        var uuid = PlayerPrefs.GetString("ubiq.avatar.texture.uuid", "");
        var blob = PlayerPrefs.GetString("ubiq.avatar.texture.blob", "");

        if (!string.IsNullOrWhiteSpace(blob))
        {
            SetCustomTexture(blob);
        }
        else
        {
            SetTexture(uuid);
        }
        return !string.IsNullOrWhiteSpace(uuid) || !string.IsNullOrWhiteSpace(blob);
    }

    public void ClearSettings()
    {
        PlayerPrefs.DeleteKey("ubiq.avatar.texture.uuid");
    }

    public Texture2D GetTexture()
    {
        if (cached == null)
        {
            if (!string.IsNullOrWhiteSpace(blob))
            {
                cached = Base64ToTexture2D(blob);
            }
            else if (!string.IsNullOrWhiteSpace(uuid))
            {
                cached = Textures.Get(uuid);
            }
        }
        return cached;
    }

    private Texture2D Base64ToTexture2D(string base64)
    {
        byte[] pngData = Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);
        return texture;
    }

    private string Texture2DToBase64(Texture2D texture)
    {
        byte[] pngData = texture.EncodeToPNG();
        string base64 = Convert.ToBase64String(pngData);
        return base64;
    }

    private void SpawnEffects(GameObject particlePrefab)
    {
        if (particlePrefab)
        {
            var instance = NetworkSpawnManager.Find(this).SpawnWithPeerScope(particlePrefab);

            instance.transform.position = floatingAvatar.torso.position;
        }
    }

}

