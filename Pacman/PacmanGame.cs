﻿using Microsoft.Xna.Framework;
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

        public PacmanGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = false;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0f);
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            tiledMapRenderer.Draw();

            spriteBatch.Begin();

            player.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}