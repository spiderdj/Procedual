using UnityEngine;
using System.Collections.Generic;
using System.Threading;
public class Enemy : MonoBehaviour {

    public GameObject player;
    public float moveSpeed = 5.0f;

    private PathFinder pathFinder;
    private System.Func<int, Direction, int> GetPathingValueFunc;
    List<Point> path;
    int currentPointIndex = 0;
    public bool gotPatrolPath = false;

    bool initalise = false;
    bool generatingPath = false;
    Vector2 startPos;
    Point endPoint;

    public Object bloodPrefab;
	// Use this for initialization
	void Start () {

	}
	
    void Initalise()
    {
        Point p = getRandomPoint();
        transform.position = new Vector3(p.x, p.y, -1);
        pathFinder = new PathFinder(Globals.map);
        pathFinder.pathDiagonal = true;
        GetPathingValueFunc = GetPathingValue;
        initalise = true;
    }

	// Update is called once per frame
	void Update () {

        if (!initalise)
            Initalise();

        ChasePlayer();
	}


    void ChasePlayer()
    {
        if(path==null||path.Count == 0 || Vector2.Distance(new Vector2(path[path.Count-1].x,path[path.Count-1].y),player.transform.position)>2)
        {
            if(!generatingPath)
            {
                startPos = transform.position;
                endPoint = new Point((int)player.transform.position.x, (int)player.transform.position.y);
                Thread genThread = new Thread(new ThreadStart(GeneratePath));
                genThread.Start();
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, new Vector2(path[currentPointIndex].x, path[currentPointIndex].y)) < 0.2f)
            {
                currentPointIndex++;
                if (currentPointIndex >= path.Count)
                {
                    path = null;
                    currentPointIndex = 0;
                }
            }
            else
            {
                Vector2 pos = Vector2.MoveTowards(transform.position, new Vector2(path[currentPointIndex].x, path[currentPointIndex].y), moveSpeed * Time.deltaTime);
                Vector2 direction = (Vector2)transform.position - new Vector2(path[currentPointIndex].x, path[currentPointIndex].y);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.position = new Vector3(pos.x, pos.y, -1);
                transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);

            }
        }
    }

    void GeneratePath()
    {
        generatingPath = true;
        path = pathFinder.findPath(new Point((int)startPos.x, (int)startPos.y), endPoint, GetPathingValueFunc);
        //path = pathFinder.findPath(new Point(0, 0), new Point(5,5), GetPathingValueFunc);
        gotPatrolPath = path.Count > 1;
        currentPointIndex = 0;
        generatingPath = false;
    }

    void Patrol()
    {
        if(!gotPatrolPath)
        {
            if(!generatingPath)
            {
                startPos = transform.position;
                endPoint = getRandomPoint();
                Thread genThread = new Thread(new ThreadStart(GeneratePath));
                genThread.Start();
            }
        }
      else
        {
            
            if (Vector2.Distance(transform.position, new Vector2(path[currentPointIndex].x, path[currentPointIndex].y)) < 0.2f)
            {
                currentPointIndex++;
                if (currentPointIndex >= path.Count)
                    gotPatrolPath = false;
            }
            else
            {
                Vector2 pos = Vector2.MoveTowards(transform.position, new Vector2(path[currentPointIndex].x, path[currentPointIndex].y), moveSpeed * Time.deltaTime);
                Vector2 direction = (Vector2)transform.position - new Vector2(path[currentPointIndex].x, path[currentPointIndex].y);
                float angle = Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;
                transform.position = new Vector3(pos.x, pos.y, -1);
                transform.rotation = Quaternion.AngleAxis(angle+90, Vector3.forward);

            }
        }
    }

    private Point getRandomPoint()
    {
        int index = Random.Range(0, Globals.rooms.Count - 1);
        Room room = Globals.rooms[index];
        return new Point(room.x + room.width/2, room.y + room.height/2);
    }


    private int GetPathingValue(int tileValue,Direction direction)
    {
        if (tileValue != 2)
            return -1;

        if (direction == Direction.Diagonal)
            return 2;
        return 1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Bullet")
        {
            GameObject obj = (GameObject)Instantiate(bloodPrefab);
            Vector3 pos = transform.position;
            pos.z = -0.1f;
            obj.transform.position = pos;
            obj.transform.LookAt(transform);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.red;
            foreach (Point point in path)
            {
                Gizmos.DrawCube(new Vector3(point.x, point.y, -1), new Vector3(1, 1, 1));
            }
        }
    }
}
