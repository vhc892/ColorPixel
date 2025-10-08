using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CoreInput : BaseInput
{
    private static float defaultZoomInSize;


    [Header("Paint Settings")]
    public Image paintCursorImage;
    [Range(0.1f, 1f)]
    public float cursorSmoothing = 0.5f;
    
    private SpriteRenderer spriteRenderer;

    // Touch helpers
    private bool isTouchOnUILayer = false;

    // Mouse helpers
    private bool isMouseOnUI = false;
    private Vector3 lastMousePosScreen;

    // Thêm:
    private bool hasLastPaintPos = false;
    private Vector3 lastPaintWorldPos;

    private bool hasMovedSincePress = false;

    private float holdStartTime = 0f;
    private bool waitingForHoldPaint = false;
    private Vector2 holdStartScreenPos;
    private const float holdThresholdTime = 1.5f;     
    private const float holdMoveTolerance = 10f;    

    private void OnEnable()
    {
        CoreGameManager.OnColorSelected += UpdateCursorColor;
    }

    private void OnDisable()
    {
        CoreGameManager.OnColorSelected -= UpdateCursorColor;
    }
    private void UpdateCursorColor(Color newColor)
    {
        paintCursorImage.color = newColor;
    }
    void Start()
    {
        defaultZoomInSize = zoomInSize;
        cam = InputHandler.Instance.mainCam;
        spriteRenderer = CoreGameManager.Instance.spriteRenderer; // dùng sprite để clamp theo biên ảnh
        paintCursorImage.gameObject.SetActive(false);
    }

    public override void SetUpCamera(bool isAnim = false)
    {
        cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Main Camera not found in the current scene!");
            return;
        }
        if (isAnim)
        {
            cam.transform.DOMove(initialCameraPosition, 0.5f).SetEase(Ease.InOutSine);
            cam.DOOrthoSize(zoomOutSize, 0.5f).SetEase(Ease.InOutSine);
        }
        else
        {
            cam.transform.position = initialCameraPosition;
            cam.orthographicSize = zoomOutSize;
        }
    }

    public void SwitchZoomInSize(int px)
    {
        zoomInSize = defaultZoomInSize * 64 / px;
    }

    // ===================== MOBILE (giữ nguyên logic hiện có) =====================
    public override void HandleMobileInput()
    {
        if (CoreGameManager.Instance.GetCurrentArtBoxSO() == null) return;
        if (CoreGameManager.Instance.GetCurrentArtBoxSO().isDone) return;

        if (Input.touchCount == 0 && PlayerManager.Instance.InputState != Helper.InputState.None)
        {
            PlayerManager.Instance.InputState = Helper.InputState.None;
        }

        if (Input.touchCount == 2 && (IsState(Helper.InputState.Zoom) || IsState(Helper.InputState.Drag)))
        {
            PlayerManager.Instance.InputState = Helper.InputState.Zoom;
            HandleZoom_Touch();
            CoreGameManager.Instance.UpdateBorderByScale(); // cập nhật viền số theo scale:contentReference[oaicite:5]{index=5}
        }
        else if (Input.touchCount == 1)
        {
            if (IsState(Helper.InputState.Painting))
            {
                HandlePaint_Touch();
                if (PlayerManager.Instance.InputState == Helper.InputState.Painting) return;
            }
            if (IsState(Helper.InputState.Drag) || IsState(Helper.InputState.Zoom))
            {
                HandleDrag_Touch();
            }
        }
    }

    private void HandlePaint_Touch()
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            if (IsPointerOverUI(touch.position)) isTouchOnUILayer = true;
            if (isTouchOnUILayer) return;

            Vector3 firstTouch = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0f));
            if (CoreGameManager.Instance.PaintAtPosition(firstTouch)) // tái dùng pipeline tô hiện có:contentReference[oaicite:6]{index=6}
            {
                PlayerManager.Instance.InputState = Helper.InputState.Painting;
                lastPaintWorldPos = firstTouch;
                hasLastPaintPos = true;
                hasMovedSincePress = false;
                AudioManager.Instance.PaintPixelSfx();
                paintCursorImage.color = CoreGameManager.Instance.currentColorBox.colorImage.color;

                paintCursorImage.transform.DOKill();
                paintCursorImage.gameObject.SetActive(true);
                paintCursorImage.transform.position = touch.position;
                paintCursorImage.transform.localScale = Vector3.zero;
                paintCursorImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
            else return;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            if (isTouchOnUILayer) return;
            if (paintCursorImage.gameObject.activeInHierarchy)
            {
                Vector3 targetPosition = touch.position;
                paintCursorImage.transform.position = Vector3.Lerp(paintCursorImage.transform.position, targetPosition, cursorSmoothing);
            }

            if (PlayerManager.Instance.InputState == Helper.InputState.Painting)
            {
                Vector3 worldNow = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0f));

                // Nếu chưa có điểm cũ (vừa bắt đầu kéo), khởi tạo
                if (!hasLastPaintPos)
                {
                    lastPaintWorldPos = worldNow;
                    hasLastPaintPos = true;
                }

                PaintAlongSegment(lastPaintWorldPos, worldNow);   // <-- quan trọng
                lastPaintWorldPos = worldNow;
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            PlayerManager.Instance.InputState = Helper.InputState.None;
            isTouchOnUILayer = false;
            hasLastPaintPos = false; // reset

            if (paintCursorImage != null)
            {
                paintCursorImage.gameObject.SetActive(false);
            }
        }
    }

    public override void HandleDrag_Touch()
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (IsPointerOverUI(touch.position)) isTouchOnUILayer = true;
            if (isTouchOnUILayer) return;
            PlayerManager.Instance.InputState = Helper.InputState.Drag;

            waitingForHoldPaint = true;
            holdStartTime = Time.time;
            holdStartScreenPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            if (isTouchOnUILayer) return;
            if (waitingForHoldPaint && Vector2.Distance(touch.position, holdStartScreenPos) > holdMoveTolerance)
            {
                waitingForHoldPaint = false;
            }

            Vector2 deltaPixel = touch.deltaPosition;
            ApplyScreenDeltaPan(deltaPixel);
        }
        else if (touch.phase == TouchPhase.Stationary)
        {
            if (isTouchOnUILayer) return;
            // giữ yên → kiểm tra thời gian
            if (waitingForHoldPaint && Time.time - holdStartTime >= holdThresholdTime)
            {
                waitingForHoldPaint = false;

                PlayerManager.Instance.InputState = Helper.InputState.Painting;
                hasMovedSincePress = false;
                AudioManager.Instance.PaintPixelSfx();

                paintCursorImage.color = CoreGameManager.Instance.currentColorBox.colorImage.color;
                paintCursorImage.transform.DOKill();
                paintCursorImage.gameObject.SetActive(true);
                paintCursorImage.transform.position = touch.position;
                paintCursorImage.transform.localScale = Vector3.zero;
                paintCursorImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            isTouchOnUILayer = false;
            PlayerManager.Instance.InputState = Helper.InputState.None;
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
    }

    // ===================== EDITOR / DESKTOP =====================
    public override void HandleEditorDesktopInput()
    {
        if (CoreGameManager.Instance.GetCurrentArtBoxSO() == null) return;
        if (CoreGameManager.Instance.GetCurrentArtBoxSO().isDone) return;
        // Reset state nếu không dùng chuột
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)
            && PlayerManager.Instance.InputState != Helper.InputState.None)
        {
            PlayerManager.Instance.InputState = Helper.InputState.None;
        }

        HandleZoom_Mouse();

        // Painting bằng Left Mouse
        if (IsState(Helper.InputState.Painting))
        {
            HandlePaint_Mouse();
            if (PlayerManager.Instance.InputState == Helper.InputState.Painting) return;
        }

        // Drag bằng Middle Mouse hoặc Right Mouse
        if (IsState(Helper.InputState.Drag))
        {
            HandleDrag_Mouse();
        }
    }

    private void HandlePaint_Mouse()
    {
        // Began
        if (Input.GetMouseButtonDown(0))
        {
            isMouseOnUI = IsPointerOverUI(Input.mousePosition);
            if (isMouseOnUI) return;

            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            if (CoreGameManager.Instance.PaintAtPosition(worldPos))
            {
                PlayerManager.Instance.InputState = Helper.InputState.Painting;
                hasMovedSincePress = false;
                AudioManager.Instance.PaintPixelSfx();

                paintCursorImage.color = CoreGameManager.Instance.currentColorBox.colorImage.color;

                paintCursorImage.transform.DOKill();
                paintCursorImage.gameObject.SetActive(true);
                paintCursorImage.transform.position = Input.mousePosition;
                paintCursorImage.transform.localScale = Vector3.zero;
                paintCursorImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
        }
        // Moved (hold)
        if (Input.GetMouseButton(0) && PlayerManager.Instance.InputState == Helper.InputState.Painting && !isMouseOnUI)
        {
            
            if (paintCursorImage.gameObject.activeInHierarchy)
            {
                Vector3 targetPosition = Input.mousePosition;
                paintCursorImage.transform.position = Vector3.Lerp(paintCursorImage.transform.position, targetPosition, cursorSmoothing);
            }
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            CoreGameManager.Instance.PaintAtPosition(worldPos);
        }
        // Ended
        if (Input.GetMouseButtonUp(0))
        {
            PlayerManager.Instance.InputState = Helper.InputState.None;
            isMouseOnUI = false;
            if (paintCursorImage != null)
            {
                paintCursorImage.gameObject.SetActive(false);
            }
        }
    }

    // Implement missing interface method
    public override void HandleZoom_Mouse()
    {
        // Example implementation: zoom using mouse scroll wheel
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            float newSize = Mathf.Clamp(cam.orthographicSize - scroll * (zoomSpeed * 50f), zoomInSize, zoomOutSize);
            cam.orthographicSize = newSize;

            CoreGameManager.Instance.UpdateBorderByScale();
        }
    }

    public override void HandleDrag_Mouse()
    {
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
        {
            isMouseOnUI = IsPointerOverUI(Input.mousePosition);
            if (isMouseOnUI) return;
            lastMousePosScreen = Input.mousePosition;
            PlayerManager.Instance.InputState = Helper.InputState.Drag;
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
            PlayerManager.Instance.InputState = Helper.InputState.None;
        }
    }

    // ===================== SHARED HELPERS =====================
    public void ApplyScreenDeltaPan(Vector2 deltaPixel)
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
        if (cam == null || spriteRenderer == null) return;

        Bounds bounds = spriteRenderer.bounds;

        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minY = bounds.min.y;
        float maxY = bounds.max.y;

        Vector3 pos = cam.transform.position;

        // Nếu camera to hơn sprite → giữ ở giữa
        if (minX > maxX || minY > maxY)
        {
            pos.x = bounds.center.x;
            pos.y = bounds.center.y;
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            cam.transform.position = pos;
        }
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
    private void PaintAlongSegment(Vector3 from, Vector3 to)
    {
        // Tính kích thước 1 pixel theo thế giới (world units)
        // dựa vào pixelsPerUnit và scale của sprite bạn đang clamp/pan theo nó
        float ppu = spriteRenderer.sprite.pixelsPerUnit;
        float worldPerPixel = (1f / ppu) * spriteRenderer.transform.lossyScale.x;

        // bước vẽ ~ 0.75 pixel để chồng lấp nhẹ, tránh lọt ô ở tốc độ cao
        float step = worldPerPixel * 0.75f;

        float dist = Vector3.Distance(from, to);
        if (dist <= 0f)
        {
            CoreGameManager.Instance.PaintAtPosition(from);
            return;
        }

        int steps = Mathf.CeilToInt(dist / step);

        // Nội suy các điểm trung gian và vẽ
        for (int i = 0; i <= steps; i++)
        {
            float t = (steps == 0) ? 1f : (float)i / steps;
            Vector3 p = Vector3.Lerp(from, to, t);
            CoreGameManager.Instance.PaintAtPosition(p);
        }
    }

}
