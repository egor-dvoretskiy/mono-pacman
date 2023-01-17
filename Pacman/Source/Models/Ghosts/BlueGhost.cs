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
        private readonly TiledMapObjectLayer _restrictedWays;

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

            /// Add to matrix map more numbers to avoid visiting transition tiles by ghosts.

            Name = "Bashful";
            _map = map; 
            _astarProcessor = new AStarProcessor(map.MatrixMap);
            _scatterPosition = ((int)scatterPosition.X / map.TiledMap.TileWidth, (int)scatterPosition.Y / map.TiledMap.TileHeight);
            _stepScatterPosition = PositionMatrix;
            _restrictedWays = map.TiledMap.ObjectLayers.Single(x => x.Name.Equals("map-restricted"));

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
                        List<(Vector2, Vector2)> localWays = FoundPossibleFrightenedWays(); // (velocity, point)

                        if (localWays == null || localWays.Count == 0)
                            break;

                        (Vector2, Vector2) nextPointTuple = FoundFarestWayByEuristicSearch(localWays); // search for the next point

                        Position = nextPointTuple.Item2;
                        AssignDirection(nextPointTuple.Item1);
                    }
                    break;
            }
        }
        private (Vector2, Vector2) FoundFarestWayByEuristicSearch(List<(Vector2, Vector2)> localVelocities)
        {
            List<int> localDistances = new List<int> ();
            int farestWayIndex = 0;
            int biggestEuristicDistance = 0;
            for (int i = 0; i < localVelocities.Count; i++)
            {
                int currentEuristicApproach = CalculateEuristicDistance(latestPlayerPosition, localVelocities[i].Item2);
                if (currentEuristicApproach > biggestEuristicDistance)
                {
                    localDistances.Clear();

                    farestWayIndex = i;
                    biggestEuristicDistance = currentEuristicApproach;
                    localDistances.Add(i);
                    continue;
                }

                if (currentEuristicApproach == biggestEuristicDistance)
                    localDistances.Add(i);
            }

            return localVelocities[localDistances[Random.Shared.Next(localDistances.Count)]];
        }

        private int CalculateEuristicDistance(Vector2 point1, Vector2 point2)
        {
            var xapproach = Math.Abs(point2.X - point1.X);
            var yapproach = Math.Abs(point1.Y - point2.Y);

            return (int)(xapproach + yapproach);
        }

        private List<(Vector2, Vector2)> FoundPossibleFrightenedWays()
        {
            List<(Vector2, Vector2)> ways = new List<(Vector2, Vector2)>();
            var localPosition = Position;

            Vector2 velocityY = new Vector2(0, Velocity.Y);
            Vector2 velocityX = new Vector2(Velocity.X, 0);

            Vector2 localPositionUp = localPosition - velocityY;
            if (!IsInRestrictedArea(localPositionUp))
                ways.Add((-velocityY, localPositionUp));

            Vector2 localPositionLeft = localPosition - velocityX;
            if (!IsInRestrictedArea(localPositionLeft))
                ways.Add((-velocityX, localPositionLeft));

            Vector2 localPositionDown = localPosition + velocityY;
            if (!IsInRestrictedArea(localPositionDown))
                ways.Add((velocityY, localPositionDown));

            Vector2 localPositionRight = localPosition + velocityX;
            if (!IsInRestrictedArea(localPositionRight))
                ways.Add((velocityX, localPositionRight));

            return ways;
        }

        private bool IsInRestrictedArea(Vector2 position)
        {
            for (int i = 0; i < _restrictedWays.Objects.Length; i++)
            {
                var currentTiledObject = _restrictedWays.Objects[i];
                var restrictedRectIteration = new Rectangle((int)currentTiledObject.Position.X, (int)currentTiledObject.Position.Y, (int)currentTiledObject.Size.Width, (int)currentTiledObject.Size.Height);
                var currentRectPosition = new Rectangle((int)(position.X - _animation.Origin.X), (int)(position.Y - _animation.Origin.Y), Width, Height);

                if (currentRectPosition.Intersects(restrictedRectIteration))
                    return true;
            }

            return false;
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
            switch(direction)
            {
                case Direction.Up when GhostPhase != GhostPhase.Frightened:
                    currentAnimation = _animationNames.Up;
                    return;
                case Direction.Down when GhostPhase != GhostPhase.Frightened:
                    currentAnimation = _animationNames.Down;
                    return;
                case Direction.Left | Direction.None when GhostPhase != GhostPhase.Frightened:
                    currentAnimation = _animationNames.Left;
                    return;
                case Direction.Right when GhostPhase != GhostPhase.Frightened:
                    currentAnimation = _animationNames.Right;
                    return;
                case Direction.Up when GhostPhase != (GhostPhase.Scatter | GhostPhase.Chase):
                    currentAnimation = _animationNames.Vulnerable;
                    return;
                case Direction.Down when GhostPhase != (GhostPhase.Scatter | GhostPhase.Chase):
                    currentAnimation = _animationNames.Vulnerable;
                    return;
                case (Direction.Left | Direction.None) when GhostPhase != (GhostPhase.Scatter | GhostPhase.Chase):
                    currentAnimation = _animationNames.Vulnerable;
                    return;
                case Direction.Right when GhostPhase != (GhostPhase.Scatter | GhostPhase.Chase):
                    currentAnimation = _animationNames.Vulnerable;
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
