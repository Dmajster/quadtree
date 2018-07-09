using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuadtreeMonobehaviour))]
public class QuadtreeInspector : Editor
{
    private QuadtreeMonobehaviour _target;

    private void OnEnable()
    {
        _target = (QuadtreeMonobehaviour) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        if (_target != null)
        {
            _target.leafs.ForEach(quadtree =>
            {
                var unityQuadtree = (UnityQuadtree) quadtree;
                var unityRoot = unityQuadtree.Root as UnityQuadtree;
                var offset = unityRoot.Size * unityQuadtree.Level * new Vector2(1, 0);
                //var center = offset + unityQuadtree.Position + unityQuadtree.Size / 2;

                var center = unityQuadtree.Position + unityQuadtree.Size / 2;
                
                //if (quadtree.Children != null) return;

                Handles.DrawWireCube(center, unityQuadtree.Size);

                Handles.Label(center, quadtree.LocationCodeString);

                Handles.Label(
                    center + new Vector2(1, 0) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.East].ToString());
                Handles.Label(
                    center + new Vector2(0, 1) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.North].ToString());
                Handles.Label(
                    center + new Vector2(-1, 0) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.West].ToString());
                Handles.Label(
                    center + new Vector2(0, -1) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.South].ToString());
            });
            
            /*
            _target.unityQuadtree?.DepthFirst((quadtree) =>
            {
                var unityQuadtree = (UnityQuadtree) quadtree;
                var unityRoot = unityQuadtree.Root as UnityQuadtree;
                var offset = unityRoot.Size * unityQuadtree.Level * new Vector2(1, 0);
                //var center = offset + unityQuadtree.Position + unityQuadtree.Size / 2;

                var center = unityQuadtree.Position + unityQuadtree.Size / 2;
                if (quadtree.Children != null) return;

                Handles.DrawWireCube(center, unityQuadtree.Size);

                Handles.Label(center, quadtree.LocationCodeString);

                Handles.Label(
                    center + new Vector2(1, 0) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.East].ToString());
                Handles.Label(
                    center + new Vector2(0, 1) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.North].ToString());
                Handles.Label(
                    center + new Vector2(-1, 0) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.West].ToString());
                Handles.Label(
                    center + new Vector2(0, -1) * unityQuadtree.Size / 4, unityQuadtree.LevelDifference[Direction.South].ToString());
            });
            */
        }
    }
}