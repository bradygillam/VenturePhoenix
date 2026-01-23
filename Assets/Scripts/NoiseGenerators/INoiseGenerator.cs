using UnityEngine;

public interface INoiseGenerator
{
    float calculateValue(Vector3 position, float scaleStart, float scaleEnd, float scaleJump, float xOffset, float yOffset);
}