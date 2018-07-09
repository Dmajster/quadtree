using UnityEngine;

public class UnityQuadtree : Quadtree
{
    public Vector2 Position;
    public Vector2 Size;

    public UnityQuadtree(Vector2 position, Vector2 size) : base()
    {
        Position = position;
        Size = size;
    }

    public UnityQuadtree(UnityQuadtree parent, Quadrant quadrant) : base(parent, quadrant)
    {
        Size = parent.Size / 2;
        Position = parent.Position + new Vector2Int((int) quadrant>>1 & 1, (int) quadrant & 1) * Size;
    }

    public override void Expand()
    {
        Children = new[]
        {
            new UnityQuadtree(this, Quadrant.SouthWest),
            new UnityQuadtree(this, Quadrant.NorthWest),
            new UnityQuadtree(this, Quadrant.SouthEast),
            new UnityQuadtree(this, Quadrant.NorthEast)
        };
    }
}