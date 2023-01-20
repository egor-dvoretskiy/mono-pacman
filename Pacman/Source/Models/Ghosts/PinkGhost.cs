using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using Pacman.Source.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models.Ghosts
{
    public class PinkGhost : Ghost
    {
        public PinkGhost(
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
                  transitions,
                  scatterPosition)
        {
            /// Add to matrix map more numbers to avoid visiting transition tiles by ghosts.

            Name = "Pinky";
        }
    }
}
