using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace _Scripts {
    public class TerminalUIEvents : MonoBehaviour {
        public static TerminalUIEvents instance { get; private set; }
        
        private GameManager _gm;
        private MortarController _mc;
        private Shell _shell;
        private Rigidbody _shellRb;
        private Transform shellTf => _shell.transform;
        
        private UIDocument _doc;
        private Label _firingAngle;
        private string _fAPrefix;
        private Label _mortarRotation;
        private string _mRPrefix;
        private Label _shellSpeed;
        private string _sSPrefix;
        private Label _shellDistance;
        private string _sDPrefix;
        
        private float _timer = 0f;
        public float updateInterval = 0.5f;
        
        // Map stuff
        private VisualElement _topoMap;
        public Terrain Terrain;
        public GameObject MapCursor;
        
        private Label _distanceLabel;
        private Label _angleLabel;
        private VisualElement _playerIcon;
        private VisualElement _cursorPoint;
        private VisualElement _extractPoint;
        private VisualElement _targetPoint;
        private VisualElement _shellPath;

        private void Awake() {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;
            
            _doc = GetComponent<UIDocument>();
            InitLabels();
            InitMap();

            _timer = updateInterval;
        }

        private void Start() {
            _gm = GameManager.instance;
            _mc = _gm.mortar;
            _shell = _mc.currentShell;
            _shellRb = _shell.GetComponent<Rigidbody>();

            UpdatePoints();
            UpdatePoints();
            UpdateZonePoints(_gm._zones, _gm._extractZone);
            UpdateZonePoints(_gm._zones, _gm._extractZone);
        }

        private void InitMap() {
            _topoMap = _doc.rootVisualElement.Q("TopoMap");
            
            _topoMap.RegisterCallback<MouseDownEvent>(evt =>
            {
                // Get the point clicked on the map
                Vector2 mousePos = evt.localMousePosition;
                Vector3 worldPos = ConvertMapToWorldPosition(mousePos);
                Vector3 mapPos = ConvertWorldToMapPosition(worldPos);
                Debug.Log($"Mouse Pos: {mousePos}");
                Debug.Log($"World Position: {worldPos}");
                Debug.Log($"Map Position: {mapPos}");
                
                MapCursor.transform.position = worldPos;
                SetElementPositionOnMap(worldPos, _cursorPoint);
                _cursorPoint.style.left = mapPos.x;
                _cursorPoint.style.top = mapPos.y;
            });
            
            _distanceLabel = _doc.rootVisualElement.Q<Label>("distance");
            _angleLabel = _doc.rootVisualElement.Q<Label>("angle");

            _playerIcon = _doc.rootVisualElement.Q<VisualElement>("playerIcon");
            _cursorPoint = _doc.rootVisualElement.Q<VisualElement>("cursorPoint");
            _extractPoint = _doc.rootVisualElement.Q<VisualElement>("extractPoint");
            _targetPoint = _doc.rootVisualElement.Q<VisualElement>("targetPoint");
            _shellPath = _doc.rootVisualElement.Q<VisualElement>("shellPath");

            // Debug.Log($"Distance Label: {_distanceLabel}");
            // Debug.Log($"Angle Label: {_angleLabel}");
            // Debug.Log($"Player Icon: {_playerIcon}");
            // Debug.Log($"Cursor Point: {_cursorPoint}");
            // Debug.Log($"Extract Point: {_extractPoint}");
            // Debug.Log($"Target Point: {_targetPoint}");
            // Debug.Log($"Shell Path: {_shellPath}");
        }

        private void UpdatePoints() {
            SetElementPositionOnMap(_mc.gameObject.transform.position, _playerIcon);
            _playerIcon.style.rotate = new Rotate(_mc.gameObject.transform.eulerAngles.y - 270);
        }

        public void UpdateZonePoints(Dictionary<int, Zone> zoneDict) {
            foreach (KeyValuePair<int, Zone> kvp in zoneDict) {
                SetElementPositionOnMap(kvp.Value.transform.position, _targetPoint);
            }
        }

        public void UpdateZonePoints(Dictionary<int, Zone> zoneDict, Zone extractZone) {
            UpdateZonePoints(zoneDict);
            
            SetElementPositionOnMap(extractZone.transform.position, _extractPoint);
        }
        
        private void Update() {
            // _timer += Time.deltaTime;
            // if (_timer >= updateInterval) {
            if (!_shell) _shell = _mc.currentShell;
            if (!_shellRb) _shellRb = _shell.GetComponent<Rigidbody>();
            
            UpdateDataText();
            //     _timer = 0f;
            // }
            
            UpdatePoints();
            UpdateZonePoints(_gm._zones, _gm._extractZone);
        }
        
        Vector3 ConvertMapToWorldPosition(Vector2 mapPos)
        {
            float mapWidth = _topoMap.resolvedStyle.width;
            float mapHeight = _topoMap.resolvedStyle.height;

            float terrainWidth = Terrain.terrainData.size.x;
            float terrainHeight = Terrain.terrainData.size.z;

            // Normalize mapPos from [0, mapWidth] and [0, mapHeight] to [0, 1]
            float normalizedX = (mapPos.x / mapWidth);
            float normalizedY = 1f - (mapPos.y / mapHeight); // Invert Y because UI toolkit Y is down, world Z is up

            // Convert normalized coords to world coords centered at (0,0)
            float worldX = (normalizedX - 0.5f) * terrainWidth;
            float worldZ = (normalizedY - 0.5f) * terrainHeight;

            return new Vector3(worldX, 0f, worldZ);
        }

        Vector2 ConvertWorldToMapPosition(Vector3 worldPos)
        {
            float mapWidth = _topoMap.resolvedStyle.width;
            float mapHeight = _topoMap.resolvedStyle.height;

            float terrainWidth = Terrain.terrainData.size.x;
            float terrainHeight = Terrain.terrainData.size.z;

            // Normalize world position from center
            float normalizedX = (worldPos.x / terrainWidth) + 0.5f;
            float normalizedY = (worldPos.z / terrainHeight) + 0.5f;

            // Invert normalizedY because UI Y is downward
            normalizedY = 1f - normalizedY;

            // Convert normalized coords to map coords
            float mapPosX = normalizedX * mapWidth;
            float mapPosY = normalizedY * mapHeight;

            return new Vector2(mapPosX, mapPosY);
        }

        
        void SetElementPositionOnMap(Vector3 worldPos, VisualElement element)
        {
            // Convert world position to map position (UI coordinates)
            Vector2 mapPos = ConvertWorldToMapPosition(worldPos);
            
            Debug.Log($"SetElementPositionOnMap: WorldPos: {worldPos} -> MapPos: {mapPos}");

            // Set the element's position in the UI using the calculated map coordinates
            element.style.left = mapPos.x;
            element.style.top = mapPos.y;
        }


        private void InitLabels() {
            _firingAngle = _doc.rootVisualElement.Q("firingAngle") as Label;
            _mortarRotation = _doc.rootVisualElement.Q("mortarRotation") as Label;
            _shellSpeed = _doc.rootVisualElement.Q("shellSpeed") as Label;
            _shellDistance = _doc.rootVisualElement.Q("shellDistance") as Label;
            
            // Debug.Log($"fa text: {_firingAngle}");

            _fAPrefix = _firingAngle.text;
            _mRPrefix = _mortarRotation.text;
            _sSPrefix = _shellSpeed.text;
            _sDPrefix = _shellDistance.text;
            
            // Debug.Log($"fa text: {_fAPrefix}");
        }

        private float Round(float num) {
            return Mathf.Round(num * 10f) / 10f;
        }

        

        private void UpdateDataText() {
            _firingAngle.text = _fAPrefix + $" {Round(_mc.firingAngle)}°";
            _mortarRotation.text = _mRPrefix + $" {Round(_mc.rotationAngle)}°";
            
            float dist = Vector3.Distance(_mc.gameObject.transform.position, shellTf.position);
            if (dist < 5f) dist = 0;
            _shellDistance.text = 
                _sDPrefix + $" {Round(dist)} METERS";
            
            _shellSpeed.text = _sSPrefix + $" {Round(_shellRb.velocity.magnitude)} M/S";

            float cursorDist = Vector3.Distance(_mc.gameObject.transform.position, MapCursor.transform.position);
            _distanceLabel.text = $"{Round(cursorDist)} m";
            
            Vector3 delta = MapCursor.transform.position - _mc.gameObject.transform.position;
            Vector2 flatDelta = new Vector2(delta.x, delta.z); // Ignore Y
            
            float angle = Mathf.Atan2(flatDelta.y, flatDelta.x) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f; // Ensure angle is always 0-360 degrees

            _angleLabel.text = $"{Round(angle)}°";
            
        }

        // debugStrings.Add($"Shell Position: {shellTf.position}");
        // debugStrings.Add($"Shell Rotation: {shellTf.eulerAngles}");
        // debugStrings.Add($"Shell Velocity: {_shellRb.velocity}");
        // debugStrings.Add($"Shell Speed: {_shellRb.velocity.magnitude}");
        // debugStrings.Add($"Mortar firing angle: {Mathf.Round(_mc.firingAngle * 10f) / 10f}° ");
        // debugStrings.Add($"Mortar rotation angle: {Mathf.Round(_mc.rotationAngle * 10f) / 10f}");
        // debugStrings.Add($"Shell Distance: {Vector3.Distance(_mc.gameObject.transform.position, _shell.transform.position)}m");
        
        
    }
}