using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile
{
    public TileStats tileStats { get; }

    private Vector3 tileCenter;
    private int[] verticeIndexes;
    private List<Vector3> verticePositions;

    private List<Tile> tileNeighbours;
    
    
    public Tile(int[] verticeIndexes, Vector3[] vertexPositions)
    {
        tileStats = new TileStats();
        this.verticeIndexes = verticeIndexes;
        verticePositions = new List<Vector3>();
        foreach (int vertexIndex in this.verticeIndexes)
        {
            if (!verticePositions.Contains(vertexPositions[vertexIndex]))
            {
                verticePositions.Add(vertexPositions[vertexIndex]);
            }
        }
        tileCenter = verticePositions.Aggregate(Vector3.zero, (sum, vertexPosition) => sum + vertexPosition) / verticePositions.Count;
        tileNeighbours = new List<Tile>();
    }

    public int[] GetTriangles()
    {
        return verticeIndexes;
    }

    public Vector3 GetCenter()
    {
        return tileCenter;
    }

    public List<Tile> GetNeighbours()
    {
        return tileNeighbours;
    }

    public void AddNeighbour(Tile neighbourIndex)
    {
        tileNeighbours.Add(neighbourIndex);
    }
}
