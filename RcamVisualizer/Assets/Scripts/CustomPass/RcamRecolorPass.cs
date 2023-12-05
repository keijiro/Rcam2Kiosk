using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Rcam2 {

// HDRP custom fullscreen pass for postprocessing recolor effects
sealed class RcamRecolorPass : CustomPass
{
    #region Editable attributes

    public RcamRecolorController _controller = null;

    #endregion

    #region CustomPass implementation

    protected override void Execute(CustomPassContext context)
    {
        if (_controller == null || !_controller.IsActive) return;
        CoreUtils.DrawFullScreen(context.cmd, _controller.SharedMaterial);
    }

    #endregion
}

} // namespace Rcam2
