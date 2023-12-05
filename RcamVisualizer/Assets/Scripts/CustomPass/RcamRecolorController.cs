using UnityEngine;
using Klak.Chromatics;

namespace Rcam2 {

// Controller and data provider for RcamRecolorPass
public sealed class RcamRecolorController : MonoBehaviour
{
    #region Public members

    public bool IsActive => BackFill || FrontFill;
    public Material SharedMaterial => UpdateMaterial();

    public bool BackFill { get; set; }
    public bool FrontFill { get; set; }

    public void ShuffleColors() => RandomizeGradientsAndColors();

    #endregion

    #region Editable attributes

    [SerializeField, Range(0, 1)] float _dithering = 0.5f;
    [SerializeField] float _lineThreshold = 0.5f;
    [SerializeField] float _lineContrast = 1;

    #endregion

    #region Private variables

    Gradient _backGradient = new Gradient();
    Gradient _frontGradient = new Gradient();
    Color _lineColor;

    #endregion

    #region Random color scheme

    GradientColorKey [] _colorKeys = new GradientColorKey[3];
    GradientAlphaKey [] _alphaKeys =
      new GradientAlphaKey[] { new GradientAlphaKey(1, 0) };

    void RandomizeGradientsAndColors()
    {
        var h1 = Random.value;
        var h2 = (h1 + 0.333f) % 1;

        var h3 = Random.value;
        var h4 = (h3 + 0.333f) % 1;

        var bg1 = Color.black;
        var bg2 = Color.HSVToRGB(h1, 1, 0.5f);
        var bg3 = Color.HSVToRGB(h2, 1, 0.8f);

        var fg1 = Color.HSVToRGB(h3, 1, 0.3f);
        var fg2 = Color.HSVToRGB(h4, 1, 1.0f);
        var fg3 = Color.white;

        _colorKeys[0] = new GradientColorKey(bg1, 0.5f);
        _colorKeys[1] = new GradientColorKey(bg2, 0.75f);
        _colorKeys[2] = new GradientColorKey(bg3, 1);
        _backGradient.SetKeys(_colorKeys, _alphaKeys);
        _backGradient.mode = GradientMode.Fixed;

        _colorKeys[0] = new GradientColorKey(fg1, 0.5f);
        _colorKeys[1] = new GradientColorKey(fg2, 0.75f);
        _colorKeys[2] = new GradientColorKey(fg3, 1);
        _frontGradient.SetKeys(_colorKeys, _alphaKeys);
        _frontGradient.mode = GradientMode.Fixed;

        _lineColor = Color.white;
    }

    #endregion

    #region Shader and material

    [SerializeField] Shader _shader = null;

    Material _material;

    Material UpdateMaterial()
    {
        if (_material == null) _material = new Material(_shader);

        var opacity = new Vector2(BackFill ? 1 : 0, FrontFill ? 1 : 0);

        _material.SetLinearGradient("_BackGradient", _backGradient);
        _material.SetLinearGradient("_FrontGradient", _frontGradient);
        _material.SetColor("_EdgeColor", _lineColor);
        _material.SetFloat("_EdgeThreshold", _lineThreshold);
        _material.SetFloat("_EdgeContrast", _lineContrast);
        _material.SetFloat("_Dithering", _dithering);
        _material.SetVector("_Opacity", opacity);

        return _material;
    }

    #endregion

    #region MonoBehaviour implementation

    void Start() => RandomizeGradientsAndColors();

    void OnDestroy() => Destroy(_material);

    #endregion
}

} // namespace Rcam2
