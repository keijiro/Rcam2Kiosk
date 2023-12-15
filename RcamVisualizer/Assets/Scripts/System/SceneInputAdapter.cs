using UnityEngine;

namespace Rcam2 {

public sealed class SceneInputAdapter : MonoBehaviour
{
    #region Editable properties

    [field:Space]
    [field:SerializeField] public InputHandle Input { get; set; }

    #endregion

    #region Public accessors

    public int ForegroundVfxIndex { get; private set; }
    public bool ForegroundVfxChanged { get; private set; }
    public bool ThirdPersonActivated => Input.GetToggle(3);

    public bool GetBackgroundVfxToggle(int index)
      => Input.GetToggle(index);

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        var prev = ForegroundVfxIndex;

        for (var i = 0; i < 8; i++)
            if (Input.GetButton(i)) ForegroundVfxIndex = i;

        ForegroundVfxChanged = (ForegroundVfxIndex != prev);
    }

    #endregion
}

} // namespace Rcam2
