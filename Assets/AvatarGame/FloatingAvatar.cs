using Ubiq;
using UnityEngine;

/// <summary>
/// Recroom/rayman style avatar with hands, torso and head. This class is only
/// here for compatibility. You can safely ignore it for this example/tutorial.
/// </summary>
public class FloatingAvatar : MonoBehaviour
{
    public Transform head; // Floating_Head
    public Transform torso; // Floating_Torso_A
    public Transform leftHand; // Floating_LeftHand_A
    public Transform rightHand; // Floating_RightHand_A

    public Renderer headRenderer;
    public Renderer torsoRenderer;
    public Renderer leftHandRenderer;
    public Renderer rightHandRenderer;

    public Transform baseOfNeckHint;

    public AnimationCurve torsoFootCurve;

    public AnimationCurve torsoFacingCurve;

    private TexturedAvatar texturedAvatar;
    private HeadAndHandsAvatar headAndHandsAvatar;
    private Vector3 footPosition;
    private Quaternion torsoFacing;
    
    private InputVar<Pose> lastGoodHeadPose;

    private void OnEnable()
    {
        headAndHandsAvatar = GetComponentInParent<HeadAndHandsAvatar>();

        if (headAndHandsAvatar)
        {
            headAndHandsAvatar.OnHeadUpdate.AddListener(HeadAndHandsEvents_OnHeadUpdate);
            headAndHandsAvatar.OnLeftHandUpdate.AddListener(HeadAndHandsEvents_OnLeftHandUpdate);
            headAndHandsAvatar.OnRightHandUpdate.AddListener(HeadAndHandsEvents_OnRightHandUpdate);
        }

        texturedAvatar = GetComponentInParent<TexturedAvatar>();

        if (texturedAvatar)
        {
            texturedAvatar.OnTextureChanged.AddListener(TexturedAvatar_OnTextureChanged);
        }

        // Ensure MeshCollider is enabled on head
        // We'll need this for putting hats on people since the capsule hitbox for avatars is actually a part of something completely separate to avatars
        if (head != null)
        {
            MeshCollider meshCollider = head.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = head.gameObject.AddComponent<MeshCollider>();
            }
            meshCollider.convex = true; // Ensures it can be a trigger
            meshCollider.isTrigger = true; // Enables trigger mode
            meshCollider.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (headAndHandsAvatar && headAndHandsAvatar != null)
        {
            headAndHandsAvatar.OnHeadUpdate.RemoveListener(HeadAndHandsEvents_OnHeadUpdate);
            headAndHandsAvatar.OnLeftHandUpdate.RemoveListener(HeadAndHandsEvents_OnLeftHandUpdate);
            headAndHandsAvatar.OnRightHandUpdate.RemoveListener(HeadAndHandsEvents_OnRightHandUpdate);
        }

        if (texturedAvatar && texturedAvatar != null)
        {
            texturedAvatar.OnTextureChanged.RemoveListener(TexturedAvatar_OnTextureChanged);
        }
    }

    private void HeadAndHandsEvents_OnHeadUpdate(InputVar<Pose> pose)
    {
        if (!pose.valid)
        {
            if (!lastGoodHeadPose.valid)
            {
                headRenderer.enabled = false;
                return;
            }
            
            pose = lastGoodHeadPose;
        }
        
        head.position = pose.value.position; 
        head.rotation = pose.value.rotation;        
        lastGoodHeadPose = pose;
    }

    private void HeadAndHandsEvents_OnLeftHandUpdate(InputVar<Pose> pose)
    {
        if (!pose.valid)
        {
            leftHandRenderer.enabled = false;
            return;
        }
        
        leftHandRenderer.enabled = true;
        leftHand.position = pose.value.position;
        leftHand.rotation = pose.value.rotation;                    
    }

    private void HeadAndHandsEvents_OnRightHandUpdate(InputVar<Pose> pose)
    {
        if (!pose.valid)
        {
            rightHandRenderer.enabled = false;
            return;
        }

        rightHandRenderer.enabled = true;
        rightHand.position = pose.value.position;
        rightHand.rotation = pose.value.rotation;                    
    }

    private void TexturedAvatar_OnTextureChanged(Texture2D tex)
    {
        headRenderer.material.mainTexture = tex;
        torsoRenderer.material = headRenderer.material;
        leftHandRenderer.material = headRenderer.material;
        rightHandRenderer.material = headRenderer.material;
    }

    private void Update()
    {
        UpdateTorso();
    }

    private void UpdateTorso()
    {
        // Give torso a bit of dynamic movement to make it expressive

        // Update virtual 'foot' position, just for animation, wildly inaccurate :)
        var neckPosition = baseOfNeckHint.position;
        footPosition.x += (neckPosition.x - footPosition.x) * Time.deltaTime * torsoFootCurve.Evaluate(Mathf.Abs(neckPosition.x - footPosition.x));
        footPosition.z += (neckPosition.z - footPosition.z) * Time.deltaTime * torsoFootCurve.Evaluate(Mathf.Abs(neckPosition.z - footPosition.z));
        footPosition.y = 0;

        // Forward direction of torso is vector in the transverse plane
        // Determined by head direction primarily, hint provided by hands
        var torsoRotation = Quaternion.identity;

        // Head: Just use head direction
        var headFwd = head.forward;
        headFwd.y = 0;

        var rot = Quaternion.LookRotation(headFwd, Vector3.up);
        var angle = Quaternion.Angle(torsoFacing, rot);
        var rotateAngle = Mathf.Clamp(Time.deltaTime * torsoFacingCurve.Evaluate(Mathf.Abs(angle)), 0, angle);
        torsoFacing = Quaternion.RotateTowards(torsoFacing, rot, rotateAngle);

        // Place torso so it makes a straight line between neck and feet
        torso.position = neckPosition;
        torso.rotation = Quaternion.FromToRotation(Vector3.down, footPosition - neckPosition) * torsoFacing;
    }
}
