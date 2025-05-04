using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

namespace _Main.Scripts
{
    public class ManagerInputs : MonoBehaviour
    {
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

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
    #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouse();
    #endif
            HandleTouch();
        }

        private void HandleMouse()
        {
            // press down
            if (Input.GetMouseButtonDown(0))
                BeginPress(Input.mousePosition, null);

            // hold
            if (isPressing && Input.GetMouseButton(0))
                ContinuePress(Input.mousePosition);

            // release
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

            // check if the press started over UI
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

            // disable camera rotation until we decide it's a drag
            RotationInputProvider.enabled = false;
        }

        private void ContinuePress(Vector2 pos)
        {
            // only enable drag-rotation if:
            //  • not already rotating
            //  • NOT over UI
            //  • movement exceeds threshold
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

            // only raycast on a quick tap, and only if NOT over UI
            if (!pressOverUI
                && !rotationEnabled
                && duration <= maxTapTime
                && distance <= maxTapMovement)
            {
                CheckHit(pos);
            }

            // reset state
            isPressing       = false;
            rotationEnabled  = false;
            currentPointerId = null;
            pressOverUI      = false;
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
    }
}
