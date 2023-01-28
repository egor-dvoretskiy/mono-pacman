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
    public class OrangeGhost : Ghost
    {
        private readonly int _pacmanBubbleSize = 8;
        public OrangeGhost(
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

            Name = "Clyde";
        }

        protected override void MoveChase()
        {
            var distanceToPlayer = CalculateEuristicDistance(latestPlayerPosition, Position) / _map.TiledMap.TileHeight;

            if (distanceToPlayer > _pacmanBubbleSize)
            {
                if (IsPositionAtStepChasePosition())
                {
                    astarPath = null;
                    currentLinkedNode = null;
                }

                if (astarPath is null)
                {
                    astarPath = _astarProcessor.FindPath(LatestPlayerPositionMatrix, PositionMatrix, PureLatestPosition);

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

                return;
            }
            //ResetMovement();
            MoveScatter();
        }
    }
}
