using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    public List<Room> rooms;
    public Hallway vertical_hallway;
    public Hallway horizontal_hallway;
    public Room start;
    public Room target;

    // Constraint: How big should the dungeon be at most
    // this will limit the run time (~10 is a good value 
    // during development, later you'll want to set it to 
    // something a bit higher, like 25-30)
    public int MAX_SIZE = 3;

    // set this to a high value when the generator works
    // for debugging it can be helpful to test with few rooms
    // and, say, a threshold of 100 iterations
    public int THRESHOLD = 100;

    // keep the instantiated rooms and hallways here 
    private List<GameObject> generated_objects;
    
    int iterations;

    public void Generate()
    {
        // dispose of game objects from previous generation process
        foreach (var go in generated_objects)
        {
            Destroy(go);
        }
        generated_objects.Clear();
        
        generated_objects.Add(start.Place(new Vector2Int(0,0)));
        List<Door> doors = start.GetDoors();
        List<Vector2Int> occupied = new List<Vector2Int>();
        occupied.Add(new Vector2Int(0, 0));
        iterations = 0;
        GenerateWithBacktracking(occupied, doors, 1);
    }


    bool GenerateWithBacktracking(List<Vector2Int> occupied, List<Door> doors, int depth)
    {
        if (iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");
        // generated_objects.Add(rooms[4].Place(new Vector2Int(1,0)));
        List<Room> matching_doors = new List<Room>();
        foreach (Room room in rooms)
        {
            if (room.HasDoorOnSide(Door.Direction.WEST))
            {
                matching_doors.Add(room);
            }
        }
        int roomIndex = generateIndex(matching_doors.Count);
        if (generated_objects.Count < 2)
        {
            List<Door> doorList = matching_doors[roomIndex].GetDoors();
            if (doorList.Count <= 1)
            {
                roomIndex = generateIndex(matching_doors.Count);
            }
        }
        generated_objects.Add(matching_doors[roomIndex].Place(new Vector2Int(1,0)));
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generated_objects = new List<GameObject>();
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
            Generate();
    }

    int generateIndex(int max_range)
    {
        return Random.Range(0, max_range);
    }
}
