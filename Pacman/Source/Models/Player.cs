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
        private readonly Transitions _transitions;

        private float rotation;
        private PlayerStatus status = PlayerStatus.None;
        private Direction direction = Direction.None;

        public Player(
            Texture2D texture, 
            Vector2 position, 
            Input input, 
            AnimatedSprite animatedSprite,
            Vector2 velocity,
            TiledMap map,
            Transitions transitions) 
            : base(texture, position)
        {
            _map = map;
            _input = input;
            _animation = animatedSprite;
            _restrictedWays = map.ObjectLayers.Single(x => x.Name.Equals("map-restricted"));
            _offset = animatedSprite.Origin;
            _transitions = transitions;

            Velocity = velocity;
        }

        public Vector2 Velocity { get; init; }

        public Direction Direction
        {
            get => direction;
        }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(_input.Up))
            {
                status = PlayerStatus.NeedUp;
            }
            else if (keyboardState.IsKeyDown(_input.Down))
            {
                status = PlayerStatus.NeedDown;
            }
            else if (keyboardState.IsKeyDown(_input.Left))
            {
                status = PlayerStatus.NeedLeft;
            }
            else if (keyboardState.IsKeyDown(_input.Right))
            {
                status = PlayerStatus.NeedRight;
            }

            if (status == PlayerStatus.None && direction != Direction.None)
                MoveDirection();
            else if (status != PlayerStatus.None)
                MoveStatus();

            CheckTransitions();

            _animation.Play("player-main");
            _animation.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation, Position, MathHelper.ToRadians(rotation));
        }

        private void CheckTransitions()
        {
            if (Position.X == _transitions.Top.Position.X && Position.Y < _transitions.Top.Position.Y && direction == _transitions.Top.Direction)
            {
                Position = new Vector2(_transitions.Down.Position.X, _transitions.Down.Position.Y /*- _transitions.Down.Size.Height + 1*/);
                direction = Direction.Up;
                return;
            }

            if (Position.X == _transitions.Down.Position.X && Position.Y > _transitions.Down.Position.Y && direction == _transitions.Down.Direction)
            {
                Position = new Vector2(_transitions.Top.Position.X, _transitions.Top.Position.Y /*+ _transitions.Top.Size.Height - 1*/);
                direction = Direction.Down;
                return;
            }
        }

        private void MoveStatus()
        {
            Direction localDirection = Direction.None;
            var position = Position;

            switch (status)
            {
                case PlayerStatus.NeedLeft:
                    {
                        position.X -= Velocity.X;
                        localDirection = Direction.Left;
                    }
                    break;
                case PlayerStatus.NeedRight:
                    {
                        position.X += Velocity.X;
                        localDirection = Direction.Right;
                    }
                    break;
                case PlayerStatus.NeedUp:
                    {
                        position.Y -= Velocity.Y;
                        localDirection = Direction.Up;
                    }
                    break;
                case PlayerStatus.NeedDown:
                    {
                        position.Y += Velocity.Y;
                        localDirection = Direction.Down;
                    }
                    break;
                case PlayerStatus.None:
                    return;
            }

            if (IsInRestrictedArea(position))
            {
                MoveDirection();
                return;
            }

            direction = localDirection;
            status = PlayerStatus.None;

            Position = position;

            ApplyRotation();
        }

        private void MoveDirection()
        {
            var position = Position;

            switch (direction)
            {
                case Direction.Left:
                    {
                        position.X -= Velocity.X;
                    }
                    break;
                case Direction.Right:
                    {
                        position.X += Velocity.X;
                    }
                    break;
                case Direction.Up:
                    {
                        position.Y -= Velocity.Y;
                    }
                    break;
                case Direction.Down:
                    {
                        position.Y += Velocity.Y;
                    }
                    break;
                default:
                    return;
            }

            if (IsInRestrictedArea(position))
                return;

            Position = position;
        }

        private void ApplyRotation()
        {
            switch (direction)
            {
                case Direction.Left:
                    {
                        rotation = 180f;
                    }
                    break;
                case Direction.Right:
                    {
                        rotation = 0f;
                    }
                    break;
                case Direction.Up:
                    {
                        rotation = -90f;
                    }
                    break;
                case Direction.Down:
                    {
                        rotation = 90f;
                    }
                    break;
            }
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
