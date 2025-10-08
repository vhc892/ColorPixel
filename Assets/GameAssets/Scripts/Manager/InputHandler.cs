using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance;
    private BaseInput currentInput;

    [Header("Camera")]
    public Camera mainCam;

    [Header("Input")]
    [SerializeField] private CoreInput coreInput;
    [SerializeField] private EventInput eventInput;

    [Header("Input Option")]
    [SerializeField] private bool forMobile;

    private Vector3 winCameraPos = new Vector3(0, -1.5f, -10);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    void Update()
    {
        if (currentInput == null) return;
        if (forMobile)
            HandleMobileInput();
        else
            HandleEditorDesktopInput();
    }

    public void TurnOffGameplayInput()
    {
        currentInput = null;
    }

    public void SwitchToCoreInput()
    {
        currentInput = coreInput;
    }

    public void SwitchToEventInput()
    {
        currentInput = eventInput;
    }

    public void HandleZoom_Touch()
    {
        if (currentInput == null) return;
        currentInput.HandleZoom_Touch();
    }

    public void HandleDrag_Touch()
    {
        if (currentInput == null) return;
        currentInput.HandleDrag_Touch();
    }

    public void HandleMobileInput()
    {
        if (currentInput == null) return;
        currentInput.HandleMobileInput();
    }

    public void HandleZoom_Mouse()
    {
        if (currentInput == null) return;
        currentInput.HandleZoom_Mouse();
    }

    public void HandleDrag_Mouse()
    {
        if (currentInput == null) return;
        currentInput.HandleDrag_Mouse();
    }

    public void HandleEditorDesktopInput()
    {
        if (currentInput == null) return;
        currentInput.HandleEditorDesktopInput();
    }

    public void SetUpCamera(bool isAnim = false)
    {
        if (currentInput == null) return;
        currentInput.SetUpCamera(isAnim);
    }

    public void SwitchZoomInSize(int spriteSize)
    {
        if (currentInput == null) return;
        coreInput.SwitchZoomInSize(spriteSize);
    }

    public float GetZoomInSize()
    {
        if (currentInput == null) return 1;
        return currentInput.zoomInSize;
    }

    public Vector3 GetInitialCameraPosition()
    {
        if (currentInput == null) return new Vector3(0, 0, -10);
        return currentInput.initialCameraPosition;
    }

    public float GetInitialCameraSize()
    {
        if (currentInput == null) return 5;
        return currentInput.initialCameraSize;
    }

    public float GetZoomOutSize()
    {
        if (currentInput == null) return 5;
        return currentInput.zoomOutSize;
    }
    
    public Vector3 GetWinCameraPosition()
    {
        return winCameraPos;
    }
}
