using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using Pacman.Source.Enum;
using Pacman.Source.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Abstract
{
    public abstract class Ghost : Sprite
    {
        protected readonly Map _map;
        protected readonly AnimatedSprite _animation;
        protected readonly AStarProcessor _astarProcessor;
        protected readonly GhostAnimationCall _animationNames;
        protected readonly TiledMapObjectLayer _restrictedWays;
        protected readonly Transitions _transitions;
        protected readonly IEnumerable<(int, int)> _patrolZone;
        protected readonly (int, int) _scatterPosition;

        protected Direction direction = Direction.None;
        protected GhostMode ghostMode = GhostMode.None;
        protected Vector2 latestPlayerPosition;
        protected Direction latestPlayerDirection;
        protected string currentAnimation;
        protected (int, int) _patrolScatterPosition;
        protected (int, int) _stepScatterPosition;
        protected (int, int) _stepChasePosition;

        protected LinkedList<NodeAStar> astarPath;
        protected LinkedListNode<NodeAStar> currentLinkedNode;

        public Ghost(
            Texture2D texture,
            Vector2 position,
            AnimatedSprite animatedSprite,
            GhostAnimationCall animationNames,
            Vector2 velocity,
            Map map,
            IEnumerable<(int, int)> patrolZone,
            Transitions transitions,
            Vector2 scatterPosition)
            : base(texture, position)
        {
            _scatterPosition = ((int)scatterPosition.X / map.TiledMap.TileWidth, (int)scatterPosition.Y / map.TiledMap.TileHeight);
            _restrictedWays = map.TiledMap.ObjectLayers.Single(x => x.Name.Equals("map-restricted"));
            _astarProcessor = new AStarProcessor(map.MatrixMap);
            _stepScatterPosition = PositionMatrix;
            _stepChasePosition = PositionMatrix;
            _animationNames = animationNames;
            _animation = animatedSprite;
            _transitions = transitions;
            _patrolZone = patrolZone;
            _map = map;

            Velocity = velocity;
            currentAnimation = _animationNames.Up;
        }

        public Vector2 Velocity { get; set; }

        public GhostPhase GhostPhase { get; set; }

        public GhostPhase PreviousGhostPhase { get; set; }

        public string Name { get; init; }

        public Vector2 PositionOriginOffset
        {
            get => new Vector2(Position.X - _animation.Origin.X, Position.Y - _animation.Origin.Y);
        }

        public (int, int) PositionMatrix
        {
            get => ((int)Position.X / Width, (int)Position.Y / Height);
        }

        public (int, int) LatestPlayerPositionMatrix
        {
            get
            {
                var xrest = latestPlayerPosition.X % _map.TiledMap.TileWidth;
                var yrest = latestPlayerPosition.Y % _map.TiledMap.TileHeight;
                var xequation = latestPlayerPosition.X / _map.TiledMap.TileWidth;
                var yequation = latestPlayerPosition.Y / _map.TiledMap.TileHeight;

                if (xrest != 0 && yrest == 0)
                    return (latestPlayerDirection == Direction.Left ? (int)Math.Floor(xequation) : (int)Math.Ceiling(xequation), (int)yequation);

                if (xrest == 0 && yrest != 0)
                    return ((int)xequation, latestPlayerDirection == Direction.Up ? (int)Math.Floor(yequation) : (int)Math.Ceiling(yequation));

                return ((int)xequation, (int)yequation);
            }
        }

        public Vector2 PatrolScatterPosition
        {
            get => new Vector2(_patrolScatterPosition.Item1 * _map.TiledMap.TileWidth, _patrolScatterPosition.Item2 * _map.TiledMap.TileHeight);
        }

        public Vector2 StepScatterPosition
        {
            get => new Vector2(_stepScatterPosition.Item1 * _map.TiledMap.TileWidth, _stepScatterPosition.Item2 * _map.TiledMap.TileHeight);
        }

        public Vector2 StepChasePosition
        {
            get => new Vector2(_stepChasePosition.Item1 * _map.TiledMap.TileWidth, _stepChasePosition.Item2 * _map.TiledMap.TileHeight);
        }

        public Vector2 ScatterPosition
        {
            get => new Vector2(_scatterPosition.Item1 * _map.TiledMap.TileWidth, _scatterPosition.Item2 * _map.TiledMap.TileHeight);
        }

        public virtual void UpdatePlayerPosition(Vector2 playerPosition, Direction playerDirection)
        {
            latestPlayerPosition = playerPosition;
            latestPlayerDirection = playerDirection;
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

        public void ResetMovement()
        {
            if (astarPath != null)
            {
                astarPath.Clear();
                astarPath = null;
            }

            currentLinkedNode = null;
            _stepChasePosition = PositionMatrix;
            _stepScatterPosition = PositionMatrix;
        }

        protected virtual void Move()
        {
            switch (GhostPhase)
            {
                case Enum.GhostPhase.Scatter:
                    {
                        MoveScatter();
                    }
                    break;
                case Enum.GhostPhase.Chase:
                    {
                        MoveChase();
                    }
                    break;
                case Enum.GhostPhase.Frightened:
                    {
                        MoveFrightened();
                    }
                    break;
            }
        }

        protected virtual void MoveFrightened()
        {
            List<(Vector2, Vector2)> localWays = FoundPossibleFrightenedWays(); // (velocity, point)

            if (localWays == null || localWays.Count == 0)
                return;

            (Vector2, Vector2) nextPointTuple = FoundFarestWayByEuristicSearch(localWays); // search for the next point

            Position = nextPointTuple.Item2;
            AssignDirection(nextPointTuple.Item1);
        }

        protected virtual void MoveChase()
        {

        }

        protected virtual void MoveScatter()
        {
            if (IsPositionAtTarget() || IsPositionAtPatrolScatterPosition())
            {
                ghostMode = GhostMode.Patrol;

                astarPath = null;
                currentLinkedNode = null;
                SetPatrolPosition();
            }

            if (astarPath is null)
            {
                astarPath = _astarProcessor.FindPath(ghostMode == GhostMode.Patrol ? _patrolScatterPosition : _scatterPosition, PositionMatrix);

                if (astarPath != null)
                    currentLinkedNode = astarPath.First;
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

        protected void AssignAnimation()
        {
            switch (direction)
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

        protected void AssignDirection(Vector2 vector)
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

        protected (Vector2, Vector2) FoundFarestWayByEuristicSearch(List<(Vector2, Vector2)> localVelocities)
        {
            List<int> localDistances = new List<int>();
            int biggestEuristicDistance = 0;
            for (int i = 0; i < localVelocities.Count; i++)
            {
                int currentEuristicApproach = CalculateEuristicDistance(latestPlayerPosition, localVelocities[i].Item2);
                if (currentEuristicApproach > biggestEuristicDistance)
                {
                    localDistances.Clear();

                    biggestEuristicDistance = currentEuristicApproach;
                    localDistances.Add(i);
                    continue;
                }

                if (currentEuristicApproach == biggestEuristicDistance)
                    localDistances.Add(i);
            }

            return localVelocities[localDistances[Random.Shared.Next(localDistances.Count)]];
        }

        protected int CalculateEuristicDistance(Vector2 point1, Vector2 point2)
        {
            var xapproach = Math.Abs(point2.X - point1.X);
            var yapproach = Math.Abs(point1.Y - point2.Y);

            return (int)(xapproach + yapproach);
        }

        protected List<(Vector2, Vector2)> FoundPossibleFrightenedWays()
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

        protected bool IsInRestrictedArea(Vector2 position)
        {
            var currentRectPosition = new Rectangle((int)(position.X - _animation.Origin.X), (int)(position.Y - _animation.Origin.Y), Width, Height);

            for (int i = 0; i < _restrictedWays.Objects.Length; i++)
            {
                var currentTiledObject = _restrictedWays.Objects[i];
                var restrictedRectIteration = new Rectangle((int)currentTiledObject.Position.X, (int)currentTiledObject.Position.Y, (int)currentTiledObject.Size.Width, (int)currentTiledObject.Size.Height);

                if (currentRectPosition.Intersects(restrictedRectIteration))
                    return true;
            }

            if (currentRectPosition.Intersects(_transitions.Top.Rectangle) || currentRectPosition.Intersects(_transitions.Down.Rectangle))
                return true;

            return false;
        }

        protected bool IsPositionFitsStepScatterPosition() =>
            PositionOriginOffset.X == StepScatterPosition.X &&
            PositionOriginOffset.Y == StepScatterPosition.Y;

        protected bool IsPositionAtTarget() =>
            PositionOriginOffset.X == ScatterPosition.X &&
            PositionOriginOffset.Y == ScatterPosition.Y;

        protected bool IsPositionAtPatrolScatterPosition() =>
            PositionOriginOffset.X == PatrolScatterPosition.X &&
            PositionOriginOffset.Y == PatrolScatterPosition.Y;

        protected bool IsPositionAtStepChasePosition() =>
            PositionOriginOffset.X == StepChasePosition.X &&
            PositionOriginOffset.Y == StepChasePosition.Y;

        protected void SetPatrolPosition()
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
