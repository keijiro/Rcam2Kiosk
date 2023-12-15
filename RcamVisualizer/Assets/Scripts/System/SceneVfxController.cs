using UnityEngine;
using UnityEngine.VFX;

namespace Rcam2 {

public sealed class SceneVfxController : MonoBehaviour
{
    #region Editable properties

    [field:Space]
    [field:SerializeField] public RcamBackgroundController Background { get; set; }
    [field:SerializeField] public RcamRecolorController Recolor { get; set; }
    [field:Space]
    [field:SerializeField] public VisualEffect[] ForegroundVfx { get; set; }
    [field:SerializeField] public VisualEffect[] BackgroundVfx { get; set; }

    #endregion

    #region Predefined values

    static class PropertyID
    {
        public static int Throttle = Shader.PropertyToID("Throttle");
    }

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        var input = GetComponent<SceneInputAdapter>();

        // Foreground VFX throttling
        for (var i = 0; i < ForegroundVfx.Length; i++)
        {
            var vfx = ForegroundVfx[i];
            if (vfx == null) continue;
            var x = vfx.GetFloat(PropertyID.Throttle);
            var dir = (i == input.ForegroundVfxIndex) ? 1 : -1;
            x = Mathf.Clamp01(x + dir * Time.deltaTime);
            vfx.SetFloat(PropertyID.Throttle, x);
        }

        // Special foreground effect: Posterize (button 6)
        if (input.ForegroundVfxChanged)
        {
            Recolor.FrontFill = (input.ForegroundVfxIndex == 6);
            Recolor.ShuffleColors();
        }

        // Background VFX throttling
        for (var i = 0; i < BackgroundVfx.Length; i++)
        {
            var vfx = BackgroundVfx[i];
            if (vfx == null) continue;
            var x = vfx.GetFloat(PropertyID.Throttle);
            var dir = input.GetBackgroundVfxToggle(i) ? 1 : -1;
            x = Mathf.Clamp01(x + dir * Time.deltaTime);
            vfx.SetFloat(PropertyID.Throttle, x);
        }

        // Special background effect: Voxelize (toggle 2)
        Background.BackFill = !input.GetBackgroundVfxToggle(2);
    }

    #endregion
}

} // namespace Rcam2
