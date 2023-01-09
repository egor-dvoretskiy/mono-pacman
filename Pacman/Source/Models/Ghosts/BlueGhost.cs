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
        private readonly AStarProcessor _astarProcessor;
        private readonly (int, int) _scatterPosition;

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
            _astarProcessor = new AStarProcessor(map.MatrixMap);
        }

        public override string Name { get; init; }

        public (int, int) PositionMatrix
        {
            get => ((int)Position.X / Width, (int)Position.Y / Height);
        }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _astarProcessor.Process(_scatterPosition, PositionMatrix);
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
    }
}
