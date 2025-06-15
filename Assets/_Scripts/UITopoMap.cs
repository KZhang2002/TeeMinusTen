using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts {
    public class UITopoMap : MonoBehaviour{
        
        // References
        private UIDocument _doc;
        private GameManager _gm;
        private MortarController _mc;
        private Shell _shell;
        private Rigidbody _shellRb;
        private Transform shellTf => shell.transform;
        
        public float updateInterval = 0.2f;
        private float _timer;
        
        #region Map Stuff
        
            public Terrain Terrain;
            private Texture2D TopoMapBG;
                
            private VisualElement _cursorPoint;
            // Visual indicator of where map calculator pointer is pointing at in world space.
            public GameObject MapCursor;
                
            private Label _angleLabel;
            private Label _distanceLabel;
                
            // Map Icons
            private VisualElement _playerIcon;
            private VisualElement _shellIcon;
            private VisualElement _shellPath;
            private VisualElement _targetPoint;
            private Label _targetLabel;
                
            private VisualElement _extractPoint;
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
        
        // Publicly exposed functions
        public void UpdateMap() {
            if (Terrain) {
                UpdateEntityIcons();
                UpdateZonePoints(_gm._zones, _gm._extractZone);
                UpdateCalculatorText();
                // UpdatePkgStatusList();
            }
        }
        
        public void ShowShellIcon() {
            _shellIcon.visible = true;
        }

        public void HideShellIcon() {
            _shellIcon.visible = false;
        }

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
                if (shell && shellRb) Debug.Log("Shell: " + shell + ", RB: " + shellRb); 
                else Debug.Log("Shell: " + shell + ", RB: " + shellRb);
                
                // if (_mc) {
                //     if (!_shell) _shell = _mc.currentShell;
                //     if (_shell && !_shellRb) _shellRb = _shell.GetComponent<Rigidbody>();
                // }
            }
            
            private void InitMap() {
                UIHelper.AssignVE(ref _topoMap, "TopoMap", _doc);

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

                UIHelper.AssignLabel(ref _distanceLabel, "distance", _doc);
                UIHelper.AssignLabel(ref _angleLabel, "angle", _doc);

                UIHelper.AssignVE(ref _playerIcon, "playerIcon", _doc);
                UIHelper.AssignVE(ref _shellIcon, "shellIcon", _doc);
                UIHelper.AssignVE(ref _cursorPoint, "cursorPoint", _doc);
                UIHelper.AssignVE(ref _extractPoint, "extractPoint", _doc);
                UIHelper.AssignVE(ref _targetPoint, "targetPoint", _doc);
                UIHelper.AssignLabel(ref _targetLabel, "targetLabel", _doc);
                UIHelper.AssignVE(ref _shellPath, "shellPath", _doc);
                
                _extractPoint.visible = false;
                _targetPoint.visible = false;
            }
            
            public void loadTopoMapTexture(Texture2D texture) {
                if (texture == null) {
                    Debug.LogError("Provided texture is null. Cannot assign to topo map.");
                    return;
                }

                TopoMapBG = texture;
                _topoMap.style.backgroundImage = TopoMapBG;
            }

        #endregion

        #region Update Functions
            
            private void UpdateEntityIcons() {
                _playerIcon.visible = true;
                // _shellIcon.visible = true;
                _cursorPoint.visible = true;
                _shellPath.visible = true;

                SetElementPositionWorldToTopoMap(_mc.gameObject.transform.position, _playerIcon);
                // _playerIcon.style.rotate = new Rotate(_mc.gameObject.transform.eulerAngles.y + 45f);
                _playerIcon.style.rotate = new Rotate((_mc.rotationAngle - 90 + 360) % 360);

                if (shell) SetElementPositionWorldToTopoMap(shell.transform.position, _shellIcon);
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

                    
                    var targetCopy = _targetPointsDict[kvp.Key];

                    if (targetCopy.ClassListContains("zoneCompleted")) {
                        return;
                    }
                    
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
            
            private void UpdateCalculatorText() {

                var dist = Vector2.Distance(new Vector2(_mc.transform.position.x, _mc.transform.position.z),
                    new Vector2(shellTf.position.x, shellTf.position.z));
                if (dist < 5f) dist = 0;
            
                var cursorDist = Vector3.Distance(_mc.gameObject.transform.position, MapCursor.transform.position);
                _distanceLabel.text = $"{UIHelper.RoundFloatToStr(cursorDist)} m from target";

                var delta = MapCursor.transform.position - _mc.gameObject.transform.position;
                var flatDelta = new Vector2(delta.x, delta.z); // Ignore Y

                var angle = Mathf.Atan2(flatDelta.y, flatDelta.x) * Mathf.Rad2Deg * -1;

                if (angle < 0f) angle += 360f;
                else if (angle > 360f) angle -= 360f;

                _angleLabel.text = $"{UIHelper.RoundFloatToStr(angle)}° heading advised";
            }
            
            // private void UpdatePkgStatusList() {
            //     var output = "";
            //
            //     foreach (var kvp in _gm._zones) {
            //         var status = kvp.Value.isCompleted ? "DELIVERED" : "IN TRANSIT";
            //         output += $"PKG {UIHelper.IntToLetter(kvp.Key)}: {status}\n";
            //     }
            //
            //     _pkgStatusList.text = output;
            // }

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

            // Constants - calculated at runtime!
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