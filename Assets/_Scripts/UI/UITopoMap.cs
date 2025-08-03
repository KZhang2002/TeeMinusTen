using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace _Scripts {
    public class UITopoMap : MonoBehaviour {
        
        // External References
        private UIDocument _doc;
        private GameManager _gm;
        private MortarController _mc;
        private Shell _shell;
        private Rigidbody _shellRb;
        private Transform shellTf => shell.transform;
        
        #region Map Stuff
        
            public Terrain Terrain;
            private Texture2D TopoMapBG;
            
            // Visual indicator of where map calculator pointer is pointing at in world space.
            [FormerlySerializedAs("MapCursor")] public GameObject MapMarker;
            private VisualElement _cursorPoint;
                
            private Label _angleLabel;
            private Label _distanceLabel;
                
            // Map Icons
            private VisualElement _playerIcon;
            private VisualElement _shellIcon;
            private VisualElement _shellPath;
            private VisualElement _targetPoint;
            private Label _targetLabel;
                
            private VisualElement _extractPoint;
            private VisualElement _extractIcon;
            private readonly Dictionary<int, VisualElement> _targetPointsDict = new();
                
            private VisualElement _topoMap;
                
            // Cursor Stuff
            public bool isDragging { get; set; }

        #endregion

        #region External Reference Getters

            // General behavior: Calculate when first called then cache value
            private Shell shell {
                get {
                    if (_shell) return _shell;
                    _shell = _mc.currentShell;
                    return _shell;
                }
            }
            
            private Rigidbody shellRb {
                get {
                    if (_shellRb) return _shellRb;
                    _shellRb = shell.GetComponent<Rigidbody>();
                    return _shellRb;
                }
            }

        #endregion
        
        #region Publicly Exposed Functions
        
            public void UpdateMapAll() {
                if (Terrain) {
                    UpdateEntityIcons();
                    UpdateZonePoints(_gm._zones, _gm._extractZone);
                    UpdateCalculatorText();
                    // UpdatePkgStatusList();
                }
            }

            public void UpdateStatic() {
                UpdateZonePoints(_gm._zones, _gm._extractZone);
            }

            public void UpdateDynamic() {
                UpdateEntityIcons();
            }
            
            public void ShowShellIcon() {
                _shellIcon.visible = true;
            }

            public void HideShellIcon() {
                _shellIcon.visible = false;
            }
            
        #endregion

        #region Start Up Functions

            private void Awake() {
                _doc = GetComponent<UIDocument>();
                if (Terrain) InitMap();
            }

            private void Start() {
                _gm = GameManager.instance;
                if (!_gm || !_gm.mortar) return;

                _mc = _gm.mortar;
                
                // NOT DEBUG - ATTEMPTS TO PREFETCH SHELL AND SHELLRB
                // if (shell && shellRb) Debug.Log("Shell: " + shell + ", RB: " + shellRb); 
                // else Debug.Log("Shell: " + shell + ", RB: " + shellRb);
                
                _playerIcon.visible = true;
                _cursorPoint.visible = true;
                _shellPath.visible = true;
                
                UpdateMapAll();
            }
            
            private void InitMap() {
                UIHelper.AssignVE(ref _topoMap, "TopoMap", _doc);
                
                _topoMap.RegisterCallback<MouseDownEvent>(evt => {
                    // Left-click
                    if (evt.button == 0) {
                        isDragging = true;
                        evt.StopPropagation();
                        MoveMapCursor(evt.localMousePosition);
                        UpdateCalculatorText();
                    }
                });

                _topoMap.RegisterCallback<MouseMoveEvent>(evt => {
                    if (isDragging) {
                        MoveMapCursor(evt.localMousePosition);
                        UpdateCalculatorText();
                    }
                });

                _topoMap.RegisterCallback<MouseUpEvent>(evt => {
                    // Left-click
                    if (evt.button == 0) {
                        isDragging = false;
                        evt.StopPropagation();
                    }
                });
                
                _topoMap.RegisterCallback((GeometryChangedEvent _) => {
                    UpdateZonePoints(_gm._zones, _gm._extractZone);
                });

                // Calculator Text
                UIHelper.AssignLabel(ref _distanceLabel, "distance", _doc);
                UIHelper.AssignLabel(ref _angleLabel, "angle", _doc);

                // Map Icons
                UIHelper.AssignVE(ref _cursorPoint, "cursorPoint", _doc);
                UIHelper.AssignVE(ref _playerIcon, "playerIcon", _doc);
                UIHelper.AssignVE(ref _shellIcon, "shellIcon", _doc);
                UIHelper.AssignVE(ref _extractPoint, "extractPoint", _doc);
                UIHelper.AssignVE(ref _extractIcon, "extractIcon", _doc);
                UIHelper.AssignVE(ref _targetPoint, "targetPoint", _doc);
                UIHelper.AssignLabel(ref _targetLabel, "targetLabel", _doc);
                UIHelper.AssignVE(ref _shellPath, "shellPath", _doc);
                
                _extractPoint.visible = false;
                _targetPoint.visible = false;
                
                // Change extract icon size to match zone size
                _topoMap.RegisterCallback((GeometryChangedEvent _) => {
                    var diameterPixels = _gm._extractZone.goalRadius * 2f * pixelsPerUnit;
                    _extractPoint.style.width = diameterPixels;
                    _extractPoint.style.height = diameterPixels;
                });
            }

            private void MoveMapCursor(Vector2 localMousePosition) {
                // Get the point clicked on the map
                var mousePos = localMousePosition;
                mousePos.y = _topoMap.resolvedStyle.height - mousePos.y;
                mousePos.x = _topoMap.resolvedStyle.width - mousePos.x;

                // Convert to world position
                var worldPos = ConvertMapToWorldPosition(mousePos);
                MapMarker.transform.position = worldPos;
                SetElementPositionWorldToTopoMap(worldPos, _cursorPoint);
            }
            
            public void loadTopoMapTexture(Texture2D texture) {
                if (texture == null) {
                    Debug.LogError("Provided map texture is null. Cannot assign to topo map.");
                    return;
                }

                TopoMapBG = texture;
                _topoMap.style.backgroundImage = TopoMapBG;
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

        #endregion

        #region Event Responses

            private void HandleShellLanded() {
            }

            private void HandleShellFired() {
                ShowShellIcon();
            }

            private void HandleShellLoaded() {
                HideShellIcon();
            }

        #endregion

        #region Update Functions
            
            private void UpdateEntityIcons() {
                SetElementPositionWorldToTopoMap(_mc.gameObject.transform.position, _playerIcon);
                _playerIcon.style.rotate = new Rotate((_mc.rotationAngle - 90 + 360) % 360);

                if (shell) SetElementPositionWorldToTopoMap(shell.transform.position, _shellIcon);
            }
            
            public void UpdateZonePoints(Dictionary<int, Zone> zoneDict) {
                foreach (var kvp in zoneDict) {
                    if (!_targetPointsDict.TryGetValue(kvp.Key, out var target) || target == null) {
                        CreateNewZonePoint(kvp);
                    }
                    
                    var targetCopy = _targetPointsDict[kvp.Key];
                    if (targetCopy.ClassListContains("zoneCompleted")) return;
                    
                    if (kvp.Value.isCompleted) {
                        targetCopy.AddToClassList("zoneCompleted");
                        Debug.Log($"PKG {UIHelper.IntToLetter(kvp.Key)} is completed.");
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

            private void CreateNewZonePoint(KeyValuePair<int, Zone> kvp) {
                // Create a copy of visual element
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
                    
                    // Set position on the map
                    SetElementPositionWorldToTopoMap(kvp.Value.transform.position, copy);
                });

                // Apply zone icon through class addition
                copy.style.backgroundImage = _targetPoint.resolvedStyle.backgroundImage;

                foreach (var className in _targetPoint.GetClasses())
                    copy.AddToClassList(className);

                // Create a copy of the label
                var labelCopy = new Label {
                    name = _targetLabel.name + "_" + kvp.Key,
                    text = UIHelper.IntToLetter(kvp.Key)
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

            private void UpdateCalculatorText() {
                var cursorDist = Vector3.Distance(_mc.gameObject.transform.position, MapMarker.transform.position);
                _distanceLabel.text = $"{UIHelper.RoundFloatToStr(cursorDist)} m from target";

                var delta = MapMarker.transform.position - _mc.gameObject.transform.position;
                var flatDelta = new Vector2(delta.x, delta.z); // Ignore Y

                var angle = Mathf.Atan2(flatDelta.y, flatDelta.x) * Mathf.Rad2Deg * -1;

                if (angle < 0f) angle += 360f;
                else if (angle > 360f) angle -= 360f;

                _angleLabel.text = $"{UIHelper.RoundFloatToStr(angle)}° heading advised";
            }

        #endregion

        #region Position Utilities

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
            
            private Vector2 SetElementPosToMap(Vector3 elementPos, VisualElement element) {
                var left = elementPos.x;
                var top = elementPos.y;
                
                element.style.left = left;
                element.style.top = top;

                return new Vector2(left, top);
            }

        #endregion
        
        #region Cached Constants and Getters

            // Constants: calculated at runtime!
            private float _mapHeight;
            private float _mapWidth;
            private float _pixelsPerUnit;
            private float _terrainHeight;
            private float _terrainWidth;
            
            // General behavior: Calculate when first called then cache value
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

            // used for map space to world space conversions
            private float pixelsPerUnit {
                get {
                    if (_pixelsPerUnit != 0 && !float.IsNaN(_pixelsPerUnit)) return _pixelsPerUnit;

                    var pixelsPerUnitX = mapWidth / terrainWidth;
                    var pixelsPerUnitY = mapHeight / terrainHeight;

                    _pixelsPerUnit = Mathf.Min(pixelsPerUnitX, pixelsPerUnitY);
                    return _pixelsPerUnit;
                }
            }

        #endregion
    }
}