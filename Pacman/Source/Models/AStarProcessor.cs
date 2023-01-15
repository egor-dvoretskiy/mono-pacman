using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class AStarProcessor
    {
        private readonly int[,] _map;

        public AStarProcessor(int[,] map)
        {
            _map = map;
        }

        public LinkedList<NodeAStar> FindScatterPath((int, int) target, (int, int) position)
        {
            LinkedList<NodeAStar> waypointsLinked = new LinkedList<NodeAStar>();
            waypointsLinked.AddLast(new NodeAStar()
            {
                Position = position,
                EuristicApproach = CalculateEuristicApproach(position, target),
                StepApproach = 0
            });

            List<NodeAStar> closedNodes = AssignCloseNodes(waypointsLinked.Last.Value.Position, target); // add ghost-spawn to closed nodes;
            //
            // infinite loop when target if no opened nodes assigned.
            int stepAmount = 0;
            while (waypointsLinked.Last.Value.Position != target)
            {
                List<NodeAStar> openedNodes = AssignOpenNodes(waypointsLinked.Last.Value.Position, target, closedNodes);
                AddNodeToClosed(waypointsLinked.Last.Value, ref closedNodes, ref openedNodes);

                if (openedNodes.Count == 0)
                    closedNodes.RemoveAt(0);

                while (openedNodes.Count > 0)
                {
                    var node = openedNodes.Find(x => x.Weight == openedNodes.Select(y => y.Weight).Min());

                    if (IsStuck(node, closedNodes))
                    {
                        AddNodeToClosed(node, ref closedNodes, ref openedNodes);
                        continue;
                    }

                    waypointsLinked.AddLast(node);
                    break;
                }
                stepAmount++;
            }

            if (waypointsLinked.Count < 2)
                return null;

            return waypointsLinked;
        }

        private void CheckHooks(ref List<NodeAStar> nodes)
        {

        }

        private void AddNodeToClosed(NodeAStar node, ref List<NodeAStar> closedNodes, ref List<NodeAStar> openedNodes)
        {
            if (openedNodes.Contains(node))
                openedNodes.Remove(node);

            if (!closedNodes.Contains(node))
                closedNodes.Add(node);
        }

        private List<NodeAStar> AssignCloseNodes((int, int) position, (int, int) target)
        {
            return new List<NodeAStar>() { 
                new NodeAStar() {
                    Position = position,
                    EuristicApproach = CalculateEuristicApproach(position, target),
                    StepApproach = 0
                } 
            };
        }

        private List<NodeAStar> AssignOpenNodes((int, int) position, (int, int) target, List<NodeAStar> closedNodes)
        {
            List<NodeAStar> values = new List<NodeAStar>();
            // replace by Node (pos, weight)

            var moveLeft = (position.Item1 - 1, position.Item2);
            if (IsBoundsKeeps(moveLeft) && 
                _map[moveLeft.Item1, moveLeft.Item2] != 1 &&
                !IsClosedNode(moveLeft, closedNodes))
            {
                values.Add(
                    new NodeAStar()
                    {
                        Position = moveLeft,
                        EuristicApproach = CalculateEuristicApproach(moveLeft, target),
                        StepApproach = 1
                    }
                );
            }

            var moveTop = (position.Item1, position.Item2 - 1);
            if (IsBoundsKeeps(moveTop) && 
                _map[moveTop.Item1, moveTop.Item2] != 1 &&
                !IsClosedNode(moveTop, closedNodes))
            {
                values.Add(
                    new NodeAStar()
                    {
                        Position = moveTop,
                        EuristicApproach = CalculateEuristicApproach(moveTop, target),
                        StepApproach = 1
                    }
                );
            }

            var moveRight = (position.Item1 + 1, position.Item2);
            if (IsBoundsKeeps(moveRight) && 
                _map[moveRight.Item1, moveRight.Item2] != 1 &&
                !IsClosedNode(moveRight, closedNodes))
            {
                values.Add(
                    new NodeAStar()
                    {
                        Position = moveRight,
                        EuristicApproach = CalculateEuristicApproach(moveRight, target),
                        StepApproach = 1
                    }
                );
            }

            var moveBottom = (position.Item1, position.Item2 + 1);
            if (IsBoundsKeeps(moveBottom) && 
                _map[moveBottom.Item1, moveBottom.Item2] != 1 &&
                !IsClosedNode(moveBottom, closedNodes))
            {
                values.Add(
                    new NodeAStar()
                    {
                        Position = moveBottom,
                        EuristicApproach = CalculateEuristicApproach(moveBottom, target),
                        StepApproach = 1
                    }
                );
            }

            return values;
        }

        private bool IsStuck(NodeAStar node, List<NodeAStar> closedNodes)
        {
            var nodePosition = node.Position;
            bool result = true;

            var moveLeft = (nodePosition.Item1 - 1, nodePosition.Item2);
            if (IsBoundsKeeps(moveLeft) &&
                _map[moveLeft.Item1, moveLeft.Item2] != 1 &&
                !IsClosedNode(moveLeft, closedNodes))
            {
                result &= false;
            }

            var moveTop = (nodePosition.Item1, nodePosition.Item2 - 1);
            if (IsBoundsKeeps(moveTop) &&
                _map[moveTop.Item1, moveTop.Item2] != 1 &&
                !IsClosedNode(moveTop, closedNodes))
            {
                result &= false;
            }

            var moveRight = (nodePosition.Item1 + 1, nodePosition.Item2);
            if (IsBoundsKeeps(moveRight) &&
                _map[moveRight.Item1, moveRight.Item2] != 1 &&
                !IsClosedNode(moveRight, closedNodes))
            {
                result &= false;
            }

            var moveBottom = (nodePosition.Item1, nodePosition.Item2 + 1);
            if (IsBoundsKeeps(moveBottom) &&
                _map[moveBottom.Item1, moveBottom.Item2] != 1 &&
                !IsClosedNode(moveBottom, closedNodes))
            {
                result &= false;
            }

            return result;
        }

        private bool IsBoundsKeeps((int,int) move)
        {
            return
                move.Item1 >= 0 &&
                move.Item1 < _map.GetLength(0) &&
                move.Item2 >= 0 &&
                move.Item2 < _map.GetLength(1);
        }

        private bool IsClosedNode((int, int) movePosition, List<NodeAStar> closedNodes)
        {
            return closedNodes.Any(x => x.Position == movePosition);
        }

        private int CalculateEuristicApproach((int, int) currentPosition, (int, int) targetPosition)
        {
            int euristicApproach = 0;

            (int, int) resultPosition = currentPosition;

            int item1Increment = Math.Sign(targetPosition.Item1 - currentPosition.Item1);
            while (resultPosition.Item1 != targetPosition.Item1)
            {
                resultPosition.Item1 += item1Increment;
                euristicApproach++;
            }

            int item2Increment = Math.Sign(targetPosition.Item2 - currentPosition.Item2);
            while (resultPosition.Item2 != targetPosition.Item2)
            {
                resultPosition.Item2 += item2Increment;
                euristicApproach++;
            }

            return euristicApproach;
        }
    }
}
