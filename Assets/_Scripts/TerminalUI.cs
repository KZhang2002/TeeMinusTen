using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace _Scripts {
    public class TerminalUI : MonoBehaviour {
        public float updateInterval = 0.2f;
        public Terrain Terrain;
        public GameObject MapCursor;

        public Texture2D TopoMapBG;

        private readonly Dictionary<int, VisualElement> _targetPointsDict = new();
        private Label _angleLabel;
        private VisualElement _cursorPoint;

        private Label _distanceLabel;

        private UIDocument _doc;
        private VisualElement _extractPoint;
        private string _fAPrefix;
        private Label _firingAngle;

        private GameManager _gm;
        private float _mapHeight;

        private float _mapWidth;
        private MortarController _mc;
        private Label _mortarRotation;
        private string _mRPrefix;

        private Label _objList;
        private float _pixelsPerUnit;
        private VisualElement _playerIcon;
        private string _sDPrefix;
        private Shell _shell;
        private Label _shellDistance;
        private Label _shellHeight;
        private VisualElement _shellIcon;
        private VisualElement _shellPath;
        private Rigidbody _shellRb;
        private string _sHPrefix;
        private VisualElement _targetPoint;
        private Label _targetLabel;
        private float _terrainHeight;
        private float _terrainWidth;

        private VisualElement _bg;

        private float _timer;

        // Map stuff
        private VisualElement _topoMap;
        public static TerminalUI instance { get; private set; }
        private Transform shellTf => _shell.transform;

        public bool isDragging { get; set; }

        private float mapWidth {
            get {
                if (_mapWidth != 0 && !float.IsNaN(_mapWidth)) return _mapWidth;
                _mapWidth = _topoMap.resolvedStyle.width;
                return _mapWidth;
            }
        }

        private float mapHeight {
            get {
                if (_mapHeight != 0 && !float.IsNaN(_mapHeight)) return _mapHeight;
                _mapHeight = _topoMap.resolvedStyle.height;
                return _mapHeight;
            }
        }

        private float terrainWidth {
            get {
                if (_terrainWidth != 0 && !float.IsNaN(_terrainWidth)) return _terrainWidth;
                _terrainWidth = Terrain.terrainData.size.x;
                return _terrainWidth;
            }
        }

        private float terrainHeight {
            get {
                if (_terrainHeight != 0 && !float.IsNaN(_terrainHeight)) return _terrainHeight;
                _terrainHeight = Terrain.terrainData.size.z;
                return _terrainHeight;
            }
        }

        private float pixelsPerUnit {
            get {
                if (_pixelsPerUnit != 0 && !float.IsNaN(_pixelsPerUnit)) return _pixelsPerUnit;

                var pixelsPerUnitX = mapWidth / terrainWidth;
                var pixelsPerUnitY = mapHeight / terrainHeight;

                _pixelsPerUnit = Mathf.Min(pixelsPerUnitX, pixelsPerUnitY);
                return _pixelsPerUnit;
            }
        }

        private void Awake() {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;

            _doc = GetComponent<UIDocument>();

            InitLabels();
            if (Terrain) InitMap();

            _timer = updateInterval;
        }

        private void Start() {
            _gm = GameManager.instance;
            if (!_gm || !_gm.mortar) return;

            _mc = _gm.mortar;
            _shell = _mc.currentShell;
            _shellRb = _shell.GetComponent<Rigidbody>();
        }

        private void Update() {
            _timer += Time.deltaTime;
            if (_timer >= updateInterval) {
                if (_mc) {
                    if (!_shell) _shell = _mc.currentShell;
                    if (_shell && !_shellRb) _shellRb = _shell.GetComponent<Rigidbody>();
                    if (_shellRb) UpdateDataText();
                }

                // todo, move out of update
                if (Terrain) {
                    UpdatePkgStatusList();
                    UpdateEntityIcons();
                    UpdateZonePoints(_gm._zones, _gm._extractZone);
                }

                _timer = 0f;
            }

            UpdateEntityIcons();
            UpdateZonePoints(_gm._zones, _gm._extractZone);
            UpdatePkgStatusList();
        }

        private void InitMap() {
            _topoMap = _doc.rootVisualElement.Q("TopoMap");

            _topoMap.RegisterCallback<MouseDownEvent>(evt => {
                if (evt.button == 0) // Left-click
                {
                    isDragging = true;
                    evt.StopPropagation();

                    // Get the point clicked on the map
                    var mousePos = evt.localMousePosition;
                    mousePos.y = _topoMap.resolvedStyle.height - mousePos.y;
                    mousePos.x = _topoMap.resolvedStyle.width - mousePos.x;

                    // Convert to world position
                    var worldPos = ConvertMapToWorldPosition(mousePos);
                    MapCursor.transform.position = worldPos;
                    SetElementPositionWorldToTopoMap(worldPos, _cursorPoint);
                }
            });

            _topoMap.RegisterCallback<MouseMoveEvent>(evt => {
                if (isDragging) {
                    // Get the point clicked on the map
                    var mousePos = evt.localMousePosition;
                    mousePos.y = _topoMap.resolvedStyle.height - mousePos.y;
                    mousePos.x = _topoMap.resolvedStyle.height - mousePos.x;

                    // Convert to world position
                    var worldPos = ConvertMapToWorldPosition(mousePos);

                    // Update cursor and map point positions
                    MapCursor.transform.position = worldPos;
                    SetElementPositionWorldToTopoMap(worldPos, _cursorPoint);
                }
            });

            _topoMap.RegisterCallback<MouseUpEvent>(evt => {
                if (evt.button == 0) // Left-click
                {
                    isDragging = false;
                    evt.StopPropagation();
                }
            });


            // _topoMap.RegisterCallback<GeometryChangedEvent>(OnMapLayoutReady);

            _distanceLabel = _doc.rootVisualElement.Q<Label>("distance");
            _angleLabel = _doc.rootVisualElement.Q<Label>("angle");

            _playerIcon = _doc.rootVisualElement.Q<VisualElement>("playerIcon");
            _shellIcon = _doc.rootVisualElement.Q<VisualElement>("shellIcon");
            _cursorPoint = _doc.rootVisualElement.Q<VisualElement>("cursorPoint");
            _extractPoint = _doc.rootVisualElement.Q<VisualElement>("extractPoint");
            _targetPoint = _doc.rootVisualElement.Q<VisualElement>("targetPoint");
            _targetLabel = _targetPoint.Q<Label>("targetLabel");
            _shellPath = _doc.rootVisualElement.Q<VisualElement>("shellPath");
            
            _extractPoint.visible = false;
            _targetPoint.visible = false;
        }

        //todo cursor stuff
        private void InitMouseInteraction() {
            _bg = _doc.rootVisualElement.Q("BG");
            
            _bg.RegisterCallback<MouseMoveEvent>(evt => {
                
            });
        }

        public void reloadTopoMapImage(string fileName) {
            if (File.Exists(fileName))
            {
                byte[] fileData = File.ReadAllBytes(fileName);
                TopoMapBG = new Texture2D(2, 2);
                TopoMapBG.LoadImage(fileData);
                _topoMap.style.backgroundImage = TopoMapBG;
            } 
            else
            {
                Debug.LogError($"Image not found at path: {fileName}");
            }
            
        }

        private void UpdateEntityIcons() {
            _playerIcon.visible = true;
            // _shellIcon.visible = true;
            _cursorPoint.visible = true;
            _shellPath.visible = true;

            SetElementPositionWorldToTopoMap(_mc.gameObject.transform.position, _playerIcon);
            // _playerIcon.style.rotate = new Rotate(_mc.gameObject.transform.eulerAngles.y + 45f);
            _playerIcon.style.rotate = new Rotate((_mc.rotationAngle - 90 + 360) % 360);

            SetElementPositionWorldToTopoMap(_shell.transform.position, _shellIcon);
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
                        name = _targetPoint.name + "_" + kvp.Key
                    };
                    copy.visible = true;

                    // Set position on the map
                    SetElementPositionWorldToTopoMap(kvp.Value.transform.position, copy);

                    // Change icon size to match zone size
                    _topoMap.RegisterCallback((GeometryChangedEvent _) => {
                        var diameterPixels = kvp.Value.goalRadius * 2f * pixelsPerUnit;
                        copy.style.width = diameterPixels;
                        copy.style.height = diameterPixels;
                    });

                    // Apply zone icon through class addition
                    _targetPoint.RegisterCallback((GeometryChangedEvent _) => {
                        copy.style.backgroundImage = _targetPoint.resolvedStyle.backgroundImage;

                        foreach (var className in _targetPoint.GetClasses())
                            copy.AddToClassList(className);
                    });

                    // ** Create a copy of the label **
                    var labelCopy = new Label {
                        name = _targetLabel.name + "_" + kvp.Key,
                        text = IntToLetter(kvp.Key)
                    };

                    // Copy the styles and classes
                    foreach (var className in _targetLabel.GetClasses())
                        labelCopy.AddToClassList(className);

                    // Add the label copy to the visual element copy
                    copy.Add(labelCopy);

                    // Add to parent and dictionary
                    _targetPoint.parent.Add(copy);
                    _targetPointsDict[kvp.Key] = copy;
                }

                
                var targetCopy = _targetPointsDict[kvp.Key];

                if (targetCopy.ClassListContains("zoneCompleted")) {
                    return;
                }
                
                if (kvp.Value.isCompleted) {
                    targetCopy.AddToClassList("zoneCompleted");
                    Debug.Log($"PKG {IntToLetter(kvp.Key)} is completed.");
                }
                
                var zonePos = kvp.Value.transform.position;
                SetElementPositionWorldToTopoMap(zonePos, targetCopy);
            }
        }

        public void UpdateZonePoints(Dictionary<int, Zone> zoneDict, Zone extractZone) {
            UpdateZonePoints(zoneDict);
            if (_gm._extractZone) {
                _extractPoint.visible = extractZone.isOpen;
                SetElementPositionWorldToTopoMap(extractZone.transform.position, _extractPoint);
            }
        }

        private Vector3 ConvertMapToWorldPosition(Vector2 mapPos) {
            // Normalize mapPos from [0, mapWidth] and [0, mapHeight] to [0, 1]
            var normalizedX = mapPos.x / mapWidth;
            var normalizedY = mapPos.y / mapHeight; // Invert Y because UI toolkit Y is down, world Z is up

            // Convert normalized coords to world coords centered at (0,0)
            var worldX = (normalizedX - 0.5f) * terrainWidth;
            var worldZ = (normalizedY - 0.5f) * terrainHeight;

            return new Vector3(worldX, 0f, worldZ);
        }

        public Vector2 ConvertWorldToMapPosition(Vector3 worldPos) {
            if (terrainWidth == 0 || terrainHeight == 0) {
                Debug.LogError("Terrain size is zero. Cannot convert world to map position.");
                return Vector2.zero;
            }

            // Normalize world position from center
            var normalizedX = worldPos.x / terrainWidth + 0.5f;
            var normalizedY = worldPos.z / terrainHeight + 0.5f;
            normalizedX = 1f - normalizedX;

            // Convert normalized coords to map coords
            var mapPosX = normalizedX * mapWidth;
            var mapPosY = normalizedY * mapHeight;

            return new Vector2(mapPosX, mapPosY);
        }

        private Vector2 SetElementPositionWorldToTopoMap(Vector3 worldPos, VisualElement element) {
            // Convert world position to map position (UI coordinates)
            var mapPos = ConvertWorldToMapPosition(worldPos);

            var left = mapPos.x - element.resolvedStyle.width / 2;
            var top = mapPos.y - element.resolvedStyle.height / 2;
            // Set the element's position in the UI using the calculated map coordinates
            element.style.left = left;
            element.style.top = top;

            return new Vector2(left, top);
        }


        private void InitLabels() {
            _firingAngle = _doc.rootVisualElement.Q("firingAngle") as Label;
            _mortarRotation = _doc.rootVisualElement.Q("mortarRotation") as Label;
            _shellHeight = _doc.rootVisualElement.Q("shellHeight") as Label;
            _shellDistance = _doc.rootVisualElement.Q("shellDistance") as Label;

            _fAPrefix = _firingAngle.text;
            _mRPrefix = _mortarRotation.text;
            _sHPrefix = _shellHeight.text;
            _sDPrefix = _shellDistance.text;

            _objList = _doc.rootVisualElement.Q<Label>("objList");
        }

        private string Round(float num) {
            var rounded = Mathf.Round(num * 10f) / 10f;
            return rounded.ToString("F1");
        }

        private string IntToLetter(int num) {
            return ((char)('A' + num)).ToString();
        }

        private void UpdateDataText() {
            _firingAngle.text = _fAPrefix + $" {Round(_mc.firingAngle)}°";
            _mortarRotation.text = _mRPrefix + $" {Round(_mc.rotationAngle)}°";

            var dist = Vector2.Distance(new Vector2(_mc.transform.position.x, _mc.transform.position.z),
                new Vector2(shellTf.position.x, shellTf.position.z));
            if (dist < 5f) dist = 0;
            _shellDistance.text = _sDPrefix + $" {Round(dist)} M";

            // _shellHeight.text = _sHPrefix + $" {Round(_shellRb.velocity.magnitude)} M/S";

            var height = _shell.transform.position.y;
            if (height < 5f) height = 0;
            _shellHeight.text = _sHPrefix + $" {Round(height)} M";


            var cursorDist = Vector3.Distance(_mc.gameObject.transform.position, MapCursor.transform.position);
            _distanceLabel.text = $"{Round(cursorDist)} m from target";

            var delta = MapCursor.transform.position - _mc.gameObject.transform.position;
            var flatDelta = new Vector2(delta.x, delta.z); // Ignore Y

            var angle = Mathf.Atan2(flatDelta.y, flatDelta.x) * Mathf.Rad2Deg * -1;

            if (angle < 0f) angle += 360f;
            else if (angle > 360f) angle -= 360f;

            _angleLabel.text = $"{Round(angle)}°  mortar angle required";
        }

        private void UpdatePkgStatusList() {
            var output = "";

            foreach (var kvp in _gm._zones) {
                var status = kvp.Value.isCompleted ? "DELIVERED" : "IN TRANSIT";
                output += $"PKG {IntToLetter(kvp.Key)}: {status}\n";
            }

            _objList.text = output;
        }
    }
}