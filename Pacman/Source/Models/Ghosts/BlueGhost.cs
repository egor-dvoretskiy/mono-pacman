using Autofac.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using Pacman.Source.Abstract;
using Pacman.Source.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models.Ghosts
{
    public class BlueGhost : Ghost
    {
        private readonly Map _map;
        private readonly AStarProcessor _astarProcessor;
        private readonly (int, int) _scatterPosition;

        private string currentAnimation;
        private (int, int) _stepScatterPosition;
        private (int, int) _patrolScatterPosition;

        private LinkedList<NodeAStar> scatterPath;
        private LinkedListNode<NodeAStar> currentLinkedNode;

        public BlueGhost(
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
                  transitions)

        {
            Name = "Bashful";
            _map = map; 
            _astarProcessor = new AStarProcessor(map.MatrixMap);
            _scatterPosition = ((int)scatterPosition.X / map.TiledMap.TileWidth, (int)scatterPosition.Y / map.TiledMap.TileHeight);
            _stepScatterPosition = PositionMatrix;

            currentAnimation = _animationNames.Up;
        }

        public override string Name { get; init; }

        public Vector2 PositionOriginOffset
        {
            get => new Vector2(Position.X - _animation.Origin.X, Position.Y - _animation.Origin.Y);
        }

        public (int, int) PositionMatrix
        {
            get => ((int)Position.X / Width, (int)Position.Y / Height);
        }

        public Vector2 PatrolScatterPosition
        {
            get => new Vector2(_patrolScatterPosition.Item1 * _map.TiledMap.TileWidth, _patrolScatterPosition.Item2 * _map.TiledMap.TileHeight);
        }

        public Vector2 StepScatterPosition
        {
            get => new Vector2(_stepScatterPosition.Item1 * _map.TiledMap.TileWidth, _stepScatterPosition.Item2 * _map.TiledMap.TileHeight);
        }

        public Vector2 ScatterPosition
        {
            get => new Vector2(_scatterPosition.Item1 * _map.TiledMap.TileWidth, _scatterPosition.Item2 * _map.TiledMap.TileHeight);
        }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Move();

            AssignAnimation();
            _animation.Play(currentAnimation);
            _animation.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation, Position, 0);
        }

        protected override void Move()
        {
            switch (GhostPhase)
            {
                case Enum.GhostPhase.Scatter:
                    {
                        if (IsPositionAtTarget() || IsPositionAtPatrolScatterPosition())
                        {
                            ghostMode = GhostMode.Patrol;

                            scatterPath = null;
                            currentLinkedNode = null;
                            SetPatrolPosition();
                        }

                        if (scatterPath is null)
                        {
                            scatterPath = _astarProcessor.FindScatterPath(ghostMode == GhostMode.Patrol ? _patrolScatterPosition : _scatterPosition, PositionMatrix);

                            if (scatterPath != null)
                                currentLinkedNode = scatterPath.First;
                        }

                        if (IsPositionFitsStepScatterPosition())
                        {
                            if (currentLinkedNode != null)
                                currentLinkedNode = currentLinkedNode.Next;

                            _stepScatterPosition = currentLinkedNode is null ? PositionMatrix : currentLinkedNode.Value.Position;
                        }

                        Vector2 step = new Vector2(
                            Math.Sign(StepScatterPosition.X - PositionOriginOffset.X) * Velocity.X,
                            Math.Sign(StepScatterPosition.Y - PositionOriginOffset.Y) * Velocity.Y
                        );

                        Position += step;
                        AssignDirection(step);
                    }
                    break;
                case Enum.GhostPhase.Chase:
                    {

                    }
                    break;
                case Enum.GhostPhase.Frightened:
                    {

                    }
                    break;
            }
        }

        private bool IsPositionFitsStepScatterPosition() =>
            PositionOriginOffset.X == StepScatterPosition.X &&
            PositionOriginOffset.Y == StepScatterPosition.Y;

        private bool IsPositionAtTarget() =>
            PositionOriginOffset.X == ScatterPosition.X &&
            PositionOriginOffset.Y == ScatterPosition.Y;

        private bool IsPositionAtPatrolScatterPosition() =>
            PositionOriginOffset.X == PatrolScatterPosition.X &&
            PositionOriginOffset.Y == PatrolScatterPosition.Y;

        private void AssignAnimation()
        {
            if (direction == Enum.Direction.Up)
            {
                currentAnimation = _animationNames.Up;
                return;
            }

            if (direction == Enum.Direction.Down)
            {
                currentAnimation = _animationNames.Down;
                return;
            }

            if (direction == (Direction.Left | Direction.None))
            {
                currentAnimation = _animationNames.Left;
                return;
            }

            if (direction == Enum.Direction.Right)
            {
                currentAnimation = _animationNames.Right;
                return;
            }
        }

        private void AssignDirection(Vector2 vector)
        {
            if (vector.X == 0 && vector.Y == 0)
            {
                direction = Direction.None;
                return;
            }

            if (vector.X > 0 && vector.Y == 0)
            {
                direction = Direction.Right;
                return;
            }

            if (vector.X < 0 && vector.Y == 0)
            {
                direction = Direction.Left;
                return;
            }

            if (vector.X == 0 && vector.Y > 0)
            {
                direction = Direction.Down;
                return;
            }

            if (vector.X == 0 && vector.Y < 0)
            {
                direction = Direction.Up;
                return;
            }
        }

        private void SetPatrolPosition()
        {
            (int, int) indexes = PositionMatrix;

            while (indexes == PositionMatrix)
            {
                indexes = _patrolZone.ElementAt(Random.Shared.Next(_patrolZone.Count()));
            }

            _patrolScatterPosition = indexes;
        }
    }
}
