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

namespace Pacman.Source.Models
{
    public class Ghost : Abstract.Sprite
    {
        private readonly AnimatedSprite _animation;
        private readonly GhostAnimationCall _animationNames;

        private Direction direction = Direction.None;

        public Ghost(
            Texture2D texture, 
            Vector2 position,
            AnimatedSprite animatedSprite,
            GhostAnimationCall animationNames,
            Vector2 velocity,
            TiledMap map,
            Transitions transitions) 
            : base(texture, position)
        {
            _animation = animatedSprite;
            _animationNames = animationNames;
        }

        public Vector2 Velocity { get; set; }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _animation.Play(_animationNames.Left);
            _animation.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation, Position, 0);
        }
    }
}
