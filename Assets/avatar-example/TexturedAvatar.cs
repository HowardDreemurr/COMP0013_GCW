using System;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;

/// <summary>
/// This class sets the avatar to use a specific texture. It also handles
/// syncing the currently active texture over the network using properties.
/// </summary>
public class TexturedAvatar : MonoBehaviour
{
    public AvatarTextureCatalogue Textures;
    public bool RandomTextureOnSpawn;
    public bool SaveTextureSetting;

    [Serializable]
    public class TextureEvent : UnityEvent<Texture2D> { }
    public TextureEvent OnTextureChanged;

    private Avatar avatar;
    private string uuid;
    private string blob_uuid;
    private string blob_skin;
    private RoomClient roomClient;

    private Texture2D cached; // Cache for GetTexture. Do not do anything else with this; use the uuid

    private void Start()
    {
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        
        avatar = GetComponent<Avatar>();

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
        if (peer != avatar.Peer && avatar.IsLocal)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        if (!string.IsNullOrWhiteSpace(peer["ubiq.avatar.texture.blob_uuid"]))
        {
            Debug.Log("[TA][RCOPU] Updating Blob...");
            //SetCustomTexture(peer["ubiq.avatar.texture.blob_uuid"]);
            Debug.Log("[TA][RCOPU] Updated!");
        } else {
            SetTexture(peer["ubiq.avatar.texture.uuid"]);
        }
    }

    // The method is LOCAL, use SetCustomTexture for custom skin swampping.
    public void SetCustomTextureTest(Texture2D texture)
    {
        Debug.Log("[TA][SCTt] Changing Custom Texture...");
        OnTextureChanged.Invoke(texture);
        Debug.Log("[TA][SCTt] Custom Texture Changed.");
    }


    public void SetCustomTexture(Texture2D texture)
    {   Debug.Log("[TA][SCT] Changing Custom Texture...");
        OnTextureChanged.Invoke(texture);
        Debug.Log("[TA][SCT] Texture Changed!");
        this.cached = texture;
        this.blob_skin = Texture2DToBase64(texture);
        Debug.Log(this.blob_skin);
        if(avatar.IsLocal)
        {
            this.blob_uuid = roomClient.SetBlob(roomClient.Room.UUID, blob_skin);
            Debug.Log(this.blob_uuid);
            roomClient.Me["ubiq.avatar.texture.blob_uuid"] = this.blob_uuid;
        }
        
        if (avatar.IsLocal && SaveTextureSetting)
        {
            SaveSettings();
        }

    }

    public void SetCustomTexture(string blob_uuid)
    {   
        this.blob_uuid = blob_uuid;
        Debug.Log(this.blob_uuid);
        roomClient.GetBlob(roomClient.Room.UUID, blob_uuid, (blob_data) => 
        {
            Debug.Log("[TA]BLOB STARTING FECTING");
            Debug.Log(blob_data);
            SetCustomTexture(Base64ToTexture2D(blob_data));

        });
    }

    public void SetCustomTexture(string blob_uuid, string blob_skin)
    {   
        roomClient.GetBlob(roomClient.Room.UUID, blob_uuid, (blob_data) => 
        {
            if (blob_skin == blob_data) {
                Texture2D texture = Base64ToTexture2D(blob_data);
                OnTextureChanged.Invoke(texture);
                this.cached = texture;
                roomClient.Me["ubiq.avatar.texture.blob_uuid"] = this.blob_uuid;
            } else {
                SetCustomTexture(Base64ToTexture2D(blob_data));
            }
        });
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
        if(String.IsNullOrWhiteSpace(uuid))
        {
            return;
        }

        if (this.uuid != uuid)
        {
            var texture = Textures.Get(uuid);
            this.uuid = uuid;
            this.blob_uuid = null;
            this.blob_skin = null;
            this.cached = texture;

            OnTextureChanged.Invoke(texture);

            if(avatar.IsLocal)
            {
                roomClient.Me["ubiq.avatar.texture.uuid"] = this.uuid;
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
        PlayerPrefs.SetString("ubiq.avatar.texture.blob_uuid", blob_uuid);
        PlayerPrefs.SetString("ubiq.avatar.texture.blob_uuid", blob_skin);
    }

    private bool LoadSettings()
    {
        var uuid = PlayerPrefs.GetString("ubiq.avatar.texture.uuid", "");
        var blob_uuid = PlayerPrefs.GetString("ubiq.avatar.texture.blob_uuid", "");
        var blob_skin = PlayerPrefs.GetString("ubiq.avatar.texture.blob_skin", "");
        if (!string.IsNullOrWhiteSpace(blob_uuid))
        {
            SetCustomTexture(blob_uuid, blob_skin);
        } else {
            SetTexture(uuid);
        }
        return !string.IsNullOrWhiteSpace(uuid) || !string.IsNullOrWhiteSpace(blob_uuid);
    }

    public void ClearSettings()
    {
        PlayerPrefs.DeleteKey("ubiq.avatar.texture.uuid");
    }

    public Texture2D GetTexture()
    {
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
}
