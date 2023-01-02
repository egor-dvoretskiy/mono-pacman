using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Timers;
using Pacman.Source.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class Player : Pacman.Source.Abstract.Sprite
    {
        private readonly AnimatedSprite _animation;
        private readonly Input _input;
        private readonly TiledMap _map;
        private readonly TiledMapObjectLayer _restrictedWays;
        private readonly Vector2 _offset; 

        private float rotation;

        public Player(
            Texture2D texture, 
            Vector2 position, 
            Input input, 
            AnimatedSprite animatedSprite,
            Vector2 velocity,
            TiledMap map) 
            : base(texture, position)
        {
            _input = input;
            _animation = animatedSprite;
            _restrictedWays = map.ObjectLayers.Single(x => x.Name.Equals("Map-Collision"));
            _offset = animatedSprite.Origin;

            Velocity = velocity;
        }

        public Vector2 Velocity { get; set; }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var position = Position;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(_input.Up))
            {
                position.Y -= Velocity.Y;
                rotation = -90f;

                Move(Direction.Up, position);
            }
            else if (keyboardState.IsKeyDown(_input.Down))
            {
                position.Y += Velocity.Y;
                rotation = 90f;

                Move(Direction.Down, position);
            }
            else if (keyboardState.IsKeyDown(_input.Left))
            {
                position.X -= Velocity.X;
                rotation = 180f;

                Move(Direction.Left, position);
            }
            else if (keyboardState.IsKeyDown(_input.Right))
            {
                position.X += Velocity.X;
                rotation = 0f;

                Move(Direction.Right, position);
            }

            _animation.Play("player-main");
            _animation.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation, Position, MathHelper.ToRadians(rotation));
        }

        private void Move(Direction direction, Vector2 position)
        {
            if (IsInRestrictedArea(position))
                return;

            switch(direction)
            {
                case Direction.Left:
                    {

                    }
                    break;
                case Direction.Right:
                    {

                    }
                    break;
                case Direction.Up:
                    {

                    }
                    break;
                case Direction.Down:
                    {

                    }
                    break;
            }

            Position = position;
        }

        private bool IsInRestrictedArea(Vector2 position)
        {
            for (int i = 0; i < _restrictedWays.Objects.Length; i++)
            {
                var currentTiledObject = _restrictedWays.Objects[i];
                var restrictedRectIteration = new Rectangle((int)currentTiledObject.Position.X, (int)currentTiledObject.Position.Y, (int)currentTiledObject.Size.Width, (int)currentTiledObject.Size.Height);
                var currentRectPosition = new Rectangle((int)(position.X - _offset.X), (int)(position.Y - _offset.Y), Width, Height);

                if (currentRectPosition.Intersects(restrictedRectIteration))
                    return true;
            }

            return false;
        }
    }
}
