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

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private AnimatedSprite mainSpritesheet;
        private Player player;

        private TiledMapObjectLayer mapTransitionsLayer;
        private Transitions transitions;

        private readonly List<MapCollectableObject> coins;
        private readonly List<MapCollectableObject> rubies;

        public PacmanGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = false;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0f);

            coins = new List<MapCollectableObject>();
            rubies = new List<MapCollectableObject>();
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

            extendedMap = Content.Load<TiledMap>("Maps/map");
            tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, extendedMap);

            #region coins

            var coinTexture = Content.Load<Texture2D>("Spritesheets/Map-Objects/coin");
            var coinsMapObjects = extendedMap.ObjectLayers.Single(x => x.Name.Equals("coins"));
            for (int i = 0; i < coinsMapObjects.Objects.Length; i++)
            {
                coins.Add(new MapCollectableObject(coinTexture, coinsMapObjects.Objects[i].Position));
            }

            #endregion

            #region rubies

            var rubyTexture = Content.Load<Texture2D>("Spritesheets/Map-Objects/ruby");
            var rubiesMapObjects = extendedMap.ObjectLayers.Single(x => x.Name.Equals("rubies"));
            for (int i = 0; i < rubiesMapObjects.Objects.Length; i++)
            {
                rubies.Add(new MapCollectableObject(rubyTexture, rubiesMapObjects.Objects[i].Position));
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

            var spriteSheet = Content.Load<SpriteSheet>("Spritesheets/pacman-spritesheet-main.sf", new JsonContentLoader());
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
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            tiledMapRenderer.Update(gameTime);
            player.Update(gameTime);

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

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawCoins()
        {
            for (int i = 0; i < coins.Count; i++)
            {
                coins[i].Draw(spriteBatch);
            }
        }

        private void DrawRubies()
        {
            for (int i = 0; i < rubies.Count; i++)
            {
                rubies[i].Draw(spriteBatch);
            }
        }

        private void CheckCoinIntersection(Player player)
        {
            for (int i = 0; i < coins.Count; i++)
            {
                if (coins[i].IsIntesectsWithPlayer(player))
                {
                    coins.RemoveAt(i);
                    return;
                }
            }
        }

        private void CheckRubyIntersection(Player player)
        {
            for (int i = 0; i < rubies.Count; i++)
            {
                if (rubies[i].IsIntesectsWithPlayer(player))
                {
                    rubies.RemoveAt(i);
                    return;
                }
            }
        }
    }
}