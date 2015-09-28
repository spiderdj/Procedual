using UnityEngine;
using System.Collections.Generic;

public class GenTest : MonoBehaviour {
    public Object Tile;
    public Object Wall;

    public GameObject player;
	// Use this for initialization
	void Start () {
        List<Room> rooms = Generator.Generate(30, 30, 5, 5, 20, 20, Tile, Wall);
        Globals.rooms = rooms;
        player.transform.position = new Vector3(rooms[0].x + rooms[0].width / 2, rooms[0].y + rooms[0].height / 2, -1);
        //Globals.map = new int[50, 50];

        //for (int i = 0; i < 4; i++)
        //{
        //    for (int j = 0; j < 4; j++)
        //    {
        //        Globals.map[9 + i, 9 + j] = 2;
        //    }
        //}

        //Globals.map[10, 10] = 1;

        //GameObject parent = new GameObject();
        //parent.name = "Dungeon";
        //for (int x = 0; x < Globals.map.GetLength(0); x++)
        //{
        //    for (int y = 0; y < Globals.map.GetLength(1); y++)
        //    {
        //        if (Globals.map[x, y] == 1)
        //        {
        //            GameObject obj = (GameObject)Instantiate(Wall);
        //            obj.transform.position = new Vector3(x, y, 0);
        //            obj.transform.SetParent(parent.transform);
        //        }
        //        else if (Globals.map[x, y] == 2)
        //        {
        //            GameObject obj = (GameObject)Instantiate(Tile);
        //            obj.transform.position = new Vector3(x, y, 0);
        //            obj.transform.SetParent(parent.transform);
        //        }
        //    }
        //}

	}

    public void OnDrawGizmos()
    {
        Generator.OnDrawGizmos();
    }

	
}
