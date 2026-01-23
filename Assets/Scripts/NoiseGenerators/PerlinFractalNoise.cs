using System;
using UnityEngine;

public class PerlinFractalNoise : MonoBehaviour, INoiseGenerator
{
    private float SPHEREOFFSET = 1f;
    
    public float calculateValue(Vector3 position, float scaleStart, float scaleEnd, float scaleJump, float xOffset, float yOffset)
    {
        int fractalDepth;

        if (scaleJump.Equals(0f))
        {
            scaleJump = 1f;
        }
        
        if (scaleStart > scaleEnd)
        {
            float i = scaleStart;
            scaleStart = scaleEnd;
            scaleEnd = scaleStart;
        } 
        
        if (scaleStart == scaleEnd)
        {
            fractalDepth = 1;
        }
        else
        {
            fractalDepth = Mathf.CeilToInt((scaleEnd - scaleStart) / scaleJump) + 2;
        }

        
        
        float fractalPerlinValue = 0f;
        float remainderInfluence = 1f;
        
        for (int i = 0; i < fractalDepth; i++)
        {
            float scale = scaleStart + (i * scaleJump);

            float fractalPerlinStepValue =
                (Mathf.PerlinNoise(scale * (position.x + xOffset + SPHEREOFFSET),
                     scale * (position.y + yOffset + SPHEREOFFSET)) +
                 Mathf.PerlinNoise(scale * (position.y + xOffset + SPHEREOFFSET),
                     scale * (position.z + yOffset + SPHEREOFFSET)) +
                 Mathf.PerlinNoise(scale * (position.z + xOffset + SPHEREOFFSET),
                     scale * (position.x + yOffset + SPHEREOFFSET))
                ) / 3f;

            if (i == fractalDepth - 1)
            { 
                fractalPerlinValue += fractalPerlinStepValue * remainderInfluence;
            }
            else
            {
                remainderInfluence /= 2f;
                fractalPerlinValue += fractalPerlinStepValue * remainderInfluence;
            }
        }
        
        return fractalPerlinValue;
    }
}
