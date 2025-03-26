using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public class ProjectileCam : GameCam {
        public Transform target1;
        private Vector3 t1Pos => target1.position;
        public Transform target2;
        private Vector3 t2Pos => target2.position;
    
        public float distanceMultiplier = 0.5f; // Adjusts distance sensitivity
        public float bufferFactor = 1.2f;
        public float bufferFactorOrtho = 1.2f;
    
        // public float zDistMultiplier = 5f;

        [Header("Perspective Settings")]
        [SerializeField] private float xOffset = 0f;
        [SerializeField] private float yOffset = 0f;
        [SerializeField] private float zOffset = 1f;

        [Header("Orthographic Settings")]
        [SerializeField] private bool _customZOffset = false;
        [SerializeField] private float _initialZOffset = -10f;
        private float _initialHeight;
        private float _initialWidth;
    
        private float _topHeight;
        private float _bottomHeight;
        private float _nextYPos;
        private float maxHeight => Mathf.Abs(_topHeight - _bottomHeight);
    
        private float _maxRight;
        private float _maxLeft;
        private float maxWidth => Mathf.Abs(_maxLeft - _maxRight);
    
        // private float _nextSize;
    
        [FormerlySerializedAs("minimumCameraSizeOrtho")] public float minimumCameraSizeCamSizeOrtho = 1f;
        private float minCamSizeOrtho => minimumCameraSizeCamSizeOrtho;

        private CameraManager _cm;
        private Transform tr => transform;
        // private Vector3 _pos => _tr.position;
        private Camera _cam;

        // Previously calculated values
        private float shellDist;
        private Vector3 midpoint;
        
        private void HandleShellLanded() {
            
        }

        private void HandleShellFired() {
            Debug.Log("Shell has been fired.");
        }

        private void HandleShellLoaded() {
            ResetCam();
        }

        private void ResetCam() {
            _topHeight = 0f;
            _bottomHeight = 0f;
            _maxLeft = 0f;
            _maxRight = 0f;
        }

        private void Awake() {
            _cam = GetComponent<Camera>();
        }

        private void Start() {
            _cm = CameraManager.Instance;
            _cm.RegisterCamera(this);
        
            if (target1 == null || target2 == null) tr.gameObject.SetActive(false);
        
            midpoint = Vector3.Lerp(t1Pos, t2Pos, 0.5f);
            _initialZOffset = tr.position.z - midpoint.z;
            _topHeight = Mathf.Max(t1Pos.y, t2Pos.y);
            _bottomHeight = Mathf.Min(t1Pos.y, t2Pos.y);
            _maxLeft = Mathf.Max(t1Pos.x, t2Pos.x);
            _maxRight = Mathf.Min(t1Pos.x, t2Pos.x);
        }

        void LateUpdate() {
            shellDist = Vector3.Distance(t1Pos, t2Pos);
            midpoint = Vector3.Lerp(t1Pos, t2Pos, 0.5f);
        
            _topHeight = Mathf.Max(t1Pos.y, t2Pos.y, _topHeight);
            _bottomHeight = Mathf.Min(t1Pos.y, t2Pos.y, _bottomHeight);
            _nextYPos = Mathf.Lerp(_topHeight, _bottomHeight, 0.5f);
        
            _maxLeft = Mathf.Max(t1Pos.x, t2Pos.x, _maxLeft);
            _maxRight = Mathf.Min(t1Pos.x, t2Pos.x, _maxRight);
        

            if (_cam.orthographic) {
                ManipulateOrtho();
            }
            else {
                ManipulatePerspective();
            }
        
        }

        private void ManipulateOrtho() {
            float newSize = Mathf.Max(shellDist, maxHeight, maxWidth) / 2f * bufferFactorOrtho;
            _cam.orthographicSize = Mathf.Max(newSize, minCamSizeOrtho);
            tr.position = new Vector3(midpoint.x, _nextYPos, _initialZOffset);
        }

        private void ManipulatePerspective() {
            float minDistance = shellDist / (Mathf.Tan(_cam.fieldOfView * distanceMultiplier * Mathf.Deg2Rad) * 2f);
            minDistance /= _cam.aspect;  // Adjust the distance based on the screen's aspect ratio
            minDistance *= bufferFactor;

            float newDistance = Mathf.Clamp(minDistance, 11, 1000);
            Vector3 offsetDir = new Vector3(xOffset, yOffset, zOffset).normalized;
        
            // Set the new camera pos, placing it at the correct distance along chosen axis

            // todo rewrite this part to account for different offsets
            Vector3 newCamPos = new Vector3(midpoint.x, _nextYPos, midpoint.z);
            newCamPos -= offsetDir * newDistance;
            tr.position = newCamPos;
            
            // transform.LookAt(midpoint);
        }
        
        private void OnEnable() {
            ShellEvent.OnShellLanded += HandleShellLanded;
            ShellEvent.OnShellFired += HandleShellFired;
            ShellEvent.OnShellLoaded += HandleShellLoaded;
        }

        private void OnDisable() {
            ShellEvent.OnShellLanded -= HandleShellLanded;
            ShellEvent.OnShellFired -= HandleShellFired;
            ShellEvent.OnShellLoaded -= HandleShellLoaded;
        }
    }
}