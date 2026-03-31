using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class AdjustCameraHeight : MonoBehaviour
{
    [SerializeField] float maxHeight = 2.0f;
    [SerializeField] float minHeight = 0.9f;
    [SerializeField] InputActionReference leftJoystick;
    [SerializeField] InputActionReference buttonY;
    [SerializeField] XROrigin xrOrigin;

    static float yOffset = 1.1f;

    void Start()
    {
        xrOrigin.CameraYOffset = yOffset;
    }

    void Update()
    {
        AdjustHeight();
    }

    void AdjustHeight()
    {
        if (!buttonY.action.inProgress) return;

        float value = leftJoystick.action.ReadValue<Vector2>().y;
        
        if (Mathf.Abs(value) < 0.1f) return;

        yOffset += value * 0.015f;
        yOffset = Mathf.Clamp(yOffset, minHeight, maxHeight);
        xrOrigin.CameraYOffset = yOffset;
    }
}