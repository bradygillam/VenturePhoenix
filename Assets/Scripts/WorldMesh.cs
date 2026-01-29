using System.Collections.Generic;
using UnityEngine;

public static class WorldMesh
{
    private static int NUM_PENTAGON = 12;
    private static int NUM_VERTEX_PER_TRIANGLE = 3;
    private static int NUM_TRIANGLE_PER_PENTAGON = 3;
    private static int NUM_TRIANGLE_PER_HEXAGON = 4;
    
    public static MeshCollider meshCollider;
    public static MeshRenderer meshRenderer;
    public static MeshFilter meshFilter;
    public static Mesh mesh;
    public static Vector3[] verticePositions { get; set; }
    public static int[] triangles { get; set; }
    
    public static Dictionary<Biome, Material> biomeMaterialMap;
    
    /* Each triangle in the mesh is mapped to the tile they belong. First triangles in the mesh all belong to the 12 pentagons.
        Iterates over all pentagons then all hexagons */
    public static void mapTrianglesToTiles()
    {
        verticePositions = mesh.vertices;
        triangles = mesh.triangles;
        GameTiles.tiles = new List<Tile>();

        int numTrisPerTile = NUM_TRIANGLE_PER_PENTAGON;
        int[] tileTriangles = new int[numTrisPerTile * NUM_VERTEX_PER_TRIANGLE];
        
        for (int i = 0; i < NUM_PENTAGON * NUM_TRIANGLE_PER_PENTAGON * NUM_VERTEX_PER_TRIANGLE; i += NUM_VERTEX_PER_TRIANGLE)
        {
            tileTriangles[i % tileTriangles.Length] = triangles[i];
            tileTriangles[(i+1) % tileTriangles.Length] = triangles[i+1];
            tileTriangles[(i+2) % tileTriangles.Length] = triangles[i+2];

            if ((i + 2) % tileTriangles.Length == tileTriangles.Length - 1)
            {
                Tile tile = new Tile(tileTriangles, verticePositions);
                GameTiles.tiles.Add(tile);
                tileTriangles = new int[numTrisPerTile * NUM_VERTEX_PER_TRIANGLE];
            }
        }
        
        numTrisPerTile = NUM_TRIANGLE_PER_HEXAGON;
        tileTriangles = new int[numTrisPerTile * NUM_VERTEX_PER_TRIANGLE];
        
        for (int i = NUM_PENTAGON * NUM_TRIANGLE_PER_PENTAGON * NUM_VERTEX_PER_TRIANGLE; i < triangles.Length; i += NUM_VERTEX_PER_TRIANGLE)
        {
            tileTriangles[i % tileTriangles.Length] = triangles[i];
            tileTriangles[(i+1) % tileTriangles.Length] = triangles[i+1];
            tileTriangles[(i+2) % tileTriangles.Length] = triangles[i+2];

            if ((i + 2) % tileTriangles.Length == tileTriangles.Length - 1)
            {
                Tile tile = new Tile(tileTriangles, verticePositions);
                GameTiles.tiles.Add(tile);
                tileTriangles = new int[numTrisPerTile * NUM_VERTEX_PER_TRIANGLE];
            }
        }
    }
    
    public static void paintTiles()
    {
        Dictionary<Biome, List<int>> biomeMeshMap = new Dictionary<Biome, List<int>>();
        
        foreach (Biome biome in biomeMaterialMap.Keys)
        {
            biomeMeshMap.Add(biome, new List<int>());
        }
        
        mesh.subMeshCount = biomeMeshMap.Count;
        
        foreach (Tile tile in GameTiles.tiles)
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

        meshRenderer.materials = biomeMaterials;
    }
    
    public static void mapBiomeToMaterials(Biome biome, Material material)
    {
        if (biomeMaterialMap == null)
        {
            biomeMaterialMap = new Dictionary<Biome, Material>();
        }
        
        biomeMaterialMap.Add(biome, material);
    }
}