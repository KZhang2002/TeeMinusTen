using System.IO;
using UnityEngine;

namespace _Scripts {
    public class TopoLines : MonoBehaviour 
    {
        public Terrain terrain;
	
        public int numberOfBands = 12;
	
        public Color bandColor = Color.white;
        public Color bkgColor = Color.clear;
	
        public Renderer outputPlain;
	
        public Texture2D topoMap;
        
        void Start() {
            Debug.Log("Application.dataPath: " + Application.dataPath);
            Debug.Log("folderPath: " + folderPath);
            
            GenerateTopoLines();
            

            
            SaveTextureAsImage();
        }
        
        public string folderPath = "Terrain/MapTextures";
        public string fileName = "map";

        public void SaveTextureAsImage(bool saveAsPng = false)
        {
            if (topoMap == null)
            {
                Debug.LogError("No texture assigned.");
                return;
            }

            byte[] bytes = saveAsPng ? topoMap.EncodeToPNG() : topoMap.EncodeToJPG(100);
    
            string extension = saveAsPng ? ".png" : ".jpg";
            string fullPath = Path.Combine(Application.dataPath, folderPath);
            Directory.CreateDirectory(fullPath);

            string fullFilePath = Path.Combine(fullPath, fileName + extension);
            Debug.Log("Texture attempt save to: " + fullFilePath);
            File.WriteAllBytes(fullFilePath, bytes);

            Debug.Log("Texture saved to: " + fullFilePath);
        }

	
        void Update() 
        {
            // if ( Input.GetMouseButtonDown(0) )
            // {
            //     GenerateTopoLines();
            // }
        }
	
        void GenerateTopoLines() 
        {
            //topoMap = ContourMap.FromTerrain( terrain );
            //topoMap = ContourMap.FromTerrain( terrain, numberOfBands );
            topoMap = ContourMap.FromTerrain( terrain, numberOfBands, bandColor, bkgColor );
		
            if ( outputPlain )
            {
                outputPlain.material.mainTexture = topoMap;
            }
        }
    }
}