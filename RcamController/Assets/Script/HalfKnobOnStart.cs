using UnityEngine;
using Klak.VJUI;

namespace Rcam2 {

sealed class HalfKnobOnStart : MonoBehaviour
{
    void Start()
      => GetComponent<Knob>().value = 0.5f;
}

} // namespace Rcam2
