using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Direction
{
    private static readonly List<Direction> Directions = new List<Direction>();
        
    public static Direction North = new Direction(new Vector2Int(0,1));
    public static Direction NorthEast = new Direction(new Vector2Int(1,1));
    public static Direction East = new Direction(new Vector2Int(1,0));
    public static Direction SouthEast = new Direction(new Vector2Int(1,-1));
    public static Direction South = new Direction(new Vector2Int(0,-1));
    public static Direction SouthWest = new Direction(new Vector2Int(-1,-1));
    public static Direction West = new Direction(new Vector2Int(-1,0));
    public static Direction NorthWest = new Direction(new Vector2Int(-1,1));

    private Direction(Vector2Int value)
    {
        Value = value;
        Directions.Add(this);
    }
        
    public Vector2Int Value;

    public Direction Opposite()
    {
        var oppositeValue = Value * -1;
        //Debug.Log($"{oppositeValue} -> {Directions.FirstOrDefault(checkedDirection => checkedDirection.Value == oppositeValue).Value}");
        return Directions.FirstOrDefault(checkedDirection => checkedDirection.Value == oppositeValue);
    }
}