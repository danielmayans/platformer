//Runtime terrain generator example

using System.Collections.Generic;
using UnityEngine;

public class Terrain2DGenerator : MonoBehaviour
{
    public Transform Target; //Player transform

    public Material Terrain2DMaterial; //Default terrain material
    public Material TerrainCapMaterial; //Default cap material

    private GameObject _lastTerrain2D; //last randomly generated terrain

    private float _lastTargetPos; //last Player position by X 

    private List<GameObject> _createdTerrains; //List array of terrains that already placed on the scene


	void Start () 
    {
        _createdTerrains = new List<GameObject>();
        CreateNextTerrain2D(Vector2.zero); //Create first terrain
	}

    void Update()
    {
        if (Target.transform.position.x > _lastTargetPos) //Is player already cross the line and want to see new terrain?
        {
            _lastTargetPos += 50; //Change last Player position by x based on terrain width (50 by defaul)
            CreateNextTerrain2D(new Vector2(_lastTargetPos, 0));
        }

        if (_createdTerrains.Count > 5) //Check if scene has more that 5 terrains
        {
            //We assume that the player always move forward and does not see terrains that stayed behind him
            Destroy(_createdTerrains[0]);
            _createdTerrains.RemoveAt(0); //Destroy old terrains
        }
    }

    void FixedUpdate()
    {
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(Target.transform.position.x, Target.transform.position.y, Camera.main.transform.position.z), Time.deltaTime * 25f);
        Target.transform.Translate(Vector3.right * 0.5f);
    }

    void CreateNextTerrain2D(Vector2 position)
    {
        //STEP 1. Create terrain
        GameObject newTerrain = TerrainEditor2D.InstantiateTerrain2D(position); //Creating default terrain

        TerrainEditor2D myTerrain = newTerrain.GetComponent<TerrainEditor2D>(); //Get TerrainEditor2D component 

        //STEP 2. Configure terrain
        myTerrain.MainMaterial = Terrain2DMaterial;  //Assign material to generated terrain
        myTerrain.CapMaterial = TerrainCapMaterial;  //Assign material to terrain cap

        //Configure cap
        myTerrain.CapHeight = 0.75f;
        myTerrain.CapOffset = 0.1f;
        myTerrain.CreateCap = true; //Set TRUE if terrain cap will be generated

        //STEP 3. Randomize terrain
        if (_lastTerrain2D != null)
            myTerrain.RandomizeTerrain(_lastTerrain2D.GetComponent<TerrainEditor2D>().GetLastVertexPoint()); //Randomize terrain and connect it with latter
        else myTerrain.RandomizeTerrain(); //Randomize terrain if previous is not exist

        //STEP 4. Recalculate terrain collider
        myTerrain.UpdateCollider(); //Calculate Collider path for terrain

        //DONE! New terrain generated!
        //If you need to change other terrain parameters (like: myTerrain.Width = 100; myTerrain.Height = 25; myTerrain.TextureSize = 50; etc.) use myTerrain.CreateTerrain(); before myTerrain.RandomizeTerrain();

        _createdTerrains.Add(newTerrain); //Add new terrain to terrains list
        _lastTerrain2D = newTerrain;
    }
}
