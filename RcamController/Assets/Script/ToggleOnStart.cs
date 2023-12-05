using UnityEngine;
using Klak.VJUI;

namespace Rcam2 {

sealed class ToggleOnStart : MonoBehaviour
{
    void Start()
      => GetComponent<Toggle>().isOn ^= true;
}

} // namespace Rcam2
