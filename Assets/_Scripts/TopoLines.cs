using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Scripts {
    public class TopoLines : MonoBehaviour 
    {
        public Terrain terrain;
	
        public int numberOfBands = 10;
        public int bandDistance = 100;
        public bool useDistance = false;
	
        public Color bandColor = Color.white;
        private Color _srgbBandColor;
        public Color bkgColor = Color.clear;
        private Color _srgbBkgColor;
	
        public Renderer outputPlain;
        public RawImage outputUI;
        public Texture2D outputTexture;
	
        public Texture2D topoMap;
        public Texture2D topoUpscaleMap;

        private TerminalUI _ui;
        private string _filePath;
        
        public static Color SRGBToLinear(Color color) {
            float r = color.r <= 0.04045f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
            float g = color.g <= 0.04045f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
            float b = color.b <= 0.04045f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);

            return new Color(r, g, b, color.a); // Preserves alpha
        }
        
        void Start() {
            Debug.Log("Application.dataPath: " + Application.dataPath);
            Debug.Log("folderPath: " + folderPath);
            
            _srgbBkgColor = SRGBToLinear(bkgColor);
            _srgbBandColor = SRGBToLinear(bandColor);
            
            GenerateTopoLines();
            string sceneName = SceneManager.GetActiveScene().name;
            SaveTextureAsImage(topoMap, sceneName + "_map", false);
            _ui.reloadTopoMapImage(_filePath);


            _ui = GetComponent<TerminalUI>();
        }
        
        public string folderPath = "";

        public void SaveTextureAsImage(Texture2D tex, string name, bool saveAsPng = false)
        {
            if (tex == null)
            {
                Debug.LogError("No texture assigned.");
                return;
            }
            
            byte[] bytes = saveAsPng ? tex.EncodeToPNG() : tex.EncodeToJPG(100);
    
            string extension = saveAsPng ? ".png" : ".jpg";
            string fullPath = Path.Combine(Application.persistentDataPath, folderPath);
            Directory.CreateDirectory(fullPath);
            
            _filePath = Path.Combine(fullPath, name + extension);
            File.WriteAllBytes(_filePath, bytes);
            
            _filePath = Path.Combine(Application.persistentDataPath, name + extension);
            File.WriteAllBytes(_filePath, bytes);
           
            
            

            // Debug.Log("Texture saved to: " + fullFilePath);
        }
        
        Texture2D FlipTextureY(Texture2D original)
        {
            int width = original.width;
            int height = original.height;
            Texture2D flippedTex = new Texture2D(width, height, original.format, false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Flip on Y-axis: take pixel from the mirrored y position
                    flippedTex.SetPixel(x, height - y - 1, original.GetPixel(x, y));
                }
            }
            flippedTex.Apply();
            return flippedTex;
        }


	
        void GenerateTopoLines() 
        {
            //topoMap = ContourMap.FromTerrain( terrain );
            //topoMap = ContourMap.FromTerrain( terrain, numberOfBands );
            
            topoMap = ContourMap.FromTerrain( terrain, useDistance ? bandDistance : numberOfBands, bandColor, bkgColor, true );
		
            if ( outputPlain ) {
                outputPlain.material.mainTexture = topoMap;
                outputTexture = topoMap;
            }
            
            if (outputUI) {
                outputUI.texture = topoMap;
            }
        }
        
        Texture2D ThickenLines(Texture2D source, int scaleFactor = 4, int radius = 2)
        {
            int width = source.width * scaleFactor;
            int height = source.height * scaleFactor;

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Bilinear;
            
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bkgColor;

            Color[] srcPixels = source.GetPixels();

            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    Color col = srcPixels[y * source.width + x];
                    if (ColorsApproximatelyEqual(col, bandColor))
                    {
                        int cx = x * scaleFactor;
                        int cy = y * scaleFactor;

                        // Draw a filled circle at scaled position
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                if (dx * dx + dy * dy <= radius * radius)
                                {
                                    int px = cx + dx;
                                    int py = cy + dy;
                                    if (px >= 0 && py >= 0 && px < width && py < height)
                                    {
                                        pixels[py * width + px] = col;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
        
        private bool ColorsApproximatelyEqual(Color a, Color b, float tolerance = 0.01f)
        {
            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance &&
                   Mathf.Abs(a.a - b.a) < tolerance;
        }

    }
}