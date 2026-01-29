using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldSetupHandler : MonoBehaviour
{
    [SerializeField] public Material land;
    [SerializeField] public Material ocean;
    [SerializeField] public Material mountain;
    
    private Dictionary<(Vector3,Vector3), (Tile, Tile)> edges;
    
    void Awake()
    {
        getRequiredComponents();

        addBiomesToMap();
        
        WorldMesh.mapTrianglesToTiles();

        mapEdgeNeighbours();
    }

    private void addBiomesToMap()
    {
        WorldMesh.mapBiomeToMaterials(Biome.ocean, ocean);
        WorldMesh.mapBiomeToMaterials(Biome.land, land);
        WorldMesh.mapBiomeToMaterials(Biome.mountain, mountain); 
    }
    
    private void getRequiredComponents()
    {
        WorldMesh.meshCollider = GetComponentInChildren<MeshCollider>();
        WorldMesh.meshRenderer = GetComponentInChildren<MeshRenderer>();
        WorldMesh.meshFilter = GetComponentInChildren<MeshFilter>();
        WorldMesh.mesh = Instantiate(WorldMesh.meshFilter.sharedMesh);
        WorldMesh.meshFilter.sharedMesh = WorldMesh.mesh;
        WorldMesh.meshCollider.sharedMesh = WorldMesh.mesh;
    }
    
    private void mapEdgeNeighbours()
    {
        edges = new Dictionary<(Vector3,Vector3), (Tile, Tile)>();
        
        foreach (Tile tile in GameTiles.tiles)
        {
            for (int i = 0; i < tile.GetTriangles().Length; i += 3)
            {
                addEdgeToList(WorldMesh.verticePositions[tile.GetTriangles()[i]], WorldMesh.verticePositions[tile.GetTriangles()[i + 1]], tile);
                addEdgeToList(WorldMesh.verticePositions[tile.GetTriangles()[i]], WorldMesh.verticePositions[tile.GetTriangles()[i + 2]], tile);
                addEdgeToList(WorldMesh.verticePositions[tile.GetTriangles()[i + 1]], WorldMesh.verticePositions[tile.GetTriangles()[i + 2]], tile);
            }
        }
        
        foreach ((Vector3,Vector3) edge in edges.Keys)
        {
            edges.TryGetValue(edge, out (Tile, Tile) tiles);

            if (tiles.Item2 != null)
            {
                tiles.Item1.AddNeighbour(tiles.Item2);
                tiles.Item2.AddNeighbour(tiles.Item1);
            }
        }
    }
    
    private void addEdgeToList(Vector3 tri1, Vector3 tri2, Tile tile)
    {
        (Vector3, Vector3) sortedVectorKey = sortVectors(tri1, tri2);

        if (!edges.TryGetValue(sortedVectorKey, out (Tile, Tile) tilesWithEdge))
        {
            edges[sortedVectorKey] = (tile, null);
        }
        else
        {
            edges[sortedVectorKey] = (tilesWithEdge.Item1, tile);
        }
    }
    
    private (Vector3, Vector3) sortVectors(Vector3 a, Vector3 b)
    {
        if (!Mathf.Approximately(a.x, b.x)) return a.x < b.x ? (a, b) : (b, a);
        if (!Mathf.Approximately(a.y, b.y)) return a.y < b.y ? (a, b) : (b, a);
        if (!Mathf.Approximately(a.z, b.z)) return a.z < b.z ? (a, b) : (b, a);
        return (a, b);
    }
}
