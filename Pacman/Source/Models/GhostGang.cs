using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using Pacman.Source.Abstract;
using Pacman.Source.Enum;
using Pacman.Source.Models.Ghosts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Pacman.Source.Models
{
    public class GhostGang
    {
        private readonly Ghost[] _ghosts;
        private readonly Map _map;
        private readonly TiledMapObjectLayer _ghostSpawn;
        private readonly TiledMapObjectLayer _scatterPoints;

        private readonly GhostAnimationCall _animationNamesBlue;
        private readonly GhostAnimationCall _animationNamesYellow;
        private readonly GhostAnimationCall _animationNamesRed;
        private readonly GhostAnimationCall _animationNamesGreen;

        private readonly int _patrolScatterRadius = 6;
        private readonly int _frightenedTime = 200000;
        private readonly Timer _timer;

        //private GhostPhase ghostPhase = GhostPhase.Scatter;

        public GhostGang(
            Texture2D texture,
            ContentManager contentManager,
            Vector2 velocity,
            Map map,
            Transitions transitions
        )
        {
            _map = map;
            _ghostSpawn = map.TiledMap.ObjectLayers.Single(x => x.Name.Equals("ghost-spawn-area"));
            _scatterPoints = map.TiledMap.ObjectLayers.Single(x => x.Name.Equals("scatter-points"));
            _timer = new Timer()
            {
                AutoReset = false,
                Enabled = false,
                Interval = _frightenedTime,                
            };
            _timer.Elapsed += Timer_Elapsed;

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

            /*var redAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-red.sf", new JsonContentLoader()));
            Ghost red = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + redAnimation.Origin + new Vector2(redAnimation.Origin.X * 2, 0) * 0,
                redAnimation,
                _animationNamesRed,
                velocity,
                map,
                transitions
            );*/

            var blueAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-blue.sf", new JsonContentLoader()));
            var blueScatterPosition = _scatterPoints.Objects.Single(x => x.Name.Equals("blue-ghost-scatter-point")).Position;
            BlueGhost blue = new BlueGhost(
                texture,
                _ghostSpawn.Objects.Single().Position + blueAnimation.Origin + new Vector2(blueAnimation.Origin.X * 2, 0) * 1,
                blueAnimation,
                _animationNamesBlue,
                velocity,
                map,
                CalculatePatrolZone(blueScatterPosition),
                blueScatterPosition, // do i need apply offset? origin
                transitions
            );

            /*var yellowAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-yellow.sf", new JsonContentLoader()));
            Ghost yellow = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + yellowAnimation.Origin + new Vector2(yellowAnimation.Origin.X * 2, 0) * 2,
                yellowAnimation,
                _animationNamesYellow,
                velocity,
                map,
                transitions
            );*/

            /*var greenAnimation = new AnimatedSprite(contentManager.Load<SpriteSheet>("Spritesheets/pacman-ghost-green.sf", new JsonContentLoader()));
            Ghost green = new Ghost(
                texture,
                _ghostSpawn.Objects.Single().Position + greenAnimation.Origin + new Vector2(greenAnimation.Origin.X * 2, 0) * 3,
                greenAnimation,
                _animationNamesGreen,
                velocity,
                map,
                transitions
            );*/

            _ghosts = new Ghost[]
            {
                //red, 
                blue, 
                //yellow, 
                //green
            };

            SetGhostPhase(GhostPhase.Scatter);
        }

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            foreach (var ghost in _ghosts)
            {
                //ghost.GhostPhase = ghostPhase;
                ghost.Update(gameTime);
                ghost.UpdatePlayerPosition(playerPosition);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var ghost in _ghosts)
            {
                ghost.Draw(spriteBatch);
            }
        }

        public void NotifyDanger()
        {
            _timer.Start();

            foreach (var ghost in _ghosts)
            {
                ghost.PreviousGhostPhase = ghost.GhostPhase;
                ghost.GhostPhase = GhostPhase.Frightened;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var ghost in _ghosts)
            {
                ghost.GhostPhase = ghost.PreviousGhostPhase;
            }
        }

        private void SetGhostPhase(GhostPhase ghostPhase)
        {
            foreach (var ghost in _ghosts)
            {
                ghost.GhostPhase = ghostPhase;
            }
        }

        private IEnumerable<(int, int)> CalculatePatrolZone(Vector2 referencePoint)
        {
            List<(int, int)> patrolZone = new List<(int, int)> ();

            var iborders = CalculateBorders((int)referencePoint.X / _map.TiledMap.TileWidth, 0, _map.MatrixMap.GetLength(0));
            var jborders = CalculateBorders((int)referencePoint.Y / _map.TiledMap.TileHeight, 0, _map.MatrixMap.GetLength(1));

            for (int i = iborders.Item1; i <= iborders.Item2; i++)
            {
                for (int j = jborders.Item1; j < jborders.Item2; j++)
                {
                    if (_map.MatrixMap[i, j] == 0)
                        patrolZone.Add((i, j));
                }
            }

            return patrolZone;
        }

        private (int, int) CalculateBorders(int pure, int limitX, int limitY)
        {
            int bottomBorder = pure - _patrolScatterRadius < limitX ? limitX : pure - _patrolScatterRadius;
            int upperBorder = pure + _patrolScatterRadius > limitY ? limitY - 1 : pure + _patrolScatterRadius;

            return (bottomBorder, upperBorder);
        }
    }
}
