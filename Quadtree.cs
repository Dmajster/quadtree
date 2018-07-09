using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Quadtree
{
    public int LocationCode;

    public string LocationCodeString => BinaryToDecimal(LocationCode, MaxLevel).ToString();

    public Quadtree Parent;
    public Quadtree[] Children;
    public Quadtree Root => Parent?.Root ?? this;

    public int MaxLevel = 5;
    public int Level = 0;

    public Dictionary<Direction, int> LevelDifference = new Dictionary<Direction, int>()
    {
        {Direction.North, 0},
        {Direction.East, 0},
        {Direction.South, 0},
        {Direction.West, 0}
    };

    public Quadtree()
    {
    }

    public Quadtree(Quadtree parent, Quadrant quadrant)
    {
        Parent = parent;
        Level = parent.Level + 1;
        MaxLevel = parent.MaxLevel;
        LocationCode = Binary(this, quadrant);
    }

    public Quadtree Find(int locationCode)
    {
        if (locationCode == LocationCode) return this;
        if (Children == null) return null;

        int index = locationCode;
        index >>= (MaxLevel - Level - 1) * 2;

        return Children[index & 3].Find(locationCode);
    }

    public Quadtree FindLeaf(int locationCode)
    {
        if (Children == null) return locationCode == LocationCode ? this : null;

        int index = locationCode;
        index >>= (MaxLevel - Level - 1) * 2;

        return Children[index & 3].FindLeaf(locationCode);
    }

    public virtual void Expand()
    {
        Children = new[]
        {
            new Quadtree(this, Quadrant.SouthWest),
            new Quadtree(this, Quadrant.NorthWest),
            new Quadtree(this, Quadrant.SouthEast),
            new Quadtree(this, Quadrant.NorthEast)
        };
    }

    public List<(Direction, int)> EqualNeighbourLocationCodes()
    {
        var directions = new[]
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
        };

        var equalNeighbourLocationCodes = new List<(Direction, int)>();

        foreach (var direction in directions)
        {
            bool overflow;
            var neighbourLocationCode = EqualNeighbourLocationCode(direction.Value, out overflow);

            if (overflow) continue;

            equalNeighbourLocationCodes.Add((direction, neighbourLocationCode));
        }

        return equalNeighbourLocationCodes;
    }


    public static List<(Direction, Quadtree)> EqualNeighbours(Quadtree quadtree)
    {
        var directions = new[]
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
        };

        var equalNeighbours = new List<(Direction, Quadtree)>();

        foreach (var direction in directions)
        {
            bool overflow;
            var neighbourLocationCode = quadtree.EqualNeighbourLocationCode(direction.Value, out overflow);

            if (overflow) continue;

            var neighbour = quadtree.Root.Find(neighbourLocationCode);

            if (neighbour == null || neighbour.Level != quadtree.Level) continue;

            equalNeighbours.Add((direction, neighbour));
        }

        return equalNeighbours;
    }

    public void BreadthFirst(Action<Quadtree> action)
    {
        Queue<Quadtree> queue = new Queue<Quadtree>();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            var checkedQuadtree = queue.Dequeue();

            action(checkedQuadtree);

            if (checkedQuadtree.Children == null) continue;
            foreach (var child in checkedQuadtree.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    public void DepthFirst(Action<Quadtree> action)
    {
        if (Children != null)
        {
            foreach (var child in Children)
            {
                child.DepthFirst(action);
            }
        }

        action(this);
    }

    public int EqualNeighbourLocationCode(Vector2Int direction, out bool overflow)
    {
        var neighbourLocationCode = EqualNeighbourLocationCode(direction);

        overflow = false;

        if (direction.x > 0 && X(neighbourLocationCode, MaxLevel) <= X(LocationCode, MaxLevel) ||
            direction.x < 0 && X(neighbourLocationCode, MaxLevel) >= X(LocationCode, MaxLevel) ||
            direction.y > 0 && Y(neighbourLocationCode, MaxLevel) <= Y(LocationCode, MaxLevel) ||
            direction.y < 0 && Y(neighbourLocationCode, MaxLevel) >= Y(LocationCode, MaxLevel)
        )
        {
            overflow = true;
        }

        return neighbourLocationCode;
    }

    public int EqualNeighbourLocationCode(Vector2Int direction)
    {
        int tx = TranslationX(MaxLevel);
        int ty = TranslationY(MaxLevel);
        int translationIncrement;

        if (Level < MaxLevel)
        {
            translationIncrement = TranslationIncrement(direction, MaxLevel) << 2 * (MaxLevel - Level);
        }
        else
        {
            translationIncrement = TranslationIncrement(direction, MaxLevel);
        }

        return (((LocationCode | ty) + (translationIncrement & tx)) & tx) |
               (((LocationCode | tx) + (translationIncrement & ty)) & ty);
    }

    public int NeighbourLocationCode(Direction direction)
    {
        if (LevelDifference[direction] < 0)
        {
            return LocationAdditionOperator(
                (LocationCode >> 2 * (MaxLevel - Level - LevelDifference[direction])) << 2 * (MaxLevel - Level - LevelDifference[direction]),
                (TranslationIncrement(direction.Value, MaxLevel) << (2 * (MaxLevel - Level - LevelDifference[direction])))
            );
        }

        return LocationAdditionOperator(LocationCode,
            TranslationIncrement(direction.Value, MaxLevel) << (2 * (MaxLevel - Level))
        );
    }

    public int LocationAdditionOperator(int value1, int value2)
    {
        int translationX = TranslationX(MaxLevel);
        int translationY = TranslationY(MaxLevel);

        return (((value1 | translationY) + (value2 & translationX)) & translationX) |
               (((value1 | translationX) + (value2 & translationY)) & translationY);
    }

    private static int Decimal(Quadtree quadtree, Quadrant quadrant)
    {
        return (int) (quadtree.Parent?.LocationCode +
                      Mathf.Pow(10, quadtree.MaxLevel - quadtree.Level) * (int) quadrant ?? 0);
    }

    private static int Binary(Quadtree quadtree, Quadrant quadrant)
    {
        return (quadtree.Parent?.LocationCode | ((int) quadrant << (quadtree.MaxLevel - quadtree.Level) * 2)) ?? 0;
    }

    private static int DecimalToBinary(int decimalValue, int maxLevel)
    {
        int binaryValue = 0;
        for (; maxLevel > 0; maxLevel--)
        {
            binaryValue <<= 2;
            binaryValue |= GetDecimalDigit(decimalValue, maxLevel);
        }

        return binaryValue;
    }

    public static int BinaryToDecimal(int binaryValue, int maxLevel)
    {
        int decimalValue = 0;
        for (int level = 0; level < maxLevel; level++)
        {
            decimalValue += (int) (Mathf.Pow(10, level) * (binaryValue & 3));
            binaryValue >>= 2;
        }

        return decimalValue;
    }

    public static string IntegerToBinaryString(int theNumber, int minimumDigits)
    {
        return Convert.ToString(theNumber, 2).PadLeft(minimumDigits, '0');
    }

    private static int TranslationIncrement(Vector2Int direction, int maxLevel)
    {
        int value = 0;
        for (int level = 0; level < maxLevel; level++)
        {
            value |= (direction.y & 1) << (level * 2);
            direction.y >>= 1;

            value |= (direction.x & 1) << (level * 2 + 1);
            direction.x >>= 1;
        }

        return value;
    }

    public static int X(int locationCode, int maxLevel)
    {
        int value = 0;

        locationCode >>= 1;
        for (int i = 0; i < 16; i++)
        {
            value |= (locationCode & 1) << i;
            locationCode >>= 2;
        }

        return value;
    }

    public static int Y(int locationCode, int maxLevel)
    {
        int value = 0;

        for (int i = 0; i < maxLevel; i++)
        {
            value |= (locationCode & 1) << i;
            locationCode >>= 2;
        }

        return value;
    }


    private static int GetDecimalDigit(int number, int index)
    {
        return (int) ((number / Math.Pow(10, index - 1)) % 10);
    }

    private static int TranslationX(int maxLevel)
    {
        int tx = 0;
        for (; maxLevel > 0; maxLevel--)
        {
            tx <<= 2;
            tx |= 1;
        }

        return tx;
    }

    private static int TranslationY(int maxLevel)
    {
        int ty = 0;
        for (; maxLevel > 0; maxLevel--)
        {
            ty <<= 2;
            ty |= 2;
        }

        return ty;
    }
}