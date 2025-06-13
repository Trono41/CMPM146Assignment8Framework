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
        iterations = 0;
        try
        {
            GenerateWithBacktracking(occupied, doors, 1);
        }
        catch (System.Exception e)
        {
            Generate();
            return;
        }
    }


    bool GenerateWithBacktracking(List<Vector2Int> occupied, List<Door> doors, int depth)
    {
        if (iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");

        iterations++;
        if (doors.Count == 0)  return depth >= 5;
        int doorIndex = Random.Range(0, doors.Count);
        Door currentDoor = doors[doorIndex];
        doors.RemoveAt(doorIndex);
        Door toConnect = currentDoor.GetMatching();
        Vector2Int placeGrid = toConnect.GetGridCoordinates();
        List<Room> candidates = new List<Room>();

        foreach (var room in rooms)
        {
            if (room.HasDoorOnSide(toConnect.GetDirection())) candidates.Add(room);
        }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = candidates[i]; candidates[i] = candidates[j]; candidates[j] = tmp;
        }

        foreach (var roomPrefab in candidates)
        {
            var cells = roomPrefab.GetGridCoordinates(placeGrid);
            bool overlap = false;
            foreach (var c in cells)
            {
                if (occupied.Contains(c)) { overlap = true; break; }
            }

            if (overlap) continue;

            var newDoors = roomPrefab.GetDoors(placeGrid);
            var addedDoors = new List<Door>();

            foreach (var d in newDoors)
                if (!d.IsMatching(currentDoor)) addedDoors.Add(d);

            occupied.AddRange(cells);
            doors.AddRange(addedDoors);

            if (GenerateWithBacktracking(occupied, doors, depth + 1))
            {
                var hallway = toConnect.IsHorizontal() ? horizontal_hallway : vertical_hallway;
                generated_objects.Add(hallway.Place(toConnect));
                generated_objects.Add(roomPrefab.Place(placeGrid));
                return true;
            }

            foreach (var d in addedDoors)
                doors.Remove(d);
            foreach (var c in cells)
                occupied.Remove(c);
        }

        doors.Insert(doorIndex, currentDoor);
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
}