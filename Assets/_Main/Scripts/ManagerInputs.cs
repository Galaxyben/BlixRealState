using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

namespace _Main.Scripts
{
    public class ManagerInputs : MonoBehaviour
    {
        [Header("Camera & Zoom Settings")]
        [Tooltip("Your Cinemachine Virtual Camera for zoom adjustments.")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [Tooltip("Speed multiplier for scroll-wheel zoom.")]
        [SerializeField] private float scrollZoomSpeed = 10f;
        [Tooltip("Speed multiplier for pinch zoom.")]
        [SerializeField] private float pinchZoomSpeed  = 0.1f;
        [Tooltip("Min (x) and Max (y) Field-of-View values.")]
        [SerializeField] private Vector2 fovLimits     = new Vector2(15f, 60f);

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CinemachineInputAxisController RotationInputProvider;

        [Header("Tap vs Drag Settings")]
        [SerializeField, Tooltip("Max time (s) for a press to count as a tap.")]
        private float maxTapTime = 0.25f;
        [SerializeField, Tooltip("Max movement (px) for a press to count as a tap.")]
        private float maxTapMovement = 10f;

        // runtime state
        private bool isPressing;
        private bool rotationEnabled;
        private bool pressOverUI;
        private Vector2 pressStartPos;
        private float pressStartTime;
        private int? currentPointerId;

        // pinch state
        private float previousPinchDistance;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            // auto-find the virtual camera if not set
            if (virtualCamera == null && RotationInputProvider != null)
                virtualCamera = RotationInputProvider.GetComponentInParent<CinemachineCamera>();
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouse();
#endif
            HandleTouch();
            HandleZoom();
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
                BeginPress(Input.mousePosition, null);

            if (isPressing && Input.GetMouseButton(0))
                ContinuePress(Input.mousePosition);

            if (isPressing && Input.GetMouseButtonUp(0))
                EndPress(Input.mousePosition);
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 0) return;

            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    BeginPress(t.position, t.fingerId);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isPressing)
                        ContinuePress(t.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isPressing)
                        EndPress(t.position);
                    break;
            }
        }

        private void BeginPress(Vector2 pos, int? pointerId)
        {
            isPressing      = true;
            rotationEnabled = false;
            pressStartPos   = pos;
            pressStartTime  = Time.time;
            currentPointerId = pointerId;

            if (EventSystem.current != null)
            {
                pressOverUI = pointerId.HasValue
                    ? EventSystem.current.IsPointerOverGameObject(pointerId.Value)
                    : EventSystem.current.IsPointerOverGameObject();
            }
            else
            {
                pressOverUI = false;
            }

            RotationInputProvider.enabled = false;
        }

        private void ContinuePress(Vector2 pos)
        {
            if (!rotationEnabled
                && !pressOverUI
                && Vector2.Distance(pos, pressStartPos) > maxTapMovement)
            {
                rotationEnabled = true;
                RotationInputProvider.enabled = true;
            }
        }

        private void EndPress(Vector2 pos)
        {
            float duration = Time.time - pressStartTime;
            float distance = Vector2.Distance(pos, pressStartPos);

            if (!pressOverUI
                && !rotationEnabled
                && duration <= maxTapTime
                && distance <= maxTapMovement)
            {
                CheckHit(pos);
            }

            // reset
            isPressing      = false;
            rotationEnabled = false;
            currentPointerId = null;
            pressOverUI     = false;
            RotationInputProvider.enabled = false;
        }

        private void CheckHit(Vector2 screenPos)
        {
            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity,
                                ~0, QueryTriggerInteraction.Collide))
            {
                var dept = hit.collider.GetComponent<DepartmentIdentifier>();
                if (dept != null)
                {
                    Debug.Log($"Tapped on department: {dept.gameObject.name}");
                    ManagerUI.Instance.SpawnDepartment();
                }
            }
        }

        private void HandleZoom()
        {
            // 1) Mouse scroll
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0f && virtualCamera != null)
                ApplyZoom(scroll * scrollZoomSpeed);
#endif

            // 2) Two-finger pinch
            if (Input.touchCount == 2 && virtualCamera != null)
            {
                Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
                float currentDist = Vector2.Distance(t0.position, t1.position);

                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    previousPinchDistance = currentDist;
                }
                else
                {
                    float delta = currentDist - previousPinchDistance;
                    ApplyZoom(delta * pinchZoomSpeed);
                    previousPinchDistance = currentDist;
                }
            }
        }

        private void ApplyZoom(float deltaFov)
        {
            var lens = virtualCamera.Lens;
            lens.FieldOfView = Mathf.Clamp(lens.FieldOfView - deltaFov, fovLimits.x, fovLimits.y);
            virtualCamera.Lens = lens;
        }
    }
}
