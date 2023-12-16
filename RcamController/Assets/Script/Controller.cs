using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Klak.Ndi;

namespace Rcam2 {

sealed class Controller : MonoBehaviour
{
    #region External scene object references

    [Space]
    [SerializeField] Camera _camera = null;
    [SerializeField] ARCameraManager _cameraManager = null;
    [SerializeField] ARCameraBackground _cameraBackground = null;
    [SerializeField] AROcclusionManager _occlusionManager = null;
    [SerializeField] InputHandle _input = null;

    #endregion

    #region Editable parameters

    [Space]
    [SerializeField] float _minDepth = 0.2f;
    [SerializeField] float _maxDepth = 3.2f;

    #endregion

    #region Hidden external asset references

    [SerializeField, HideInInspector] NdiResources _ndiResources = null;
    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Internal objects

    const int _width = 2048;
    const int _height = 1024;

    NdiSender _ndiSender;
    Matrix4x4 _projection;
    (RenderTexture latest, RenderTexture delayed) _senderRT;
    (Material bg, Material mux) _material;

    #endregion

    #region Internal methods

    Metadata MakeMetadata()
      => new Metadata { CameraPosition = _camera.transform.position,
                        CameraRotation = _camera.transform.rotation,
                        ProjectionMatrix = _projection,
                        DepthRange = new Vector2(_minDepth, _maxDepth),
                        InputState = _input.InputState };

    RenderTexture NewRT()
    {
        var rt = new RenderTexture(_width, _height, 0);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.Create();
        return rt;
    }

    #endregion

    #region Public method (UI callback)

    public async void ResetSystem()
    {
        _input.Button15 = true;
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        _input.Button15 = false;
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        SceneManager.LoadScene(0);
    }

    #endregion

    #region Camera callbacks

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // We expect there is at least one texture.
        if (args.textures.Count == 0) return;

        // Try receiving Y/CbCr textures.
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            var tex = args.textures[i];
            if (id == ShaderID.TextureY)
                _material.mux.SetTexture(ShaderID.TextureY, tex);
            else if (id == ShaderID.TextureCbCr)
                _material.mux.SetTexture(ShaderID.TextureCbCr, tex);
        }

        // Try receiving the projection matrix.
        if (args.projectionMatrix.HasValue)
        {
            _projection = args.projectionMatrix.Value;

            // Aspect ratio compensation (camera vs. 16:9)
            _projection[1, 1] *= (16.0f / 9) / _camera.aspect;
        }

        // Use the first texture to calculate the source texture aspect ratio.
        var tex1 = args.textures[0];
        var texAspect = (float)tex1.width / tex1.height;

        // Aspect ratio compensation factor for the multiplexer
        var aspectFix = texAspect / (16.0f / 9);
        _material.bg.SetFloat(ShaderID.AspectFix, aspectFix);
        _material.mux.SetFloat(ShaderID.AspectFix, aspectFix);
    }

    void OnOcclusionFrameReceived(AROcclusionFrameEventArgs args)
    {
        // Try receiving stencil/depth textures.
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            var tex = args.textures[i];
            if (id == ShaderID.HumanStencil)
                _material.mux.SetTexture(ShaderID.HumanStencil, tex);
            else if (id == ShaderID.EnvironmentDepth)
                _material.mux.SetTexture(ShaderID.EnvironmentDepth, tex);
        }
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Shader setup
        _material = (new Material(_shader), new Material(_shader));
        _material.bg.EnableKeyword("RCAM_MONITOR");
        _material.mux.EnableKeyword("RCAM_MULTIPLEXER");

        // Custom background material
        _cameraBackground.customMaterial = _material.bg;
        _cameraBackground.useCustomMaterial = true;

        // Render texture for the NDI source
        _senderRT = (NewRT(), NewRT());

        // NDI sender instantiation
        _ndiSender = gameObject.AddComponent<NdiSender>();
        _ndiSender.SetResources(_ndiResources);
        _ndiSender.ndiName = "Rcam";
        _ndiSender.captureMethod = CaptureMethod.Texture;
        _ndiSender.sourceTexture = _senderRT.delayed;
    }


    void OnDestroy()
    {
        Destroy(_material.bg);
        Destroy(_material.mux);
        Destroy(_senderRT.latest);
        Destroy(_senderRT.delayed);
    }

    void OnEnable()
    {
        // Camera callback setup
        _cameraManager.frameReceived += OnCameraFrameReceived;
        _occlusionManager.frameReceived += OnOcclusionFrameReceived;
    }

    void OnDisable()
    {
        // Camera callback termination
        _cameraManager.frameReceived -= OnCameraFrameReceived;
        _occlusionManager.frameReceived -= OnOcclusionFrameReceived;
    }

    void Update()
    {
        // Metadata update
        _ndiSender.metadata = MakeMetadata().Serialize();

        // Parameter update
        var range = new Vector2(_minDepth, _maxDepth);
        _material.bg.SetVector(ShaderID.DepthRange, range);
        _material.mux.SetVector(ShaderID.DepthRange, range);

        // NDI sender RT update
        Graphics.CopyTexture(_senderRT.latest, _senderRT.delayed);
        Graphics.Blit(null, _senderRT.latest, _material.mux, 0);
    }

    #endregion
}

} // namespace Rcam2
