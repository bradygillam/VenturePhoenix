using System.Collections.Generic;
using UnityEngine;

public class WorldGenerationHandler : MonoBehaviour
{
    [SerializeField] private string seedString;
    [SerializeField] private float scaleStart;
    [SerializeField] private float scaleEnd;
    [SerializeField] private float scaleJump;
    [SerializeField] private float elevationSlider;
    [SerializeField] private float maxMinElevation;
    
    private INoiseGenerator noiseGenerator;
    private List<Tile> tiles;
    private int seed;
    private float xOffset;
    private float yOffset;
    
    private float ASSUMED_PERLIN_AVERAGE = 0.5f;
    
    void Start()
    {
        noiseGenerator = GetComponent<INoiseGenerator>();
        assignLandAndOcean();
    }
    
    public void assignLandAndOcean()
    {
        useSeed();
        
        foreach (Tile tile in GameTiles.tiles)
        {
            float noise = noiseGenerator.calculateValue(tile.GetCenter(), scaleStart, scaleEnd, scaleJump, xOffset, yOffset);
            
            tile.tileStats.elevation = CalculateElevation(noise) + elevationSlider;
            
            if (tile.tileStats.elevation >= 100)
            {
                tile.tileStats.biome = Biome.mountain;
            }
            else if (tile.tileStats.elevation >= 0)
            {
                tile.tileStats.biome = Biome.land;
            }
            else
            {
                tile.tileStats.biome = Biome.ocean;
            }
        }
        
        WorldMesh.paintTiles();
    }

    private void useSeed()
    {
        seed = seedString.GetHashCode();
        Random.InitState(seed);
        xOffset = Random.Range(0f, 10000f);
        yOffset = Random.Range(0f, 10000f);
    }
    
    private float CalculateElevation(float perlinValue)
    {
        return maxMinElevation * (perlinValue - ASSUMED_PERLIN_AVERAGE);
    }
}
