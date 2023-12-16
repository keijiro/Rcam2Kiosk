using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rcam2 {

public sealed class SceneInputAdapter : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField] public float AutoCycleInterval { get; set; } = 10;

    #endregion

    #region Public accessors

    public int ForegroundVfxIndex { get; private set; }
    public bool ForegroundVfxChanged { get; private set; }
    public bool ThirdPersonActivated => Singletons.InputHandle.GetToggle(3);
    public bool AutoCycleActivated => Singletons.InputHandle.GetToggle(4);

    public bool GetBackgroundVfxToggle(int index)
      => Singletons.InputHandle.GetToggle(index);

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
            if (Singletons.InputHandle.GetButton(i)) ForegroundVfxIndex = i;

        ForegroundVfxChanged = (ForegroundVfxIndex != prev);

        // Reset button
        if (Singletons.InputHandle.GetButton(15)) SceneManager.LoadScene(0);
    }

    #endregion
}

} // namespace Rcam2
