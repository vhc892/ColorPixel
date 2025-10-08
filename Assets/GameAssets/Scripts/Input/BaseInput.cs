using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInput : MonoBehaviour
{
    [Header("Initial Camera Settings")]
    public Vector3 initialCameraPosition = new Vector3(0, 0, -10);
    public float initialCameraSize = 6f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.01f;       // touch pinch speed
    public float zoomOutSize = 6f;        // max ortho size
    public float zoomInSize = 0.5f;       // min ortho size

    [Header("Drag Settings")]
    public float dragSpeedMin = 0.2f;     // when fully zoomed out
    public float dragSpeedMax = 1f;       // when fully zoomed in
    
    public Camera cam;

    public abstract void HandleZoom_Touch();
    public abstract void HandleDrag_Touch();
    public abstract void HandleMobileInput();
    public abstract void HandleZoom_Mouse();
    public abstract void HandleDrag_Mouse();
    public abstract void HandleEditorDesktopInput();
    public abstract void SetUpCamera(bool isAnim = false);
}
