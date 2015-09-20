using UnityEngine;
using System.Collections.Generic;

public class GenTest : MonoBehaviour {
    public Object Tile;
    public Object Wall;

    public GameObject player;
	// Use this for initialization
	void Start () {
        List<Room> rooms = Generator.Generate(50, 50, 5, 5, 20, 20, Tile,Wall);
        player.transform.position = new Vector3(rooms[0].x+rooms[0].width/2, rooms[0].y+rooms[0].height/2,-1);
	}

    public void OnDrawGizmos()
    {
        Generator.OnDrawGizmos();
    }

	
}
