using System.Collections.Generic;
using UnityEngine;

public static class GameTiles
{
    public static List<Tile> tiles { get; set; }

    static GameTiles()
    {
        tiles = new List<Tile>();
    }

    public static Tile getTileByTriangleIndex(int triangleIndex)
    {
        Vector3 triangleCenter = (WorldMesh.verticePositions[triangleIndex] + WorldMesh.verticePositions[triangleIndex + 1] +
                                  WorldMesh.verticePositions[triangleIndex + 2]) / 3;
        
        float smallestDistance = float.MaxValue;
        Tile closestTile = null;
        
        foreach (Tile tile in tiles)
        {
            if (smallestDistance > Vector3.Distance(triangleCenter, tile.GetCenter()))
            {
                smallestDistance = Vector3.Distance(triangleCenter, tile.GetCenter());
                closestTile = tile;
            }
        }
        
        return closestTile;
    }
}