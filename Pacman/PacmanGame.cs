using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using Pacman.Source.Models;
using System.Collections.Generic;

namespace Pacman
{
    public class PacmanGame : Game
    {
        private TiledMap extendedMap;
        private TiledMapRenderer tiledMapRenderer;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private List<Sprite> sprites;

        public PacmanGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = false;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 680;
            graphics.PreferredBackBufferHeight = 480;
            graphics.ApplyChanges();

            GraphicsDevice.Clear(new Color(33, 33, 33));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            extendedMap = Content.Load<TiledMap>("Maps/map");
            tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, extendedMap);

            sprites = new List<Sprite>()
            {
                new Sprite(new Dictionary<string, Animation>()
                {
                    { "move-up", new Animation(Content.Load<Texture2D>("Objects/pink/move-up"), 2) },
                    { "move-down", new Animation(Content.Load<Texture2D>("Objects/pink/move-down"), 2) },
                    { "move-left", new Animation(Content.Load<Texture2D>("Objects/pink/move-left"), 2) },
                    { "move-right", new Animation(Content.Load<Texture2D>("Objects/pink/move-right"), 2) },
                })
                {
                    Position = new Vector2(100, 100),
                    Input = new Input()
                    {
                    Up = Keys.W,
                    Down = Keys.S,
                    Left = Keys.A,
                    Right = Keys.D,
                    },
                },
            };
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            tiledMapRenderer.Update(gameTime);

            foreach (var sprite in sprites)
                sprite.Update(gameTime, sprites);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            tiledMapRenderer.Draw();

            spriteBatch.Begin();

            foreach (var sprite in sprites)
                sprite.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}