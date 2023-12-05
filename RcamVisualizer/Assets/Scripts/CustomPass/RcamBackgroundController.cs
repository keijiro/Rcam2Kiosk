using UnityEngine;
using Unity.Mathematics;

namespace Rcam2 {

// Controller and data provider for RcamBackgroundPass
public sealed class RcamBackgroundController : MonoBehaviour
{
    #region Public members

    public bool IsActive => BackFill || FrontFill;
    public int PassNumber => EffectNumber;
    public Material SharedMaterial => UpdateMaterial();

    public bool BackFill { get; set; }
    public bool FrontFill { get; set; }
    public int EffectNumber { get; set; }
    public float EffectDirection { get; set; }

    #endregion

    #region Shader and material

    [SerializeField] Shader _shader = null;

    Material _material;

    Material UpdateMaterial()
    {
        if (_material == null) _material = new Material(_shader);

        var opacity = new Vector2(BackFill ? 1 : 0, FrontFill ? 1 : 0);
        _material.SetVector("_Opacity", opacity);

        var phi = EffectDirection * Mathf.PI * 2;
        var direction = new Vector2(Mathf.Sin(phi), Mathf.Cos(phi));
        _material.SetVector("_Direction", direction);

        var t = Time.time;
        var nx = noise.snoise(math.float2(t * 2.21f, 1));
        var ny = noise.snoise(math.float2(t * 0.36f, 2));
        var nz = noise.snoise(math.float2(t * 0.48f, 3));
        nx = 0.6f + 0.40f * nx;
        ny = 0.5f + 0.45f * ny;
        nz = 0.6f + 0.40f * nz;
        _material.SetVector("_EffectParams", new Vector3(nx * nx, ny, nz));

        return _material;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => Destroy(_material);

    #endregion
}

} // namespace Rcam2
