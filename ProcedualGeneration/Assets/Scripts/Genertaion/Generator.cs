using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Room
{
    public int x;
    public int y;
    public int width;
    public int height;
    public List<Room> connectedRooms = new List<Room>();

    public Room(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public void ConnectRoom(Room room)
    {
        connectedRooms.Add(room);
    }

    public bool isConnectedToRoom(Room room)
    {
        return connectedRooms.Contains(room);
    }
}

public class Generator : MonoBehaviour {

    //Stores data about where tiles are placed
    private static int[,] tilePlacements;
    private static List<Room> rooms;

    private static float roomChance = 1.0f;
    private static float CorridorDistance = 30.0f;

    private static PathFinder pathFinder;
    private static System.Func<int, Direction,int> getPathingValueFunc;

    public static List<Room> Generate(int LevelWidth, int LevelHeight, int MinRoomWidth, int MinRoomHeight, int MaxRoomWidth, int MaxRoomHeight, Object Tile, Object Wall)
    {
        //Make sure that the width and height of the tile array is the size of the level
        tilePlacements = new int[LevelWidth, LevelHeight];
        pathFinder = new PathFinder(tilePlacements);
        pathFinder.pathDiagonal = false; //Stop diagonal path finding
        getPathingValueFunc = CalculatePathingValue;
        rooms = new List<Room>();

        for (int y = 0; y < LevelHeight; y++)
        {
            for (int x = 0; x < LevelWidth; x++)
            {
                genRoom(x, y, MinRoomWidth, MinRoomHeight, MaxRoomWidth, MaxRoomHeight, LevelWidth, LevelHeight);
            }
        }
        MakeCorridors();
        placeInLevel(Tile,Wall);
        return rooms;
    }

    static void genRoom(int x, int y, int MinRoomWidth, int MinRoomHeight, int MaxRoomWidth, int MaxRoomHeight, int LevelWidth, int LevelHeight)
    {
        int freeX = getFreeSpaceX(x, y);
        int freeY = MaxRoomHeight;

        for (int i = 0; i < MaxRoomHeight; i++)
        {
            if (y + i < LevelHeight)
            {
                int fX = getFreeSpaceX(x, y + i);

                if (fX < MinRoomWidth)
                {
                    freeY = i - 1;
                    break;
                }

                if (fX < freeX)
                    freeX = fX;
            }
            else
            {
                freeY = i - 1;
                break;
            }
        }

        freeX = Mathf.Clamp(freeX, 0, MaxRoomWidth);

        if (freeX < MinRoomWidth || freeY < MinRoomHeight)
        {
            return;
        }

        float ran = Random.Range(0, 1);

        if (ran <= roomChance)
        {
            int budgeRoomX = freeX - MinRoomWidth;
            int budgeRoomY = freeY - MinRoomHeight;

            int offsetX = Random.Range(0, budgeRoomX);
            int offsetY = Random.Range(0, budgeRoomY);

            int roomWidth = Random.Range(MinRoomWidth, freeX - offsetX);
            int roomHeight = Random.Range(MinRoomHeight, freeY - offsetY);

            rooms.Add(new Room(x + offsetX, y + offsetY, roomWidth, roomHeight));
            placeRoom(x + offsetX, y + offsetY, roomWidth, roomHeight);
        }
    }


    private static void MakeCorridors()
    {
        foreach (Room room in rooms)
        {
            List<Room> nearbyRooms = getRoomsAround(room);
            foreach (Room rm in nearbyRooms)
            {
                if(!room.isConnectedToRoom(rm))
                {
                    JoinRooms(room,rm);
                }
            }
        }
    }

    private static void JoinRooms(Room room, Room room1)
    {
        room.ConnectRoom(room1);
        List<Point> path = pathFinder.findPath(new Point(room.x + room.width / 2, room.y + room.height / 2),new Point(room1.x + room1.width/2, room1.y + room1.height/2),getPathingValueFunc);
        foreach (var point in path)
        {
            tilePlacements[point.x, point.y] = 2;

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if(point.x + x >= 0 && point.x + x < tilePlacements.GetLength(0) && point.y + y >= 0 && point.y + y < tilePlacements.GetLength(1))
                    {
                        if(tilePlacements[point.x + x, point.y + y] == 0)
                            tilePlacements[point.x + x, point.y + y] = 1;
                    }
                }
            }


        }
    }

    private static int CalculatePathingValue(int tileValue, Direction direction)
    {
        //We don't care about direction as we can't path diagonally
        return (3 - tileValue);
    }

   

    private static List<Room> getRoomsAround(Room room)
    {
        List<Room> results = new List<Room>();

        foreach (Room rm in rooms)
        {
            if (room == rm)
                continue;

            float distance = Mathf.Sqrt(Mathf.Pow((rm.x - room.x), 2) + Mathf.Pow((rm.y - room.y), 2));
            if(distance < CorridorDistance)
            {
                results.Add(rm);
            }
        }

        return results;
    }

    private static int getFreeSpaceX(int x, int y)
    {
        for (int i = 0; i < tilePlacements.GetLength(0)-x; i++)
        {
           
            if(tilePlacements[x+i,y]!=0)
            {
                return i;
            }
        }
        return tilePlacements.GetLength(0) - x;
    }

    private static void placeRoom(int x, int y, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int tileId = 2;

                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                    tileId = 1;

                tilePlacements[x + i, y + j] = tileId;
            }
        }
    }

    private static void placeInLevel(Object Tile, Object Wall)
    {
        GameObject parent = new GameObject();
        Globals.map = tilePlacements;
        parent.name = "Dungeon";
        for (int x = 0; x < tilePlacements.GetLength(0); x++)
        {
            for (int y = 0; y < tilePlacements.GetLength(1); y++)
            {
                if(tilePlacements[x,y] == 1)
                {
                    GameObject obj = (GameObject)Instantiate(Wall);
                    obj.transform.position = new Vector3(x, y, 0);
                    obj.transform.SetParent(parent.transform);
                }
                else if(tilePlacements[x,y]==2)
                {
                    GameObject obj = (GameObject)Instantiate(Tile);
                    obj.transform.position = new Vector3(x, y, 0);
                    obj.transform.SetParent(parent.transform);
                }
            }
        }
    }

    public static void OnDrawGizmos()
    {
        for(int i=0;i<rooms.Count;i++)
        {
            Room room = rooms[i];
            Color color = Color.Lerp(Color.red, Color.blue, (float)i / (float)rooms.Count);
            Gizmos.color = color;
            Gizmos.DrawCube(new Vector3(room.x + room.width / 2, room.y + room.height / 2, 0), new Vector3(room.width, room.height, 1));
        }
    }



	
}
