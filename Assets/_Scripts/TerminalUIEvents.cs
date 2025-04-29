using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts {
    public class TerminalUIEvents : MonoBehaviour {
        public float updateInterval = 0.5f;
        public Terrain Terrain;
        public GameObject MapCursor;
        private Label _angleLabel;
        private VisualElement _cursorPoint;

        private Label _distanceLabel;

        private UIDocument _doc;
        private VisualElement _extractPoint;
        private string _fAPrefix;
        private Label _firingAngle;

        private GameManager _gm;
        private MortarController _mc;
        private Label _mortarRotation;
        private string _mRPrefix;
        private VisualElement _playerIcon;
        private string _sDPrefix;
        private Shell _shell;
        private Label _shellDistance;
        private Label _shellHeight;
        private VisualElement _shellIcon;
        private VisualElement _shellPath;
        private Rigidbody _shellRb;
        private string _sHPrefix;
        private VisualElement _targetPointOriginal;

        private Label _objList;

        private readonly Dictionary<int, VisualElement> _targetPointsDict = new();

        private float _timer;

        // Map stuff
        private VisualElement _topoMap;
        public static TerminalUIEvents instance { get; private set; }
        private Transform shellTf => _shell.transform;

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
        }

        private void Update() {
            _timer += Time.deltaTime;
            if (_timer >= updateInterval) {
                if (!_shell) _shell = _mc.currentShell;
                if (!_shellRb) _shellRb = _shell.GetComponent<Rigidbody>();

                UpdateDataText();
                UpdateObjList();
                UpdatePoints();
                UpdateZonePoints(_gm._zones, _gm._extractZone);
                _timer = 0f;
            }

            // UpdatePoints();
            // UpdateZonePoints(_gm._zones, _gm._extractZone);
            // UpdateObjList();
        }

        private void InitMap() {
            _topoMap = _doc.rootVisualElement.Q("TopoMap");

            _topoMap.RegisterCallback<MouseDownEvent>(evt => {
                // Get the point clicked on the map
                var mousePos = evt.localMousePosition;
                // mousePos.x += 22f;
                // mousePos.y -= 22f;
                var worldPos = ConvertMapToWorldPosition(mousePos);

                // Debug.Log($"Mouse Pos: {mousePos}");
                // Debug.Log($"World Position: {worldPos}");
                // Debug.Log($"Map Position: {mapPos}");

                MapCursor.transform.position = worldPos;
                SetElementPositionOnMap(worldPos, _cursorPoint);
                // _cursorPoint.style.left = mousePos.x;
                // _cursorPoint.style.top = mousePos.y;
            });
            
            // _topoMap.RegisterCallback<GeometryChangedEvent>(OnMapLayoutReady);

            _distanceLabel = _doc.rootVisualElement.Q<Label>("distance");
            _angleLabel = _doc.rootVisualElement.Q<Label>("angle");

            _playerIcon = _doc.rootVisualElement.Q<VisualElement>("playerIcon");
            _shellIcon = _doc.rootVisualElement.Q<VisualElement>("shellIcon");
            _cursorPoint = _doc.rootVisualElement.Q<VisualElement>("cursorPoint");
            _extractPoint = _doc.rootVisualElement.Q<VisualElement>("extractPoint");
            _targetPointOriginal = _doc.rootVisualElement.Q<VisualElement>("targetPoint");
            _shellPath = _doc.rootVisualElement.Q<VisualElement>("shellPath");

            // _playerIcon.visible = false;
            // _shellIcon.visible = false;
            // _cursorPoint.visible = false;
            // _extractPoint.visible = false;
            // _targetPointOriginal.visible = false;
            // _shellPath.visible = false;

            // Debug.Log($"Distance Label: {_distanceLabel}");
            // Debug.Log($"Angle Label: {_angleLabel}");
            // Debug.Log($"Player Icon: {_playerIcon}");
            // Debug.Log($"Cursor Point: {_cursorPoint}");
            // Debug.Log($"Extract Point: {_extractPoint}");
            // Debug.Log($"Target Point: {_targetPoint}");
            // Debug.Log($"Shell Path: {_shellPath}");
        }

        private void UpdatePoints() {
            _playerIcon.visible = true;
            // _shellIcon.visible = true;
            _cursorPoint.visible = true;
            _shellPath.visible = true;

            SetElementPositionOnMap(_mc.gameObject.transform.position, _playerIcon);
            _playerIcon.style.rotate = new Rotate(_mc.gameObject.transform.eulerAngles.y - 270);

            SetElementPositionOnMap(_shell.transform.position, _shellIcon);
        }

        public void ShowShellIcon() {
            _shellIcon.visible = true;
        }

        public void HideShellIcon() {
            _shellIcon.visible = false;
        }

        public void UpdateZonePoints(Dictionary<int, Zone> zoneDict) {
            foreach (var kvp in zoneDict) {
                if (!_targetPointsDict.TryGetValue(kvp.Key, out var target) || target == null) {
                    

                    var copy = new VisualElement {
                        name = _targetPointOriginal.name + "_" + kvp.Key
                    };
                    copy.visible = true;
                    
                    _topoMap.RegisterCallback<GeometryChangedEvent>((GeometryChangedEvent evt) => {
                        float diameterPixels = kvp.Value.goalRadius * 2f * CalculatePixelUnitConversionFactor();
                        
                        copy.style.width = diameterPixels;
                        copy.style.height = diameterPixels;
                    });
                    
                    _targetPointOriginal.RegisterCallback<GeometryChangedEvent>((GeometryChangedEvent evt) => {
                        // copy.style.backgroundColor = _targetPointOriginal.resolvedStyle.backgroundColor;
                        copy.style.backgroundImage = _targetPointOriginal.resolvedStyle.backgroundImage;
                        
                        foreach (var className in _targetPointOriginal.GetClasses())
                            copy.AddToClassList(className);
                    });

                    
                    _targetPointOriginal.parent.Add(copy);

                    _targetPointsDict[kvp.Key] = copy;
                }

                var targetCopy = _targetPointsDict[kvp.Key];
                SetElementPositionOnMap(kvp.Value.transform.position, targetCopy);
            }
        }

        public void UpdateZonePoints(Dictionary<int, Zone> zoneDict, Zone extractZone) {
            UpdateZonePoints(zoneDict);
            _extractPoint.visible = extractZone.isOpen;

            SetElementPositionOnMap(extractZone.transform.position, _extractPoint);
        }

        private float CalculatePixelUnitConversionFactor() {
            float mapWidth = _topoMap.resolvedStyle.width;
            float mapHeight = _topoMap.resolvedStyle.height;
            
            float terrainWidth = Terrain.terrainData.size.x;
            float terrainHeight = Terrain.terrainData.size.z;

            float pixelsPerUnitX = mapWidth / terrainWidth;
            float pixelsPerUnitY = mapHeight / terrainHeight; 
            Debug.Log(pixelsPerUnitX+ "," + pixelsPerUnitY);
            return Mathf.Min(pixelsPerUnitX, pixelsPerUnitY);
        }

        private Vector3 ConvertMapToWorldPosition(Vector2 mapPos) {
            var mapWidth = _topoMap.resolvedStyle.width;
            var mapHeight = _topoMap.resolvedStyle.height;

            var terrainWidth = Terrain.terrainData.size.x;
            var terrainHeight = Terrain.terrainData.size.z;
            
            float pixelsPerUnitX = mapWidth / terrainWidth;
            float pixelsPerUnitY = mapHeight / terrainHeight;
            Debug.Log(pixelsPerUnitX+ "," + pixelsPerUnitY);

            // Normalize mapPos from [0, mapWidth] and [0, mapHeight] to [0, 1]
            var normalizedX = mapPos.x / mapWidth;
            var normalizedY = 1f - mapPos.y / mapHeight; // Invert Y because UI toolkit Y is down, world Z is up

            // Convert normalized coords to world coords centered at (0,0)
            var worldX = (normalizedX - 0.5f) * terrainWidth; // -22f
            var worldZ = (normalizedY - 0.5f) * terrainHeight; // +22f

            return new Vector3(worldX, 0f, worldZ);
        }

        private Vector2 ConvertWorldToMapPosition(Vector3 worldPos) {
            var mapWidth = _topoMap.resolvedStyle.width;
            var mapHeight = _topoMap.resolvedStyle.height;

            var terrainWidth = Terrain.terrainData.size.x;
            var terrainHeight = Terrain.terrainData.size.z;

            // Normalize world position from center
            var normalizedX = worldPos.x / terrainWidth + 0.5f;
            var normalizedY = worldPos.z / terrainHeight + 0.5f;

            // Invert normalizedY because UI Y is downward
            normalizedY = 1f - normalizedY;

            // Convert normalized coords to map coords
            var mapPosX = normalizedX * mapWidth;
            var mapPosY = normalizedY * mapHeight;

            return new Vector2(mapPosX, mapPosY);
        }

        private void SetElementPositionOnMap(Vector3 worldPos, VisualElement element) {
            // Convert world position to map position (UI coordinates)
            var mapPos = ConvertWorldToMapPosition(worldPos);

            // Debug.Log($"SetElementPositionOnMap: WorldPos: {worldPos} -> MapPos: {mapPos}");

            // Set the element's position in the UI using the calculated map coordinates
            element.style.left = mapPos.x - element.resolvedStyle.width / 2;
            element.style.top = mapPos.y - element.resolvedStyle.height / 2;
        }


        private void InitLabels() {
            _firingAngle = _doc.rootVisualElement.Q("firingAngle") as Label;
            _mortarRotation = _doc.rootVisualElement.Q("mortarRotation") as Label;
            _shellHeight = _doc.rootVisualElement.Q("shellHeight") as Label;
            _shellDistance = _doc.rootVisualElement.Q("shellDistance") as Label;

            // Debug.Log($"fa text: {_firingAngle}");

            _fAPrefix = _firingAngle.text;
            _mRPrefix = _mortarRotation.text;
            _sHPrefix = _shellHeight.text;
            _sDPrefix = _shellDistance.text;

            // Debug.Log($"fa text: {_fAPrefix}");
            
            _objList = _doc.rootVisualElement.Q<Label>("objList");
        }

        private string Round(float num) {
            var rounded = Mathf.Round(num * 10f) / 10f;
            return rounded.ToString("F1");
        }

        private void UpdateDataText() {
            _firingAngle.text = _fAPrefix + $" {Round(_mc.firingAngle)}°";
            _mortarRotation.text = _mRPrefix + $" {Round(_mc.rotationAngle)}°";

            var dist = Vector3.Distance(_mc.gameObject.transform.position, shellTf.position);
            if (dist < 5f) dist = 0;
            _shellDistance.text = _sDPrefix + $" {Round(dist)} METERS";

            // _shellHeight.text = _sHPrefix + $" {Round(_shellRb.velocity.magnitude)} M/S";

            var height = _shell.transform.position.y;
            if (height < 5f) height = 0;
            _shellHeight.text = _sHPrefix + $" {Round(height)} METERS";

            var cursorDist = Vector3.Distance(_mc.gameObject.transform.position, MapCursor.transform.position);
            _distanceLabel.text = $"{Round(cursorDist)} m";

            var delta = MapCursor.transform.position - _mc.gameObject.transform.position;
            var flatDelta = new Vector2(delta.x, delta.z); // Ignore Y

            var angle = Mathf.Atan2(flatDelta.y, flatDelta.x) * Mathf.Rad2Deg * -1;

            if (angle < 0f) angle += 360f;
            else if (angle > 360f) angle -= 360f;

            _angleLabel.text = $"{Round(angle)}°";
        }

        private void UpdateObjList() {
            string output = "";

            foreach (var kvp in _gm._zones) {
                string status = kvp.Value.isCompleted ? "COMPLETED" : "INCOMPLETE";
                output += $"TARGET {kvp.Key}: {status}\n";
            }

            _objList.text = output;
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