using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Rcam2 {

// HDRP custom fullscreen pass for drawing camera images
sealed class RcamBackgroundPass : CustomPass
{
    #region Editable attributes

    public RcamBackgroundController _controller = null;

    #endregion

    #region CustomPass implementation

    protected override void Execute(CustomPassContext context)
    {
        if (_controller == null || !_controller.IsActive) return;

        var recv = Singletons.Receiver;
        if (recv == null || recv.ColorTexture == null) return;

        var prj = ProjectionUtil.VectorFromReceiver;
        var v2w = Singletons.Receiver.CameraToWorldMatrix;

        var m = _controller.SharedMaterial;
        m.SetVector(ShaderID.ProjectionVector, prj);
        m.SetMatrix(ShaderID.InverseViewMatrix, v2w);
        m.SetTexture(ShaderID.ColorTexture, recv.ColorTexture);
        m.SetTexture(ShaderID.DepthTexture, recv.DepthTexture);

        CoreUtils.DrawFullScreen(context.cmd, m, null, _controller.PassNumber);
    }

    #endregion
}

} // namespace Rcam2
