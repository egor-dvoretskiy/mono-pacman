using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using Pacman.Source.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class GhostGang
    {
        private readonly Ghost[] _ghosts;
        private readonly TiledMapObjectLayer _ghostSpawn;

        private readonly GhostAnimationCall _animationNamesBlue;
        private readonly GhostAnimationCall _animationNamesYellow;
        private readonly GhostAnimationCall _animationNamesRed;
        private readonly GhostAnimationCall _animationNamesGreen;

        private GhostPhase ghostPhase = GhostPhase.Scatter;

        public GhostGang(
            Texture2D texture,
            ContentManager contentManager,
            Vector2 velocity,
            TiledMap map,
            Transitions transitions
        )
        {
            _ghostSpawn = map.ObjectLayers.Single(x => x.Name.Equals("ghost-spawn-area"));
            
            _animationNamesRed = new GhostAnimationCall()
            {
                ConsumedLeft = "ghost-consumed-move-left",
                ConsumedDown = "ghost-consumed-move-down",
                ConsumedRight = "ghost-consumed-move-right",
                ConsumedUp = "ghost-consumed-move-up",
                Vulnerable = "ghost-red-vulnerable",
                Left = "ghost-red-move-left",
                Right = "ghost-red-move-right",
                Down = "ghost-red-move-down",
                Up = "ghost-red-move-up",
            };

            _animationNamesBlue = new GhostAnimationCall()
            {
                ConsumedLeft = "ghost-consumed-move-left",
                ConsumedDown = "ghost-consumed-move-down",
                ConsumedRight = "ghost-consumed-move-right",
                ConsumedUp = "ghost-consumed-move-up",
                Vulnerable = "ghost-blue-vulnerable",
                Left = "ghost-blue-move-left",
                Right = "ghost-blue-move-right",
                Down = "ghost-blue-move-down",
                Up = "ghost-blue-move-up",
            };

            _animationNamesYellow = new GhostAnimationCall()
            {
                ConsumedLeft = "ghost-consumed-move-left",
                ConsumedDown = "ghost-consumed-move-down",
                ConsumedRight = "ghost-consumed-move-right",
                ConsumedUp = "ghost-consumed-move-up",
                Vulnerable = "ghost-yellow-vulnerable",
                Left = "ghost-yellow-move-left",
                Right = "ghost-yellow-move-right",
                Down = "ghost-yellow-move-down",
                Up = "ghost-yellow-move-up",
            };

            _animationNamesGreen = new GhostAnimationCall()
            {
                ConsumedLeft = "ghost-consumed-move-left",
                ConsumedDown = "ghost-consumed-move-down",
                ConsumedRight = "ghost-consumed-move-right",
                ConsumedUp = "ghost-consumed-move-up",
                Vulnerable = "ghost-green-vulnerable",
                Left = "ghost-green-move-left",
                Right = "ghost-green-move-right",
                Down = "ghost-green-move-down",
                Up = "ghost-green-move-up",
            };

            var redAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-red.sf", new JsonContentLoader()));
            Ghost red = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + redAnimation.Origin + new Vector2(redAnimation.Origin.X * 2, 0) * 0,
                redAnimation,
                _animationNamesRed,
                velocity,
                map,
                transitions
            );

            var blueAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-blue.sf", new JsonContentLoader()));
            Ghost blue = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + blueAnimation.Origin + new Vector2(blueAnimation.Origin.X * 2, 0) * 1,
                blueAnimation,
                _animationNamesBlue,
                velocity,
                map,
                transitions
            );

            var yellowAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-yellow.sf", new JsonContentLoader()));
            Ghost yellow = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + yellowAnimation.Origin + new Vector2(yellowAnimation.Origin.X * 2, 0) * 2,
                yellowAnimation,
                _animationNamesYellow,
                velocity,
                map,
                transitions
            );

            var greenAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-green.sf", new JsonContentLoader()));
            Ghost green = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + greenAnimation.Origin + new Vector2(greenAnimation.Origin.X * 2, 0) * 3,
                greenAnimation,
                _animationNamesGreen,
                velocity,
                map,
                transitions
            );

            _ghosts = new Ghost[]
            {
                red, 
                blue, 
                yellow, 
                green
            };            
        }

        public void Update(GameTime gameTime)
        {
            foreach (var ghost in _ghosts)
            {
                ghost.GhostPhase = ghostPhase;
                ghost.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var ghost in _ghosts)
            {
                ghost.Draw(spriteBatch);
            }
        }
    }
}
