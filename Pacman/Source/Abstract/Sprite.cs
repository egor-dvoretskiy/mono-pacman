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
        private readonly int _sizeInnerBoxIntersection = 4;

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

        public Rectangle BoxIntersectionForCollectableObject
        {
            get => new Rectangle(
                (int)Position.X + Width / 2 - _sizeInnerBoxIntersection / 2,
                (int)Position.Y + Height / 2 - _sizeInnerBoxIntersection / 2,
                _sizeInnerBoxIntersection,
                _sizeInnerBoxIntersection
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
