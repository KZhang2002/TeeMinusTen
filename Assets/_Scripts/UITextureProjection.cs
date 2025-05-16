using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Samples.Runtime.Rendering
{
    public class UITextureProjection : MonoBehaviour
    {
        public Camera m_TargetCamera;

        /// <summary>
        /// When using a render texture, this camera will be used to translate screencoodinates to the panel's coordinates
        /// </summary>
        /// <remarks>
        /// If none is set, it will be initialized with Camera.main
        /// </remarks>
        public Camera targetCamera
        {
            get
            {
                if (m_TargetCamera == null) {
                    m_TargetCamera = Camera.main;
                    Debug.Log("Target camera set to " + Camera.main.transform.name);
                }
                
                return m_TargetCamera;
            }
            set => m_TargetCamera = value;
        }

        public PanelSettings TargetPanel;
        
        public RenderTexture targetTexture;

        public VolumeProfile volumeProfile;
        
        private LensDistortion lensDistortion;

        private Func<Vector2, Vector2> m_DefaultRenderTextureScreenTranslation;
        
        void Awake()
        {
            if (volumeProfile == null || !volumeProfile.TryGet(out lensDistortion))
            {
                Debug.LogError("Lens Distortion not found in the assigned Volume Profile.");
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        void OnEnable()
        {
            if (TargetPanel != null)
            {
                if (m_DefaultRenderTextureScreenTranslation == null)
                {
                    m_DefaultRenderTextureScreenTranslation = (pos) => ScreenCoordinatesToRenderTexture(pos);
                }

                TargetPanel.SetScreenToPanelSpaceFunction(m_DefaultRenderTextureScreenTranslation);
            }
        }

        void OnDisable()
        {
            //we reset it back to the default behavior
            if (TargetPanel != null)
            {
                TargetPanel.SetScreenToPanelSpaceFunction(null);
            }
        }

        /// <summary>
        /// Transforms a screen position to a position relative to render texture used by a MeshRenderer.
        /// </summary>
        /// <param name="screenPosition">The position in screen coordinates.</param>
        /// <param name="currentCamera">Camera used for 3d object picking</param>
        /// <param name="targetTexture">The texture used by the panel</param>
        /// <returns>Returns the coordinates in texel space, or a position containing NaN values if no hit was recorded or if the hit mesh's material is not using the render texture as their mainTexture</returns>
        private Vector2 ScreenCoordinatesToRenderTexture(Vector2 screenPosition) { 
            var invalidPosition = new Vector2(float.NaN, float.NaN);

            screenPosition.y = Screen.height - screenPosition.y;
            
            Vector2 distortedUV = ApplyLensDistortion(screenPosition / new Vector2(Screen.width, Screen.height));
            
            Vector2 distortedScreenPos = new Vector2(distortedUV.x * Screen.width, (1 - distortedUV.y) * Screen.height);
            
            var cameraRay = targetCamera.ScreenPointToRay(distortedScreenPos);

            RaycastHit hit;
            if (!Physics.Raycast(cameraRay, out hit))
            {
                return invalidPosition;
            }
            // Debug.Log("Raycast hit at " + hit.point);
            
            MeshRenderer rend = hit.transform.GetComponent<MeshRenderer>();

            if (rend == null || rend.sharedMaterial.mainTexture != targetTexture)
            {
                return invalidPosition;
            }

            Vector2 pixelUV = hit.textureCoord;

            //since y screen coordinates are usually inverted, we need to flip them
            pixelUV.y = 1 - pixelUV.y;

            pixelUV.x *= targetTexture.width;
            pixelUV.y *= targetTexture.height;

            return pixelUV;
        }
        
        private Vector2 ApplyLensDistortion(Vector2 uv) {
            if (lensDistortion == null || !lensDistortion.active)
            {
                // Debug.LogWarning("Lens Distortion is not active in the Volume.");
                return uv;
            }

            // Pull the values dynamically
            float intensity = lensDistortion.intensity.value;
            float scale = lensDistortion.scale.value;
            Vector2 lensCenter = lensDistortion.center.value;
            Vector2 lensXYMult = Vector2.one;

            // Apply distortion
            Vector2 half = Vector2.one * 0.5f;
            float amount = 1.6f * Mathf.Max(Mathf.Abs(intensity * 100f), 1f);
            float theta = Mathf.Deg2Rad * Mathf.Min(160f, amount);
            float sigma = 2f * Mathf.Tan(theta * 0.5f);
            Vector2 center = lensCenter * 2f - Vector2.one;

            Vector4 p1 = new Vector4(
                center.x,
                center.y,
                Mathf.Max(lensXYMult.x, 1e-4f),
                Mathf.Max(lensXYMult.y, 1e-4f)
            );
            Vector4 p2 = new Vector4(
                intensity >= 0f ? theta : 1f / theta,
                sigma,
                1f / scale,
                intensity * 100f
            );

            Vector2 transformedUV = (uv - half) * p2.z + half;
            Vector2 distAxis = new Vector2(p1.z, p1.w);
            Vector2 distCenter = new Vector2(p1.x, p1.y);
            Vector2 ruv = distAxis * (transformedUV - half - distCenter);

            float ru = ruv.magnitude;

            if (p2.w > 0.0)
            {
                float wu = ru * p2.x;
                ru = Mathf.Tan(wu) * (1f / (ru * p2.y));
                transformedUV = transformedUV + ruv * (ru - 1f);
            }
            else
            {
                ru = (1f / ru) * p2.x * Mathf.Atan(ru * p2.y);
                transformedUV = transformedUV + ruv * (ru - 1f);
            }

            return transformedUV;
        }


    }
}

