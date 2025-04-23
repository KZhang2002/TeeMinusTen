using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    public class ContourMap : MonoBehaviour {
        // Creates contour map from terrain heightmap data as Texture2D

        // use default colours and optional parameter numberOfBands
        public static Texture2D FromTerrain(Terrain terrain, int bandNum = 12, bool useBandNumAsDistance = false) {
            return FromTerrain(terrain, bandNum, Color.white, Color.clear, useBandNumAsDistance);
        }

        // define all parameters
        public static Texture2D FromTerrain(Terrain terrain, int bandNum, Color bandColor, Color bkgColor, bool useBandNumAsDistance = false) {
            // dimensions
            var width = terrain.terrainData.heightmapResolution;
            var height = width;

            // heightmap data
            var heightmap = terrain.terrainData.GetHeights(0, 0, width, height);

            // Create Output Texture2D with heightmap dimensions
            var topoMap = new Texture2D(width, height);
            topoMap.anisoLevel = 16;

            // array for storing colours to be applied to texture
            var colourArray = new Color[width * height];

            // Set background
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                colourArray[y * width + x] = bkgColor;

            // Initial Min/Max values for normalized terrain heightmap values
            var minHeight = 1f;
            float maxHeight = 0;

            // Find lowest and highest points
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) {
                if (minHeight > heightmap[y, x]) minHeight = heightmap[y, x];
                if (maxHeight < heightmap[y, x]) maxHeight = heightmap[y, x];
            }

            var realHeight = maxHeight * terrain.terrainData.size.y;
            Debug.Log("realHeight: " + realHeight);

            // Create height band list
            var bandDistance = useBandNumAsDistance ? bandNum * maxHeight/realHeight : (maxHeight - minHeight) / bandNum; // Number of height bands to create
            Debug.Log("bandDistance: " + bandDistance);

            var bands = new List<float>();

            // Get ranges
            var r = minHeight + bandDistance;
            while (r < maxHeight) {
                bands.Add(r);
                r += bandDistance;
                Debug.Log("band added: " + r);
            }

            // Create slice buffer
            var slice = new bool[width, height];

            // Draw bands
            for (var b = 0; b < bands.Count; b++) {
                // Get Slice
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    if (heightmap[y, x] >= bands[b])
                        slice[x, y] = true;
                    else
                        slice[x, y] = false;

                // Detect edges on slice and write to output
                for (var y = 1; y < height - 1; y++)
                for (var x = 1; x < width - 1; x++)
                    if (slice[x, y])
                        if (
                            slice[x - 1, y] == false ||
                            slice[x + 1, y] == false ||
                            slice[x, y - 1] == false ||
                            slice[x, y + 1] == false) {
                            // heightmap is read y,x from bottom left
                            // texture is read x,y from top left
                            // magic equation to find correct array index
                            var ind = (height - y - 1) * width + (width - x - 1);

                            colourArray[ind] = bandColor;
                        }
            }

            // apply colour array to texture
            topoMap.SetPixels(colourArray);
            topoMap.Apply();

            // Return result
            return topoMap;
        }
    }
}