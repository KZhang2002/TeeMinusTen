using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace _Scripts {
    public class UIManager : MonoBehaviour {
        public float updateInterval = 0.2f;
        private float _timer;
        
        // References
        private UIDocument _doc;
        private GameManager _gm;
        private MortarController _mc;
        
        private Shell _shell;
        private Rigidbody _shellRb;
        
        public static UIManager instance { get; private set; }
        private Transform shellTf => _shell.transform;
        private UITopoMap _map;

        #region Terminal Data

            // Operational Data
            private Label _firingAngle;
            private string _fAPrefix;
            
            private Label _mortarRotation; // aka mortar heading
            private string _mRPrefix;
            
            private Label _shellDistance;
            private string _sDPrefix;
            private Label _shellHeight;
            private string _sHPrefix;

            private Label _rangeData;
            
            // Package Status Data
            private Label _pkgStatusList;
            
        #endregion

        #region Start Up
            private void Awake() {
                if (instance != null && instance != this)
                    Destroy(this);
                else
                    instance = this;

                _doc = GetComponent<UIDocument>();
                _map = GetComponent<UITopoMap>();

                InitLabels();

                _timer = updateInterval;
            }

            private void Start() {
                _gm = GameManager.instance;
                if (!_gm || !_gm.mortar) return;

                _mc = _gm.mortar;
                
                // UpdateRangeData();
                // _map.UpdateStatic();
            }
            
            public void loadTopoMapTexture(Texture2D texture) {
                // if (texture == null) {
                //     Debug.LogError("Provided texture is null. Cannot assign to topo map.");
                //     return;
                // }
                //
                // TopoMapBG = texture;
                // _topoMap.style.backgroundImage = TopoMapBG;
                _map.loadTopoMapTexture(texture);
            }
            
            private void InitLabels() {
                UIHelper.AssignLabel(ref _firingAngle, "firingAngle", _doc);
                UIHelper.AssignLabel(ref _mortarRotation, "mortarRotation", _doc);
                UIHelper.AssignLabel(ref _shellHeight, "shellHeight", _doc);
                UIHelper.AssignLabel(ref _shellDistance, "shellDistance", _doc);
                UIHelper.AssignLabel(ref _rangeData, "RangeData", _doc);
                
                _fAPrefix = _firingAngle.text;
                _mRPrefix = _mortarRotation.text;
                _sHPrefix = _shellHeight.text;
                _sDPrefix = _shellDistance.text;

                _pkgStatusList = _doc.rootVisualElement.Q<Label>("objList");
            }
            
        #endregion
        
        #region Event Responses
            private void OnEnable() {
                ShellEvent.OnShellLanded += HandleShellLanded;
                // ShellEvent.OnShellFired += HandleShellFired;
                ShellEvent.OnShellLoaded += HandleShellLoaded;
                ShellEvent.OnShellRegistered += HandleShellRegistered;
            }

            private void OnDisable() {
                ShellEvent.OnShellLanded -= HandleShellLanded;
                // ShellEvent.OnShellFired -= HandleShellFired;
                ShellEvent.OnShellLoaded -= HandleShellLoaded;
                ShellEvent.OnShellRegistered -= HandleShellRegistered;
            }
            
            private void HandleShellLanded(int zoneID) {
                // add switch to static rendering here
                // UpdatePkgStatusList();
                // _map.UpdateStatic();
            }

            private void HandleShellLoaded() {
                // UpdateRangeData();
            }
            
            private void HandleShellRegistered(Shell shell) {
                _shell = shell;
                _shellRb = _shell.GetComponent<Rigidbody>();
                
                UpdateRangeData();
            }
            
        #endregion

        private void Update() {
            _timer += Time.deltaTime;
            if (_timer >= updateInterval) {
                UpdateDataText();
                _timer = 0f;
            }
            
            _map.UpdateMapAll();
            UpdatePkgStatusList();
        }
        
        public void UpdateZonePoints(Dictionary<int, Zone> zoneDict, Zone extractZone) {
            _map.UpdateZonePoints(zoneDict, extractZone);
            // UpdateZonePoints(zoneDict);
            // if (_gm._extractZone) {
            //     _extractPoint.visible = extractZone.isOpen;
            //     SetElementPositionWorldToTopoMap(extractZone.transform.position, _extractPoint);
            // }
        }
        
        private void UpdateDataText() {
            _firingAngle.text = _fAPrefix + $" {UIHelper.RoundFloatToStr(_mc.firingAngle)}°/{UIHelper.RoundFloatToStr(_mc.actualFiringAngle)}°";
            _mortarRotation.text = _mRPrefix + $" {UIHelper.RoundFloatToStr(_mc.rotationAngle % 360)}°/{UIHelper.RoundFloatToStr(_mc.actualRotationAngle % 360)}°";

            var dist = Vector2.Distance(new Vector2(_mc.transform.position.x, _mc.transform.position.z),
                new Vector2(shellTf.position.x, shellTf.position.z));
            if (dist < 5f) dist = 0;
            _shellDistance.text = _sDPrefix + $" {UIHelper.RoundFloatToStr(dist)} M";

            // _shellHeight.text = _sHPrefix + $" {UIHelper.RoundFloatToStr(_shellRb.velocity.magnitude)} M/S";

            var height = _shell.transform.position.y;
            if (height < 5f) height = 0;
            _shellHeight.text = _sHPrefix + $" {UIHelper.RoundFloatToStr(height)} M";
        }
        
        private void UpdatePkgStatusList() {
            var output = "";

            foreach (var kvp in _gm._zones) {
                var status = kvp.Value.isCompleted ? "DELIVERED" : "IN TRANSIT";
                output += $"PKG {UIHelper.IntToLetter(kvp.Key)}: {status}\n";
            }

            _pkgStatusList.text = output;
        }
        
        private void UpdateRangeData() {
            impulseType type = _shell.impulseType;
            _rangeData.text = ShellRangeData.RangeTable[type];
            // Debug.Log("input range data as: " + _rangeData.text);
        }
    }
}