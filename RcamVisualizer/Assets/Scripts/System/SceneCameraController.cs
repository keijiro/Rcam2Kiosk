using UnityEngine;
using UnityEngine.VFX;
using Klak.Math;

namespace Rcam2 {

public sealed class SceneCameraController : MonoBehaviour
{
    #region Editable properties

    [field:Space]
    [field:SerializeField] public InputHandle Input { get; set; }

    [field:Space]
    [field:SerializeField] public Transform Target { get; set; }
    [field:SerializeField] public Transform Follower { get; set; }
    [field:SerializeField] public Transform LockPoint { get; set; }

    [field:Space]
    [field:SerializeField] public GameObject Background { get; set; }
    [field:SerializeField] public GameObject Recolor { get; set; }

    [field:Space]
    [field:SerializeField] public VisualEffect PointCloud { get; set; }
    [field:SerializeField] public VisualEffect ProjecterVfx { get; set; }
    [field:SerializeField] public Color LineColor { get; set; } = Color.white;

    #endregion

    #region Private objects

    bool _locked;

    #endregion

    #region Predefined values

    static class PropertyID
    {
        public static int LineColor = Shader.PropertyToID("Line Color");
    }

    #endregion

    #region Camera switching (async operations)

    async void LockCamera()
    {
        // Hierarchy change (parented to the lock point)
        Target.parent = LockPoint;

        // Tween to the zero point
        for (var t = 0.0f; t < 0.25f; t += Time.deltaTime)
        {
            Target.localPosition = ExpTween.Step(Target.localPosition, Vector3.zero, 12);
            Target.localRotation = ExpTween.Step(Target.localRotation, Quaternion.identity, 12);
            await Awaitable.NextFrameAsync();
        }

        // Force reset
        Target.localPosition = Vector3.zero;
        Target.localRotation = Quaternion.identity;

        // Effect visibilities
        Background.SetActive(true);
        Recolor.SetActive(true);
        PointCloud.enabled = false;
        ProjecterVfx.SetVector4(PropertyID.LineColor, Color.clear);
    }

    void UnlockCamera()
    {
        // Smooth follower reset to the current camera position
        Follower.position = Target.position;
        Follower.rotation = Target.rotation;

        // Hierarhy change (parented to the smooth follower)
        Target.parent = Follower;

        // Effect visibilities
        PointCloud.enabled = true;
        Background.SetActive(false);
        Recolor.SetActive(false);
        ProjecterVfx.SetVector4(PropertyID.LineColor, LineColor);
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _locked = true;

    void Update()
    {
        // Button input
        var lockButtonPressed = Input.GetButton(8);
        var unlockButtonPressed = Input.GetButton(9);

        if (!_locked && lockButtonPressed)
        {
            // Lock action invoked
            _locked = true;
            LockCamera();
        }
        else if (_locked && unlockButtonPressed)
        {
            // Unlock action invoked
            _locked = false;
            UnlockCamera();
        }
    }

    #endregion
}

} // namespace Rcam2
