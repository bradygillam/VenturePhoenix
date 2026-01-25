using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldSetupHandler : MonoBehaviour
{
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    [SerializeField] public Material land;
    [SerializeField] public Material ocean;
    [SerializeField] public Material mountain;
    
    private Vector3[] vertices;
    private int[] triangles;
    public List<Tile> tiles { get; private set; }
    private Dictionary<(Vector3,Vector3), (Tile, Tile)> edges;
    
    private int numPentagons = 12;
    private int numVerticesPerTriangle = 3;
    private int numTrisPerPentagon = 3;
    private int numTrisPerHexagon = 4;

    private Dictionary<Biome, Material> biomeMaterialMap;
    
    void Awake()
    {
        getRequiredComponents();

        mapBiomeToMaterials();
        
        mapTrianglesToTiles();

        mapEdgeNeighbours();
    }

    private void mapBiomeToMaterials()
    {
        biomeMaterialMap = new Dictionary<Biome, Material>();
        
        biomeMaterialMap.Add(Biome.ocean, ocean);
        biomeMaterialMap.Add(Biome.land, land);
        biomeMaterialMap.Add(Biome.mountain, mountain);
    }
    
    private void getRequiredComponents()
    {
        meshCollider = GetComponentInChildren<MeshCollider>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshFilter = GetComponentInChildren<MeshFilter>();
        mesh = Instantiate(meshFilter.sharedMesh);
    }
    
    /* Each triangle in the mesh is mapped to the tile they belong. First triangles in the mesh all belong to the 12 pentagons.
        Iterates over all pentagons then all hexagons */
    private void mapTrianglesToTiles()
    {
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        tiles = new List<Tile>();

        int numTrisPerTile = numTrisPerPentagon;
        int[] tileTriangles = new int[numTrisPerTile * numVerticesPerTriangle];
        
        for (int i = 0; i < numPentagons * numTrisPerPentagon * numVerticesPerTriangle; i += numVerticesPerTriangle)
        {
            tileTriangles[i % tileTriangles.Length] = triangles[i];
            tileTriangles[(i+1) % tileTriangles.Length] = triangles[i+1];
            tileTriangles[(i+2) % tileTriangles.Length] = triangles[i+2];

            if ((i + 2) % tileTriangles.Length == tileTriangles.Length - 1)
            {
                Tile tile = new Tile(tileTriangles, vertices);
                tiles.Add(tile);
                tileTriangles = new int[numTrisPerTile * numVerticesPerTriangle];
            }
        }
        
        numTrisPerTile = numTrisPerHexagon;
        tileTriangles = new int[numTrisPerTile * numVerticesPerTriangle];
        
        for (int i = numPentagons * numTrisPerPentagon * numVerticesPerTriangle; i < triangles.Length; i += numVerticesPerTriangle)
        {
            tileTriangles[i % tileTriangles.Length] = triangles[i];
            tileTriangles[(i+1) % tileTriangles.Length] = triangles[i+1];
            tileTriangles[(i+2) % tileTriangles.Length] = triangles[i+2];

            if ((i + 2) % tileTriangles.Length == tileTriangles.Length - 1)
            {
                Tile tile = new Tile(tileTriangles, vertices);
                tiles.Add(tile);
                tileTriangles = new int[numTrisPerTile * numVerticesPerTriangle];
            }
        }
    }
    
    private void mapEdgeNeighbours()
    {
        edges = new Dictionary<(Vector3,Vector3), (Tile, Tile)>();
        
        foreach (Tile tile in tiles)
        {
            for (int i = 0; i < tile.GetTriangles().Length; i += 3)
            {
                addEdgeToList(vertices[tile.GetTriangles()[i]], vertices[tile.GetTriangles()[i + 1]], tile);
                addEdgeToList(vertices[tile.GetTriangles()[i]], vertices[tile.GetTriangles()[i + 2]], tile);
                addEdgeToList(vertices[tile.GetTriangles()[i + 1]], vertices[tile.GetTriangles()[i + 2]], tile);
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
    
    public void paintTiles()
    {
        Dictionary<Biome, List<int>> biomeMeshMap = new Dictionary<Biome, List<int>>();
        
        foreach (Biome biome in biomeMaterialMap.Keys)
        {
            biomeMeshMap.Add(biome, new List<int>());
        }
        
        mesh.subMeshCount = biomeMeshMap.Count;
        
        foreach (Tile tile in tiles)
        {
            biomeMeshMap[tile.tileStats.biome].AddRange(tile.GetTriangles());
        }

        int i = 0;
        Material[] biomeMaterials = new Material[biomeMeshMap.Count];
        foreach (Biome biome in biomeMeshMap.Keys)
        {
            mesh.SetTriangles(biomeMeshMap[biome], i);
            biomeMaterials[i] = biomeMaterialMap[biome];
            i++;
        }
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshRenderer.materials = biomeMaterials;

        meshFilter.mesh = mesh;
    }
}
