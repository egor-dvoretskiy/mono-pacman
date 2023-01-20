using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Pacman.Source.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public struct Transition
    {
        public Vector2 Position { get; set; }

        public Size2 Size { get; set; }

        public Rectangle Rectangle
        {
            get => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.Width, (int)Size.Height);
        }

        public Direction Direction { get; set; }
    }
}
