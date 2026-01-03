using UnityEngine;

public class BeaconBillboard : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (_mainCamera == null) return;

        transform.rotation = _mainCamera.transform.rotation;
    }
}