using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Rcam2 {

[ExecuteInEditMode]
sealed class ClearOnlyCustomRender : MonoBehaviour
{
    Color ClearColor
      => GetComponent<HDAdditionalCameraData>().backgroundColorHDR;

    Rect FullViewport
      => new Rect(0, 0, GetComponent<Camera>().pixelWidth,
                        GetComponent<Camera>().pixelHeight);

    void OnEnable()
      => GetComponent<HDAdditionalCameraData>().customRender += CustomRender;

    void OnDisable()
      => GetComponent<HDAdditionalCameraData>().customRender -= CustomRender;

    void CustomRender(ScriptableRenderContext context, HDCamera camera)
    {
        var rt = camera.camera.targetTexture;
        var rtid = rt != null ?
            new RenderTargetIdentifier(rt) :
            new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

        var cmd = CommandBufferPool.Get("Clear Only");
        cmd.SetViewport(FullViewport);
        cmd.ClearRenderTarget(true, true, ClearColor, 1);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}

} // namespace Rcam2
