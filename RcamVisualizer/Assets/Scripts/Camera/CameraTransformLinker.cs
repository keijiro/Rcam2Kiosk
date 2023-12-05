using UnityEngine;

namespace Rcam2 {

sealed class CameraTransformLinker : MonoBehaviour
{
    // Keyframes for interpolation
    (Vector3 p, Quaternion r, float t) _key1, _key2;

    // Transform update with interpolation
    void UpdateTransformLerped(Vector3 position, Quaternion rotation)
    {
        // Current time, extrapolated time of next frame
        var (t, nt) = (Time.time, Time.time + Time.deltaTime);

        // Keyframe update
        if (_key2.p != position)
        {
            _key1 = _key2;
            _key2 = (position, rotation, t);
        }

        // Interpolation parameter
        var ip = Mathf.Clamp01((nt - _key2.t) / (_key2.t - _key1.t));

        // Transform update
        transform.position = Vector3.Lerp(_key1.p, _key2.p, ip);
        transform.rotation = Quaternion.Slerp(_key1.r, _key2.r, ip);
    }

    void LateUpdate()
    {
        var recv = Singletons.Receiver;
        UpdateTransformLerped(recv.CameraPosition, recv.CameraRotation);
    }
}

} // namespace Rcam2
