using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Process((int, int) target, (int, int) position)
        {
            List<(int, int)> closedNodes = AssignCloseNodes(position); // add ghost-spawn to closed nodes;
            List<(int, int)> openedNodes = AssignOpenNodes(position);

        }

        private List<(int, int)> AssignCloseNodes((int, int) position)
        {
            return new List<(int, int)>() { position };
        }

        private List<(int, int)> AssignOpenNodes((int, int) position)
        {
            List<(int, int)> values = new List<(int, int)> ();
            // replace by Node (pos, weight)

            var moveLeft = (position.Item1--, position.Item2);
            if (IsBoundsKeeps(moveLeft) && 
                _map[moveLeft.Item1, moveLeft.Item2] != 1)
            {
                values.Add(moveLeft);
            }

            var moveTop = (position.Item1, position.Item2--);
            if (IsBoundsKeeps(moveTop) && 
                _map[moveTop.Item1, moveTop.Item2] != 1)
            {
                values.Add(moveTop);
            }

            var moveRight = (position.Item1++, position.Item2);
            if (IsBoundsKeeps(moveRight) && 
                _map[moveRight.Item1, moveRight.Item2] != 1)
            {
                values.Add(moveRight);
            }

            var moveBottom = (position.Item1, position.Item2++);
            if (IsBoundsKeeps(moveBottom) && 
                _map[moveBottom.Item1, moveBottom.Item2] != 1)
            {
                values.Add(moveBottom);
            }

            return values;
        }

        private bool IsBoundsKeeps((int,int) move)
        {
            return
                move.Item1 >= 0 &&
                move.Item1 < _map.GetLength(0) &&
                move.Item2 >= 0 &&
                move.Item2 < _map.GetLength(1);
        }
    }
}
