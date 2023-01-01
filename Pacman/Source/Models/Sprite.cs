using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class Sprite
    {
        #region Fields

        protected AnimationManager animationManager;

        protected Dictionary<string, Animation> animations;

        protected Vector2 position;

        protected Texture2D texture;

        #endregion

        #region Properties

        public Input Input;

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;

                if (animationManager != null)
                    animationManager.Position = position;
            }
        }

        public float Speed = 1f;

        public Vector2 Velocity;

        #endregion

        #region Methods

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, Position, Color.White);
            else if (animationManager != null)
                animationManager.Draw(spriteBatch);
            else throw new Exception("This ain't right..!");
        }

        public virtual void Move()
        {
            if (Keyboard.GetState().IsKeyDown(Input.Up))
                Velocity.Y = -Speed;
            else if (Keyboard.GetState().IsKeyDown(Input.Down))
                Velocity.Y = Speed;
            else if (Keyboard.GetState().IsKeyDown(Input.Left))
                Velocity.X = -Speed;
            else if (Keyboard.GetState().IsKeyDown(Input.Right))
                Velocity.X = Speed;
        }

        protected virtual void SetAnimations()
        {
            if (Velocity.X > 0)
                animationManager.Play(animations["move-right"]);
            else if (Velocity.X < 0)
                animationManager.Play(animations["move-left"]);
            else if (Velocity.Y > 0)
                animationManager.Play(animations["move-down"]);
            else if (Velocity.Y < 0)
                animationManager.Play(animations["move-up"]);
            else animationManager.Stop();
        }

        public Sprite(Dictionary<string, Animation> animations)
        {
            this.animations = animations;
            animationManager = new AnimationManager(this.animations.First().Value);
        }

        public Sprite(Texture2D texture)
        {
            this.texture = texture;
        }

        public virtual void Update(GameTime gameTime, List<Sprite> sprites)
        {
            Move();

            SetAnimations();

            animationManager.Update(gameTime);

            Position += Velocity;
            Velocity = Vector2.Zero;
        }

        #endregion
    }
}
