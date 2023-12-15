using UnityEngine;

namespace Rcam2 {

public sealed class SceneInputAdapter : MonoBehaviour
{
    #region Editable properties

    [field:Space]
    [field:SerializeField] public InputHandle Input { get; set; }
    [field:SerializeField] public float AutoCycleInterval { get; set; } = 10;

    #endregion

    #region Public accessors

    public int ForegroundVfxIndex { get; private set; }
    public bool ForegroundVfxChanged { get; private set; }
    public bool ThirdPersonActivated => Input.GetToggle(3);
    public bool AutoCycleActivated => Input.GetToggle(4);

    public bool GetBackgroundVfxToggle(int index)
      => Input.GetToggle(index);

    #endregion

    #region MonoBehaviour implementation

    async void Start()
    {
        while (true)
        {
            for (var i = 0; i < 8 - 2; i++) // exclude last two fx
            {
                await Awaitable.WaitForSecondsAsync(AutoCycleInterval);

                if (AutoCycleActivated)
                {
                    ForegroundVfxIndex = i;
                    ForegroundVfxChanged = true;

                    await Awaitable.NextFrameAsync();
                    ForegroundVfxChanged = false;
                }
            }
        }
    }

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
