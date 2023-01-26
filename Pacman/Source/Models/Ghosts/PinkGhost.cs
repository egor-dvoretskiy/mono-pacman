using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using Pacman.Source.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models.Ghosts
{
    public class PinkGhost : Ghost
    {
        private readonly int _chaseRadius = 4;

        public PinkGhost(
            Texture2D texture,
            Vector2 position,
            AnimatedSprite animatedSprite,
            GhostAnimationCall animationNames,
            Vector2 velocity,
            Map map,
            IEnumerable<(int, int)> patrolZone,
            Vector2 scatterPosition,
            Transitions transitions)
            : base(
                  texture,
                  position,
                  animatedSprite,
                  animationNames,
                  velocity,
                  map,
                  patrolZone,
                  transitions,
                  scatterPosition)
        {
            /// Add to matrix map more numbers to avoid visiting transition tiles by ghosts.

            Name = "Pinky";
        }

        protected override void MoveChase()
        {
            if (IsPositionAtStepChasePosition())
            {
                astarPath = null;
                currentLinkedNode = null;
            }

            if (astarPath is null)
            {
                astarPath = _astarProcessor.FindPath(GetChaseTarget(), PositionMatrix);

                if (astarPath != null)
                    currentLinkedNode = astarPath.First;
            }

            if (IsPositionAtStepChasePosition())
            {
                if (currentLinkedNode != null)
                    currentLinkedNode = currentLinkedNode.Next;

                _stepChasePosition = currentLinkedNode is null ? PositionMatrix : currentLinkedNode.Value.Position;
            }

            Vector2 step = new Vector2(
                Math.Sign(StepChasePosition.X - PositionOriginOffset.X) * Velocity.X,
                Math.Sign(StepChasePosition.Y - PositionOriginOffset.Y) * Velocity.Y
            );

            Position += step;
            AssignDirection(step);
        }

        private (int, int) GetChaseTarget()
        {
            var chaseZone = CalculateChaseZone(latestPlayerPosition);

            return chaseZone.Count() == 0 ? _scatterPosition : chaseZone.ElementAt(Random.Shared.Next(chaseZone.Count()));
        }

        private IEnumerable<(int, int)> CalculateChaseZone(Vector2 referencePoint)
        {
            List<(int, int)> patrolZone = new List<(int, int)>();

            var iborders = CalculateBorders((int)referencePoint.X / _map.TiledMap.TileWidth, 0, _map.MatrixMap.GetLength(0));
            var jborders = CalculateBorders((int)referencePoint.Y / _map.TiledMap.TileHeight, 0, _map.MatrixMap.GetLength(1));

            for (int i = iborders.Item1; i < iborders.Item2; i++)
            {
                for (int j = jborders.Item1; j < jborders.Item2; j++)
                {
                    if (_map.MatrixMap[i, j] == 0)
                        patrolZone.Add((i, j));
                }
            }

            return patrolZone;
        }

        private (int, int) CalculateBorders(int pure, int limitX, int limitY)
        {
            int bottomBorder = pure - _chaseRadius < limitX ? limitX : pure - _chaseRadius;
            int upperBorder = pure + _chaseRadius > limitY ? limitY - 1 : pure + _chaseRadius;

            return (bottomBorder, upperBorder);
        }
    }
}
