using UnityEngine;

namespace Rcam2 {

sealed class ProjectionMatrixLinker : MonoBehaviour
{
    Camera _camera;

    void Start()
      => _camera = GetComponent<Camera>();

    void Update()
    {
        // Calculate the camera parameters from the projection matrix.
        // This doesn't take any effect but gives better compatibility with
        // component depending on these parameters.
        var m = Singletons.Receiver.ProjectionMatrix;
        var (h, z, w) = (m[1, 1], m[2, 2], m[2, 3]);
        _camera.nearClipPlane = w / (z - 1);
        _camera.farClipPlane = w / (z + 1);
        _camera.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(1 / h) * 2;

        // Overwrite the projection matrix.
        _camera.projectionMatrix = Singletons.Receiver.ProjectionMatrix;
    }
}

} // namespace Rcam2
