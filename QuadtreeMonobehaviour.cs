using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class QuadtreeMonobehaviour : MonoBehaviour
{
    public UnityQuadtree unityQuadtree;
    public List<Quadtree> leafs = new List<Quadtree>();

    private IEnumerator Start()
    {
        unityQuadtree = new UnityQuadtree(Vector2.zero, new Vector2(10, 10))
        {
            MaxLevel = 3
        };

        unityQuadtree.Expand();
        unityQuadtree.Children[2].Expand();
        unityQuadtree.Children[3].Expand();
        unityQuadtree.Children[3].Children[1].Expand();

        yield return StartCoroutine(CalculateNeighbourDifferences());

        Debug.Log(unityQuadtree.Children[3].Children[1].Children[3].LocationCodeString + " <- " + Quadtree.BinaryToDecimal(unityQuadtree.Children[3].Children[1].Children[3].NeighbourLocationCode(Direction.West),
            unityQuadtree.MaxLevel));
    }

    public IEnumerator CalculateNeighbourDifferences()
    {
        leafs.Clear();
        List<Quadtree> processing = new List<Quadtree>()
        {
            unityQuadtree
        };

        while (processing.Count > 0)
        {
            var processedQuadtree = processing[0];
            processing.RemoveAt(0);

            yield return new WaitForSecondsRealtime(1);

            //If no children stop processing as it's not a "gray" node
            if (processedQuadtree.Children == null) continue;

            var equalNeighbourLocationCodes = processedQuadtree.EqualNeighbourLocationCodes();
            Debug.Log($"{{{processedQuadtree.Level} {processedQuadtree.LocationCodeString}}}");
            leafs.ForEach(comparedQuadtree =>
                Debug.Log($"\t{{{comparedQuadtree.Level} {comparedQuadtree.LocationCodeString}}}"));
            Debug.Log("");
            equalNeighbourLocationCodes.ForEach(equalNeighbourLocationCode =>
            {
                var equalNeighbour = leafs.FirstOrDefault(comparedQuadtree =>
                    comparedQuadtree.LocationCode == equalNeighbourLocationCode.Item2 &&
                    processedQuadtree.Level == comparedQuadtree.Level);

                if (equalNeighbour == null) return;

                equalNeighbour.LevelDifference[equalNeighbourLocationCode.Item1.Opposite()] += 1;
            });

            //Remove parent from leafs and add children to end of list to comform with Aizawa's algorithm
            leafs.Remove(processedQuadtree);

            processedQuadtree.Children[0].LevelDifference[Direction.West] =
                processedQuadtree.LevelDifference[Direction.West] - 1;
            processedQuadtree.Children[0].LevelDifference[Direction.South] =
                processedQuadtree.LevelDifference[Direction.South] - 1;

            processedQuadtree.Children[1].LevelDifference[Direction.North] =
                processedQuadtree.LevelDifference[Direction.North] - 1;
            processedQuadtree.Children[1].LevelDifference[Direction.West] =
                processedQuadtree.LevelDifference[Direction.West] - 1;

            processedQuadtree.Children[2].LevelDifference[Direction.South] =
                processedQuadtree.LevelDifference[Direction.South] - 1;
            processedQuadtree.Children[2].LevelDifference[Direction.East] =
                processedQuadtree.LevelDifference[Direction.East] - 1;

            processedQuadtree.Children[3].LevelDifference[Direction.North] =
                processedQuadtree.LevelDifference[Direction.North] - 1;
            processedQuadtree.Children[3].LevelDifference[Direction.East] =
                processedQuadtree.LevelDifference[Direction.East] - 1;


            foreach (var processedQuadtreeChild in processedQuadtree.Children)
            {
                leafs.Add(processedQuadtreeChild);
                processing.Add(processedQuadtreeChild);

                equalNeighbourLocationCodes = processedQuadtreeChild.EqualNeighbourLocationCodes();
                equalNeighbourLocationCodes.ForEach(equalNeighbourLocationCode =>
                {
                    var equalNeighbour = leafs.FirstOrDefault(comparedQuadtree =>
                        comparedQuadtree.LocationCode == equalNeighbourLocationCode.Item2 &&
                        processedQuadtreeChild.Level == comparedQuadtree.Level &&
                        processedQuadtree.Children.All(comparedChildrenQuadtree =>
                            comparedChildrenQuadtree != comparedQuadtree)
                    );

                    if (equalNeighbour == null) return;

                    equalNeighbour.LevelDifference[equalNeighbourLocationCode.Item1.Opposite()] += 1;
                });
            }
        }
    }
}