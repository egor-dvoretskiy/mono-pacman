using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using Pacman.Source.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pacman
{
    public class PacmanGame : Game
    {
        private TiledMap extendedMap;
        private TiledMapRenderer tiledMapRenderer;
        private int[,] mapMatrix;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private AnimatedSprite mainSpritesheet;
        private Player player;
        private GhostGang _ghostGang;

        private TiledMapObjectLayer mapTransitionsLayer;
        private Transitions transitions;

        private readonly List<MapCollectableObject> _coins;
        private readonly List<MapCollectableObject> _rubies;

        public PacmanGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = false;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0f);

            _coins = new List<MapCollectableObject>();
            _rubies = new List<MapCollectableObject>();
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 680;
            graphics.PreferredBackBufferHeight = 480;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region map

            extendedMap = Content.Load<TiledMap>("Maps/map");
            tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, extendedMap);
            mapMatrix = new int[extendedMap.Width, extendedMap.Height];
            FillMapMatrix();

            #endregion

            #region coins

            var coinTexture = Content.Load<Texture2D>("Spritesheets/Map-Objects/coin");
            var coinsMapObjects = extendedMap.ObjectLayers.Single(x => x.Name.Equals("coins"));
            for (int i = 0; i < coinsMapObjects.Objects.Length; i++)
            {
                _coins.Add(new MapCollectableObject(coinTexture, coinsMapObjects.Objects[i].Position));
            }

            #endregion

            #region rubies

            var rubyTexture = Content.Load<Texture2D>("Spritesheets/Map-Objects/ruby");
            var rubiesMapObjects = extendedMap.ObjectLayers.Single(x => x.Name.Equals("rubies"));
            for (int i = 0; i < rubiesMapObjects.Objects.Length; i++)
            {
                _rubies.Add(new MapCollectableObject(rubyTexture, rubiesMapObjects.Objects[i].Position));
            }

            #endregion

            #region transitions

            mapTransitionsLayer = extendedMap.ObjectLayers.Single(x => x.Name.Equals("transition-vertical"));
            var topTransition = mapTransitionsLayer.Objects.Single(x => x.Name.Equals("top"));
            var downTransition = mapTransitionsLayer.Objects.Single(x => x.Name.Equals("down"));

            transitions = new Transitions()
            {
                Top = new Transition()
                {
                    Direction = Source.Enum.Direction.Up,
                    Position = topTransition.Position + new Vector2(topTransition.Size.Width / 2, topTransition.Size.Height / 2 - topTransition.Size.Height + 1),
                    Size = topTransition.Size
                },
                Down = new Transition()
                {
                    Direction = Source.Enum.Direction.Down,
                    Position = downTransition.Position + new Vector2(downTransition.Size.Width / 2, downTransition.Size.Height / 2 + downTransition.Size.Height - 1),
                    Size = downTransition.Size
                },
            };

            #endregion

            #region player

            var playerStartPositionObj = extendedMap.ObjectLayers.Single(x => x.Name.Equals("start-position")).Objects.First();
            var playerStartPosition = playerStartPositionObj.Position;
            playerStartPosition.X += playerStartPositionObj.Size.Width / 2;
            playerStartPosition.Y += playerStartPositionObj.Size.Height / 2;

            var spriteSheet = Content.Load<SpriteSheet>("Spritesheets/pacman-main.sf", new JsonContentLoader());
            mainSpritesheet = new AnimatedSprite(spriteSheet);

            player = new Player(
                new Texture2D(GraphicsDevice, (int)playerStartPositionObj.Size.Width, (int)playerStartPositionObj.Size.Height), 
                playerStartPosition, 
                new Input()
                {
                    Up = Keys.W,
                    Down = Keys.S,
                    Left = Keys.A,
                    Right = Keys.D,
                },
                mainSpritesheet,
                new Vector2(1, 1),
                extendedMap,
                transitions
            );

            #endregion

            #region ghosts

            _ghostGang = new GhostGang(
                new Texture2D(GraphicsDevice, (int)playerStartPositionObj.Size.Width, (int)playerStartPositionObj.Size.Height),
                Content,
                new Vector2(1, 1),
                extendedMap,
                transitions
            );

            #endregion
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            tiledMapRenderer.Update(gameTime);
            player.Update(gameTime);
            _ghostGang.Update(gameTime);

            CheckCoinIntersection(player);
            CheckRubyIntersection(player);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            tiledMapRenderer.Draw();

            spriteBatch.Begin();

            DrawCoins();
            DrawRubies();

            player.Draw(spriteBatch);
            _ghostGang.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawCoins()
        {
            for (int i = 0; i < _coins.Count; i++)
            {
                _coins[i].Draw(spriteBatch);
            }
        }

        private void DrawRubies()
        {
            for (int i = 0; i < _rubies.Count; i++)
            {
                _rubies[i].Draw(spriteBatch);
            }
        }

        private void CheckCoinIntersection(Player player)
        {
            for (int i = 0; i < _coins.Count; i++)
            {
                if (_coins[i].IsIntesectsWithPlayer(player))
                {
                    _coins.RemoveAt(i);
                    return;
                }
            }
        }

        private void CheckRubyIntersection(Player player)
        {
            for (int i = 0; i < _rubies.Count; i++)
            {
                if (_rubies[i].IsIntesectsWithPlayer(player))
                {
                    _rubies.RemoveAt(i);
                    return;
                }
            }
        }

        private void FillMapMatrix()
        {
            var restricted = extendedMap.ObjectLayers.Single(x => x.Name.Equals("map-restricted"));
            foreach (var obj in restricted.Objects)
            {
                var position = obj.Position;
                int i = (int)position.X / extendedMap.TileWidth;
                int j = (int)position.Y / extendedMap.TileHeight;

                if (obj.Name.Equals("ghost-entrance-1") || obj.Name.Equals("ghost-entrance-2"))
                    continue;

                //pzdc, i know
                for (int i2 = 0; i2 < obj.Size.Width / extendedMap.TileWidth; i2++)
                {
                    for (int j2 = 0; j2 < obj.Size.Height / extendedMap.TileHeight; j2++)
                    {
                        mapMatrix[j + j2, i + i2] = 1;
                    }
                }

            }
        }
    }
}