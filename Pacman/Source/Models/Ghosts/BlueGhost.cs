using Autofac.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using Pacman.Source.Abstract;
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

        private (int, int) _stepScatterPosition;

        public BlueGhost(
            Texture2D texture,
            Vector2 position,
            AnimatedSprite animatedSprite,
            GhostAnimationCall animationNames,
            Vector2 velocity,
            Map map,
            Vector2 scatterPosition,
            Transitions transitions) 
            : base(
                  texture, 
                  position, 
                  animatedSprite, 
                  animationNames, 
                  velocity, 
                  map, 
                  transitions)

        {
            Name = "Bashful";
            _map = map;
            _astarProcessor = new AStarProcessor(map.MatrixMap);
            _scatterPosition = ((int)scatterPosition.X / map.TiledMap.TileWidth, (int)scatterPosition.Y / map.TiledMap.TileHeight);
            _stepScatterPosition = PositionMatrix;
        }

        public override string Name { get; init; }

        public (int, int) PositionMatrix
        {
            get => ((int)Position.X / Width, (int)Position.Y / Height);
        }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsPositionFitsStepScatterPosition())
                _stepScatterPosition = _astarProcessor.Process(_scatterPosition, PositionMatrix);

            Move();

            _animation.Play(_animationNames.Left);
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
                        if (IsPositionFitsStepScatterPosition())
                            break;

                        Vector2 direction = new Vector2(
                            Math.Sign(_stepScatterPosition.Item1 * _map.TiledMap.TileWidth - PositionMatrix.Item1 * _map.TiledMap.TileWidth) * Velocity.X,
                            Math.Sign(_stepScatterPosition.Item2 * _map.TiledMap.TileHeight - PositionMatrix.Item2 * _map.TiledMap.TileHeight) * Velocity.Y
                        );

                        Position += direction;
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
            Position.X == _stepScatterPosition.Item1 * _map.TiledMap.TileWidth &&
            Position.Y == _stepScatterPosition.Item2 * _map.TiledMap.TileHeight;
    }
}
