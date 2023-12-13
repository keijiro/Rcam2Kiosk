using UnityEngine;
using UnityEngine.VFX;

namespace Rcam2 {

public sealed class SceneController : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField] public VisualEffect[] ForegroundVfx { get; set; }
    [field:SerializeField] public VisualEffect[] BackgroundVfx { get; set; }

    #endregion

    #region Private objects

    int _fgVfxIndex;
    InputHandle _input;

    #endregion

    #region Predefined values

    static class PropertyID
    {
        public static int Throttle = Shader.PropertyToID("Throttle");
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _input = FindFirstObjectByType<InputHandle>();

    void Update()
    {
        // Selection by the foreground VFX button
        var prevFgVfxIndex = _fgVfxIndex;
        for (var i = 0; i < ForegroundVfx.Length; i++)
            if (_input.GetButton(i)) _fgVfxIndex = i;

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

        // Special foreground effect: Posterize (button 6)
        if (prevFgVfxIndex != _fgVfxIndex)
        {
            var recolor = FindFirstObjectByType<RcamRecolorController>();
            recolor.FrontFill = (_fgVfxIndex == 6);
            recolor.ShuffleColors();
        }

        // Background VFX throttling
        for (var i = 0; i < BackgroundVfx.Length; i++)
        {
            var vfx = BackgroundVfx[i];
            if (vfx == null) continue;
            var x = vfx.GetFloat(PropertyID.Throttle);
            var dir = _input.GetToggle(i) ? 1 : -1;
            x = Mathf.Clamp01(x + dir * 0.5f * Time.deltaTime);
            vfx.SetFloat(PropertyID.Throttle, x);
        }

        // Special background effect: Voxelize (toggle 2)
        var bg = FindFirstObjectByType<RcamBackgroundController>();
        bg.BackFill = !_input.GetToggle(2);
    }

    #endregion
}

} // namespace Rcam2
