using UnityEngine;
using UnityEngine.VFX;

namespace Rcam2 {

public sealed class SceneCameraController : MonoBehaviour
{
    #region Editable properties

    [field:Space]
    [field:SerializeField] public InputHandle Input { get; set; }

    [field:Space]
    [field:SerializeField] public Transform Target { get; set; }
    [field:SerializeField] public Transform LockPoint { get; set; }
    [field:SerializeField] public Transform Floater { get; set; }

    [field:Space]
    [field:SerializeField] public GameObject Background { get; set; }
    [field:SerializeField] public GameObject Recolor { get; set; }
    [field:SerializeField] public GameObject PointCloud { get; set; }

    #endregion

    #region Private objects

    bool _floating;

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        var prevFloating = _floating;

        // Button input
        var locked = Input.GetButton(8);
        var unlocked = Input.GetButton(9);
        _floating = locked ? false : (unlocked ? true : prevFloating);

        if (_floating != prevFloating)
        {
            if (_floating)
            {
                Target.parent = Floater;
                Target.localPosition = Vector3.zero;
                Target.localRotation = Quaternion.identity;
            }
            else
            {
                Target.parent = LockPoint;
                Target.localPosition = Vector3.zero;
                Target.localRotation = Quaternion.identity;
            }

            Background.SetActive(!_floating);
            Recolor.SetActive(!_floating);
            PointCloud.SetActive(_floating);
        }
    }

    #endregion
}

} // namespace Rcam2
