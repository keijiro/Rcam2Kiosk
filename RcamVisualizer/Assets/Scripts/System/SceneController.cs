using UnityEngine;
using UnityEngine.VFX;

namespace Rcam2 {

public sealed class SceneController : MonoBehaviour
{
    #region Editable properties

    [field:Space]
    [field:SerializeField] public Transform FixedCameraTarget { get; set; }
    [field:SerializeField] public Transform FloatingCameraTarget { get; set; }

    [field:Space]
    [field:SerializeField] public VisualEffect[] ForegroundVfx { get; set; }
    [field:SerializeField] public VisualEffect[] BackgroundVfx { get; set; }

    #endregion

    #region Private objects

    int _fgVfxIndex;
    bool _cameraLock;
    Transform _cameraFollwer;

    #endregion

    #region Predefined values

    static class PropertyID
    {
        public static int Throttle = Shader.PropertyToID("Throttle");
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var camera = Camera.main;
        _cameraFollwer = camera.transform.parent;
        camera.transform.parent = FixedCameraTarget;
        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        _cameraLock = true;
    }

    void Update()
    {
        var input = FindFirstObjectByType<InputHandle>();

        // Selection by the foreground VFX button
        var prevFgVfxIndex = _fgVfxIndex;
        for (var i = 0; i < ForegroundVfx.Length; i++)
            if (input.GetButton(i)) _fgVfxIndex = i;

        // Foreground VFX throttling
        for (var i = 0; i < ForegroundVfx.Length; i++)
        {
            var vfx = ForegroundVfx[i];
            if (vfx == null) continue;
            var x = vfx.GetFloat(PropertyID.Throttle);
            var dir = (i == _fgVfxIndex) ? 1 : -1;
            x = Mathf.Clamp01(x + dir * 0.5f * Time.deltaTime);
            vfx.SetFloat(PropertyID.Throttle, x);
        }

        var recolor = FindFirstObjectByType<RcamRecolorController>();
        // Special foreground effect: Posterize (button 6)
        if (prevFgVfxIndex != _fgVfxIndex)
        {
            recolor.FrontFill = (_fgVfxIndex == 6);
            recolor.ShuffleColors();
        }

        // Background VFX throttling
        for (var i = 0; i < BackgroundVfx.Length; i++)
        {
            var vfx = BackgroundVfx[i];
            if (vfx == null) continue;
            var x = vfx.GetFloat(PropertyID.Throttle);
            var dir = input.GetToggle(i) ? 1 : -1;
            x = Mathf.Clamp01(x + dir * 0.5f * Time.deltaTime);
            vfx.SetFloat(PropertyID.Throttle, x);
        }

        // Special background effect: Voxelize (toggle 2)
        var bg = FindFirstObjectByType<RcamBackgroundController>();
        if (bg != null) bg.BackFill = !input.GetToggle(2);

        //
        var prevLock = _cameraLock;
        var toLocked = input.GetButton(8);
        var toUnlocked = input.GetButton(9);
        _cameraLock = toLocked ? true : (toUnlocked ? false : _cameraLock);

        if (_cameraLock != prevLock)
        {
            var camera = Camera.main;
            if (_cameraLock)
            {
                camera.transform.parent = FixedCameraTarget;
                camera.transform.localPosition = Vector3.zero;
                camera.transform.localRotation = Quaternion.identity;
            }
            else
            {
                camera.transform.parent = _cameraFollwer;
                camera.transform.localPosition = Vector3.zero;
                camera.transform.localRotation = Quaternion.identity;
            }

            bg.gameObject.SetActive(_cameraLock);
            recolor.gameObject.SetActive(_cameraLock);
        }
    }

    #endregion
}

} // namespace Rcam2
