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
    private WorldSetupHandler worldSetupHandler;
    private List<Tile> tiles;
    private int seed;
    private float xOffset;
    private float yOffset;
    
    void Start()
    {
        noiseGenerator = GetComponent<INoiseGenerator>();
        worldSetupHandler = GetComponent<WorldSetupHandler>();
        tiles = worldSetupHandler.tiles;
        assignLandAndOcean();
    }
    
    public void assignLandAndOcean()
    {
        useSeed();
        
        foreach (Tile tile in tiles)
        {
            float noise = noiseGenerator.calculateValue(tile.GetCenter(), scaleStart, scaleEnd, scaleJump, xOffset, yOffset);
            
            tile.tileStats.elevation = CalculateElevation(noise) + elevationSlider;
            
            if (tile.tileStats.elevation < 0)
            {
                tile.SetMaterial(worldSetupHandler.ocean);
            }
            else
            {
                tile.SetMaterial(worldSetupHandler.land);
            }
        }
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
        float ASSUMEDPERLINAVERAGE = 0.5f;
        
        return maxMinElevation * (perlinValue - ASSUMEDPERLINAVERAGE);
    }
    
}
