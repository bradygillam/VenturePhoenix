using System.Collections.Generic;
using UnityEngine;

public static class GameTiles
{
    public static List<Tile> tiles { get; set; }

    static GameTiles()
    {
        tiles = new List<Tile>();
    }

    public static Tile getTileByTriangleNormal(Vector3 triangleNormal)
    {
        float smallestDistance = float.MaxValue;
        Tile closestTile = null;
        
        foreach (Tile tile in tiles)
        {
            if (smallestDistance > Vector3.Distance(triangleNormal, tile.GetCenter()))
            {
                smallestDistance = Vector3.Distance(triangleNormal, tile.GetCenter());
                closestTile = tile;
            }
        }
        
        return closestTile;
    }
}