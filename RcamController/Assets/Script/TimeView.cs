using UnityEngine;
using UnityEngine.UI;

namespace Rcam2 {

sealed class TimeView : MonoBehaviour
{
    [SerializeField] Text _timeText = null;

    void Update()
      => _timeText.text = System.DateTime.Now.ToString("HH:mm:ss");
}

} // namespace Rcam2
