using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Abstract
{
    public abstract class Sprite
    {
        protected Texture2D texture;

        public Sprite(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.Position = position;
        }

        public Vector2 Position { get; set; }

        public Rectangle Box
        {
            get => new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                Width,
                Height
            );
        }

        public int Width
        {
            get => texture.Width;
        }

        public int Height
        {
            get => texture.Height;
        }

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color.White);
        }
    }
}
