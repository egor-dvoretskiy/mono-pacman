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
        protected readonly AnimatedSprite _animation;
        protected readonly GhostAnimationCall _animationNames;
        protected readonly Map _map;

        protected Direction direction = Direction.None;

        public Ghost(
            Texture2D texture,
            Vector2 position,
            AnimatedSprite animatedSprite,
            GhostAnimationCall animationNames,
            Vector2 velocity,
            Map map,
            Transitions transitions)
            : base(texture, position)
        {
            _animation = animatedSprite;
            _animationNames = animationNames;
            _map = map;
        }

        public Vector2 Velocity { get; set; }

        public GhostPhase GhostPhase { get; set; }

        public abstract string Name { get; init; }

        protected abstract void Move();
    }
}
