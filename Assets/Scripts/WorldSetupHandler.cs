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

    [SerializeField] private Material land;
    [SerializeField] private Material ocean;
    
    private Vector3[] vertices;
    private int[] triangles;
    private List<Tile> tiles;
    private Dictionary<(Vector3,Vector3), (Tile, Tile)> edges;
    
    private int numPentagons = 12;
    private int numVerticesPerTriangle = 3;
    private int numTrisPerPentagon = 3;
    private int numTrisPerHexagon = 4;
    
    [SerializeField] private float sphereOffset;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;
    [SerializeField] private float scale;
    [SerializeField] private float elevationSlider;
    [SerializeField] private int numTimesGrowLand;
    [SerializeField] private int numTimesGrowOcean;
    [SerializeField] private float maxMinElevation;
    
    void Start()
    {
        getRequiredComponents();
        
        mapTrianglesToTiles();

        mapEdgeNeighbours();

        assignLandAndOcean();
        
        paintTiles();
    }

    void Update()
    {
        //assignLandAndOcean();
        
    }

    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 1f, 0));
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
    
    private float CalculatePerlin(float x, float y, float z, float iterator)
    {
        return (Mathf.PerlinNoise(scale * (x + xOffset + (sphereOffset * iterator)), scale * (y + yOffset + (sphereOffset * iterator))) +
                Mathf.PerlinNoise(scale * (y + xOffset + (sphereOffset * iterator)), scale * (x + yOffset + (sphereOffset * iterator))) +
                Mathf.PerlinNoise(scale * (x + xOffset + (sphereOffset * iterator)), scale * (z + yOffset + (sphereOffset * iterator)))
                ) / 3f;
    }
    
    private void paintTiles()
    {
        List<int> landMesh = new List<int>();
        List<int> oceanMesh =  new List<int>();
        mesh.subMeshCount = 2;
        
        foreach (Tile tile in tiles)
        {
            if (tile.GetMaterial() == land)
            {
                landMesh.AddRange(tile.GetTriangles()); //
            }
            else
            {
                oceanMesh.AddRange(tile.GetTriangles());
            }
        }
        
        mesh.SetTriangles(landMesh, 0);
        mesh.SetTriangles(oceanMesh, 1);
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshRenderer.materials = new Material[]
        {
            land,
            ocean
        };

        meshFilter.mesh = mesh;
    }

    private float CalculateElevation(float perlinValue)
    {
        return maxMinElevation * (perlinValue - 0.5f);
    }

    private void assignLandAndOcean()
    {
        foreach (Tile tile in tiles)
        {
            float perlin = 0;
                
            for (float i = 1; i < 100; i += 32)
            {
                perlin += CalculatePerlin(tile.GetCenter().x, tile.GetCenter().y, tile.GetCenter().z, i) / 4;
            }
            
            tile.tileStats.elevation = CalculateElevation(perlin) + elevationSlider;
            
            if (tile.tileStats.elevation < 0)
            {
                tile.SetMaterial(ocean);
            }
            else
            {
                tile.SetMaterial(land);
            }
        }

        for (int i = 0; i < numTimesGrowLand; i++)
        {
            Queue<Tile> edgeRandomizerTile = new Queue<Tile>(tiles);

            while (edgeRandomizerTile.Count > 0)
            {
                Tile tile = edgeRandomizerTile.Dequeue();

                if (tile.GetMaterial() == land)
                {
                    continue;
                }

                int matchingMaterial = 0;

                foreach (Tile neighbour in tile.GetNeighbours())
                {
                    if (neighbour.GetMaterial() == land)
                    {
                        matchingMaterial++;
                    }
                }

                if (Random.value < (matchingMaterial) / (float)tile.GetNeighbours().Count)
                {
                    tile.SetMaterial(land);
                    foreach (Tile readd in tile.GetNeighbours())
                    {
                        edgeRandomizerTile.Enqueue(readd);
                    }
                }
            }
        }
        
        for (int i = 0; i < numTimesGrowOcean; i++)
        {
            Queue<Tile> edgeRandomizerTile = new Queue<Tile>(tiles);

            while (edgeRandomizerTile.Count > 0)
            {
                Tile tile = edgeRandomizerTile.Dequeue();

                if (tile.GetMaterial() == ocean)
                {
                    continue;
                }

                int matchingMaterial = 0;

                foreach (Tile neighbour in tile.GetNeighbours())
                {
                    if (neighbour.GetMaterial() == ocean)
                    {
                        matchingMaterial++;
                    }
                }

                if (Random.value < (matchingMaterial) / (float)tile.GetNeighbours().Count)
                {
                    tile.SetMaterial(ocean);
                    foreach (Tile readd in tile.GetNeighbours())
                    {
                        edgeRandomizerTile.Enqueue(readd);
                    }
                }
            }
        }
    }
}
