using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventInput : BaseInput
{

    // Touch helpers
    private bool isTouchOnUILayer = false;

    // Mouse helpers
    private bool isMouseOnUI = false;
    private Vector3 lastMousePosScreen;

    void Start()
    {
        cam = InputHandler.Instance.mainCam;
    }

    public override void SetUpCamera(bool isAnim = false)
    {
        if (isAnim)
        {
            cam.transform.DOMove(initialCameraPosition, 0.5f).SetEase(Ease.InOutSine);
            cam.DOOrthoSize(initialCameraSize, 0.5f).SetEase(Ease.InOutSine);
        }
        else
        {
            cam.transform.position = initialCameraPosition;
            cam.orthographicSize = zoomOutSize;
        }
    }

    // ===================== MOBILE (giữ nguyên logic hiện có) =====================
    public override void HandleMobileInput()
    {
        if (Input.touchCount == 2)
        {
            HandleZoom_Touch();
        }
        else if (Input.touchCount == 1)
        {
            HandleDrag_Touch();
        }
    }

    public override void HandleDrag_Touch()
    {
        Touch touch = Input.GetTouch(0);
        Debug.Log(touch.position);
        Debug.Log(touch.deltaPosition);
        if (touch.phase == TouchPhase.Began)
        {
            // if (IsPointerOverUI(touch.position)) isTouchOnUILayer = true;
            // if (isTouchOnUILayer) return;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            if (isTouchOnUILayer) return;

            Vector2 deltaPixel = touch.deltaPosition;
            ApplyScreenDeltaPan(deltaPixel);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            isTouchOnUILayer = false;
        }
    }

    public override void HandleZoom_Touch()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currentMagnitude = (touch0.position - touch1.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        float newSize = Mathf.Clamp(cam.orthographicSize - difference * zoomSpeed, zoomInSize, zoomOutSize);
        
        cam.orthographicSize = newSize;
        // (Clamp camera khi cần)
        ClampCamera();
    }

    // ===================== EDITOR / DESKTOP =====================
    public override void HandleEditorDesktopInput()
    {
        // Reset state nếu không dùng chuột
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)
            && PlayerManager.Instance.InputState != Helper.InputState.None)
        {
            PlayerManager.Instance.InputState = Helper.InputState.None;
        }

        HandleZoom_Mouse();

        // Drag bằng Middle Mouse hoặc Right Mouse
        if (IsState(Helper.InputState.Drag))
        {
            HandleDrag_Mouse();
        }
    }

    public override void HandleZoom_Mouse()
    {
        // Example implementation: zoom using mouse scroll wheel
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            float newSize = Mathf.Clamp(cam.orthographicSize - scroll * (zoomSpeed * 50f), zoomInSize, zoomOutSize);
            cam.orthographicSize = newSize;
            CoreGameManager.Instance.UpdateBorderByScale();
            ClampCamera();
        }
    }

    public override void HandleDrag_Mouse()
    {
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
        {
            isMouseOnUI = IsPointerOverUI(Input.mousePosition);
            if (isMouseOnUI) return;
            lastMousePosScreen = Input.mousePosition;
        }
        // drag
        if ((Input.GetMouseButton(2) || Input.GetMouseButton(1)) && !isMouseOnUI)
        {
            Vector2 deltaPixel = (Vector2)(Input.mousePosition - lastMousePosScreen);
            lastMousePosScreen = Input.mousePosition;

            ApplyScreenDeltaPan(deltaPixel);
        }
        // end drag
        if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1))
        {
            isMouseOnUI = false;
        }
    }

    // ===================== SHARED HELPERS =====================
    private void ApplyScreenDeltaPan(Vector2 deltaPixel)
    {
        // Tính world-per-pixel dựa trên kích thước camera & aspect (giống touch):contentReference[oaicite:8]{index=8}
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float worldPerPixelX = camWidth / Screen.width;
        float worldPerPixelY = camHeight / Screen.height;

        // Kéo như map: dịch chuyển ngược hướng kéo chuột/ngón tay
        Vector3 delta = new Vector3(-deltaPixel.x * worldPerPixelX,
                                    -deltaPixel.y * worldPerPixelY,
                                    0);

        float t = Mathf.InverseLerp(zoomOutSize, zoomInSize, cam.orthographicSize);
        float speed = Mathf.Lerp(dragSpeedMin, dragSpeedMax, t);

        cam.transform.position += delta * speed;

        ClampCamera(); // giữ camera trong biên sprite:contentReference[oaicite:9]{index=9}
    }

    private void ClampCamera()
    {
        if (cam == null || EventGameManager.Instance.GetEventBG() == null) return;

        Bounds bounds = EventGameManager.Instance.GetEventBG().bounds;

        // Nửa chiều cao camera
        float vertExtent = cam.orthographicSize;
        // Nửa chiều rộng camera (theo tỉ lệ màn hình)
        float horzExtent = vertExtent * cam.aspect;

        Vector3 pos = cam.transform.position;

        // Nếu camera to hơn sprite → giữ camera ở giữa sprite
        if (bounds.size.x <= horzExtent * 2f && bounds.size.y <= vertExtent * 2f)
        {
            pos.x = bounds.center.x;
            pos.y = bounds.center.y;
        }
        else
        {
            float minX = bounds.min.x + horzExtent;
            float maxX = bounds.max.x - horzExtent;
            float minY = bounds.min.y + vertExtent;
            float maxY = bounds.max.y - vertExtent;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        cam.transform.position = pos;
    }


    private bool IsState(Helper.InputState inputState)
    {
        // Giữ logic: cho phép chuyển khi đang None hoặc đúng state mục tiêu:contentReference[oaicite:10]{index=10}
        return PlayerManager.Instance.InputState == Helper.InputState.None
            || PlayerManager.Instance.InputState == inputState;
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        // Dùng EventSystem để phát hiện UI cho cả touch & mouse:contentReference[oaicite:11]{index=11}
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Theo mặc định code cũ dùng layer name "UI"
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
}
