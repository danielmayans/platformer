using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class TerrainEditor2D : MonoBehaviour
{
    public int MeshId;

    // --- Terrain size and resolution
    public int Width = 50;
    public int Height = 50;
    public int Resolution = 2;

    // --- Main terrain settings
    public Material MainMaterial;
    public int TextureSize = 15;
    public string SortingLayerName;
    public int OrderInLayer;

    // --- Rnd values for Randomizer
    public float RndAmplitude = 5;
    public int RndHillsCount = 5;
    public float RndHeight = 25;

    // --- Cap settings
    public bool CreateCap;
    public Material CapMaterial;
    public int CapTextureTiling = 50;
    public float CapHeight = 1;
    public float CapOffset;

    // --- Additional settings
    public bool FixSides;
    public float LeftFixedPoint = 25;
    public float RightFixedPoint = 25;
    public bool Create3DCollider;
    public float Collider3DWidth = 5;

    // --- Objects references
    public GameObject CapObj;
    public GameObject Collider3DObj;

    // --- public methods
    public void CreateTerrain() //Create new terrain
    {
        Mesh pathMesh;
        if (gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            pathMesh = new Mesh();
            pathMesh.name = "Terrain2D_mesh_" + MeshId;
        }
        else
        {
            pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            pathMesh.Clear();
        }

        Vector3[] verticles = new Vector3[((Width * 2) * Resolution) + 2];

        for (int i = 0; i < verticles.Length; i += 2) //Generate mesh verts
        {
            float vertsInterval = (i * 0.5f) / Resolution;

            verticles[i] = new Vector3(vertsInterval, Height * 0.5f, 0);
            verticles[i + 1] = new Vector3(vertsInterval, 0, 0);
        }
        if (FixSides)
        {
            verticles[0].y = LeftFixedPoint;
            verticles[verticles.Length - 2].y = RightFixedPoint;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;

        SetSrotingLayerAndOrder();

        UpdateCollider();
        CheckCapAnd3DCol();
    }

    public void GenerateCap(Vector3[] vertsPos) //Create cap of this terrain
    {
        if (CapObj == null)
        {
            CapObj = new GameObject("Terrain2D Cap");
            CapObj.transform.position = transform.position;
            CapObj.transform.parent = transform;
            CapObj.AddComponent<MeshFilter>();
            CapObj.AddComponent<MeshRenderer>();
        }

        Mesh pathMesh;
        if (CapObj.GetComponent<MeshFilter>().sharedMesh == null)
        {
            pathMesh = new Mesh();
            pathMesh.name = "Terrain2D_cap_mesh_" + MeshId;
        }
        else
        {
            pathMesh = CapObj.GetComponent<MeshFilter>().sharedMesh;
            pathMesh.Clear();
        }

        Vector3[] verticles = vertsPos;

        for (int i = 0; i < verticles.Length; i += 2) //Generate mesh verts
        {
            verticles[i] = new Vector3(verticles[i].x, verticles[i].y, -0.01f);
            verticles[i + 1] = new Vector3(verticles[i + 1].x, verticles[i].y - CapHeight, -0.01f);
            verticles[i].y += CapOffset;
            verticles[i + 1].y += CapOffset;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * CapTextureTiling, 1);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * CapTextureTiling, 0);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        CapObj.GetComponent<MeshFilter>().mesh = pathMesh;
        CapObj.GetComponent<Renderer>().material = CapMaterial;

        SetSrotingLayerAndOrder();
    }

    public void UpdateCollider() //Generate new path for PolygonCollider2D based on positions of top verticles
    {
        /*Vector3[] verticles = GetVertsPos();
        Vector2[] points = new Vector2[verticles.Length / 2];

        int point = 0;
        for (int i = 0; i < verticles.Length; i += 2)
        {
            points[point] = new Vector2(verticles[i].x, verticles[i].y);
            point++;
        }
        gameObject.GetComponent<EdgeCollider2D>().points = points;*/

        Vector3[] verticles = GetVertsPos();
        Vector2[] points = new Vector2[verticles.Length];
        
        int point = 0;
        for (int i = 0; i < verticles.Length; i += 2)
        {
            if (i >= verticles.Length - 1)
                break;

            points[point] = new Vector2(verticles[i].x, verticles[i].y);
            point++;
        }

        for (int i = verticles.Length - 1; i > 1; i -= 2)
        {
            if (i <= 0)
                break;

            points[point] = new Vector2(verticles[i].x, verticles[i].y);
            point++;
        }

        gameObject.GetComponent<PolygonCollider2D>().SetPath(0, points);
    }

    public void UpdateCollider3D()
    {
        Mesh collider3DMesh;
        if (Collider3DObj == null)
        {
            Collider3DObj = new GameObject("Terrain2D Collider3D");
            Collider3DObj.transform.parent = transform;
            Collider3DObj.transform.localPosition = Vector3.zero;

            Collider3DObj.AddComponent<MeshFilter>();
            Collider3DObj.AddComponent<MeshCollider>();

            collider3DMesh = new Mesh();
            collider3DMesh.name = "Terrain2D_collider3D_mesh_" + MeshId;
        }
        else
        {
            collider3DMesh = Collider3DObj.GetComponent<MeshFilter>().sharedMesh;
        }

        Vector3[] verticles = GetVertsPos();
        Vector3[] vertsPoints = new Vector3[verticles.Length];

        int point = 0;
        for (int i = 0; i < verticles.Length; i += 2)
        {
            vertsPoints[point] = new Vector3(verticles[i].x, verticles[i].y, -Collider3DWidth * 0.5f);
            vertsPoints[point + 1] = new Vector3(verticles[i].x, verticles[i].y, Collider3DWidth * 0.5f);
            point += 2;
        }

        #region ConfigureMesh

        int[] tris = new int[(vertsPoints.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < vertsPoints.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            else
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[vertsPoints.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        collider3DMesh.vertices = vertsPoints;
        collider3DMesh.triangles = tris;
        collider3DMesh.normals = normals;
        collider3DMesh.uv = uv;

        collider3DMesh.RecalculateBounds();

        #endregion

        Collider3DObj.GetComponent<MeshFilter>().mesh = collider3DMesh;
        Collider3DObj.GetComponent<MeshCollider>().sharedMesh = Collider3DObj.GetComponent<MeshFilter>().sharedMesh;

    } //Generate 3D Mesh Collider based on positions of top verticles

    public void EditMesh(Vector3[] newVertsPos) //Edit mesh using new verticles array
    {
        Mesh pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticles = newVertsPos;

        if (FixSides)
        {
            verticles[0].y = LeftFixedPoint;
            verticles[verticles.Length - 2].y = RightFixedPoint;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;

        CheckCapAnd3DCol();
    }

    public void RandomizeTerrain()//Generate terrain based on Rnd values
    {
        Mesh pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticles = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;

        int v = (Width * Resolution) / RndHillsCount;
        int step = 0;

        float a = RandomizeVertexPoint();

        if (FixSides)
            verticles[0].y = LeftFixedPoint;
        else
        {
            verticles[0].y = a;
            a = RandomizeVertexPoint();
        }

        for (int i = 2; i < verticles.Length; i += 2) //Generate mesh verts
        {
            if (step >= v)
            {
                a = RandomizeVertexPoint();
                step = 0;
            }

            verticles[i].y = Mathf.Lerp(verticles[i - 2].y, a, ((float)step / v) / Resolution);

            step++;

        }

        if (FixSides)
            RightFixedPoint = verticles[verticles.Length - 2].y;

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;
        gameObject.GetComponent<MeshRenderer>().material = MainMaterial;

        CheckCapAnd3DCol();
    }

    public void RandomizeTerrain(float lastVertexPoint) //Generate terrain based on Rnd values and last vertex point
    {
        Mesh pathMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticles = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;

        int v = (Width * Resolution) / RndHillsCount;
        int step = 0;

        float a = RandomizeVertexPoint();
        verticles[0].y = lastVertexPoint;

        for (int i = 2; i < verticles.Length; i += 2) //Generate mesh verts
        {
            if (step >= v)
            {
                a = RandomizeVertexPoint();
                step = 0;
            }

            if (!FixSides)
                verticles[i].y = Mathf.Lerp(verticles[i - 2].y, a, ((float)step / v) / Resolution);
            else
            {
                verticles[i].y = Mathf.Lerp(verticles[i - 2].y, a, ((float)step / v) / Resolution);
            }

            step++;
        }

        #region ConfigureMesh
        int[] tris = new int[(verticles.Length - 2) * 3];

        bool toSide = false;
        int curTrisIndex = 0;
        for (int i = 0; i < verticles.Length - 2; i++)
        {
            if (toSide)
            {
                tris[curTrisIndex] = i;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i + 2;
            }
            else
            {
                tris[curTrisIndex] = i + 2;
                tris[curTrisIndex + 1] = i + 1;
                tris[curTrisIndex + 2] = i;
            }
            toSide = !toSide;

            curTrisIndex += 3;
        }

        Vector3[] normals = new Vector3[verticles.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }

        Vector2[] uv = new Vector2[verticles.Length];

        for (int i = 0; i < uv.Length; i += 2)
        {
            uv[i] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i].y / Height) * ((float)Height / Width) * TextureSize);
            uv[i + 1] = new Vector2((float)i / (uv.Length - 2) * TextureSize, (verticles[i + 1].y / Height) * ((float)Height / Width) * TextureSize);
        }

        pathMesh.vertices = verticles;
        pathMesh.triangles = tris;
        pathMesh.normals = normals;
        pathMesh.uv = uv;
        pathMesh.RecalculateBounds();

        #endregion

        gameObject.GetComponent<MeshFilter>().mesh = pathMesh;
        gameObject.GetComponent<MeshRenderer>().material = MainMaterial;

        CheckCapAnd3DCol();
    }

    public void SetSrotingLayerAndOrder()
    {
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = SortingLayerName;
        gameObject.GetComponent<MeshRenderer>().sortingOrder = OrderInLayer;

        if (CapObj != null)
        {
            CapObj.GetComponent<MeshRenderer>().sortingLayerName = SortingLayerName;
            CapObj.GetComponent<MeshRenderer>().sortingOrder = OrderInLayer;
        }
    }

    public float GetLastVertexPoint() //Get last verticle point of this mesh for use it in next random generated terrain
    {
        return gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[gameObject.GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2].y;
    }

    public Vector3[] GetVertsPos() //Get verticles array of this mesh
    {
        if (gameObject.GetComponent<MeshFilter>().sharedMesh != null)
            return gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
        return null;
    }


    // --- private methods
    void CheckCapAnd3DCol()
    {
        if (CreateCap)
            GenerateCap(GetVertsPos());
        else
        {
            if (CapObj != null)
                DestroyImmediate(CapObj);
        }

        if (Create3DCollider)
            UpdateCollider3D();
        else
        {
            if (Collider3DObj != null)
                DestroyImmediate(Collider3DObj);
        }
    } //Check if cap and/or 3D collider is needs to be created and generate them

    float RandomizeVertexPoint() //Set random verticle point based on randomizer values
    {
        float a = RndHeight - RndAmplitude;
        float b = RndHeight + RndAmplitude;

        if (a < 0.1f)
            a = 0.1f;
        if (b > Height)
            b = Height;

        return Random.Range(a, b);
    }


    // --- static methods
    public static GameObject InstantiateTerrain2D(Vector3 position)
    {
        GameObject newTerrain = new GameObject("New Terrain2D");
        newTerrain.transform.position = position;
        newTerrain.AddComponent<MeshFilter>();
        newTerrain.AddComponent<MeshRenderer>();
        newTerrain.AddComponent<PolygonCollider2D>();
        newTerrain.AddComponent<TerrainEditor2D>();

        newTerrain.GetComponent<TerrainEditor2D>().CreateTerrain();

        return newTerrain;
    }

    public static GameObject InstantiateTerrain2D(Vector3 position, int width, int height, int resolution)
    {

        GameObject newTerrain = new GameObject("New Terrain2D");
        newTerrain.transform.position = position;
        newTerrain.AddComponent<MeshFilter>();
        newTerrain.AddComponent<MeshRenderer>();
        newTerrain.AddComponent<PolygonCollider2D>();
        newTerrain.AddComponent<TerrainEditor2D>();

        newTerrain.GetComponent<TerrainEditor2D>().Width = width;
        newTerrain.GetComponent<TerrainEditor2D>().Height = height;
        newTerrain.GetComponent<TerrainEditor2D>().Resolution = resolution;

        newTerrain.GetComponent<TerrainEditor2D>().CreateTerrain();

        return newTerrain;
    }
}


