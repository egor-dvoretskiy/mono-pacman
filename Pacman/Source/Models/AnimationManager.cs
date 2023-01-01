using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class AnimationManager
    {
        private Animation animation;

        private float timer;

        public Vector2 Position { get; set; }

        public AnimationManager(Animation animation)
        {
            this.animation = animation;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(animation.Texture,
                             Position,
                             new Rectangle(animation.CurrentFrame * animation.FrameWidth,
                                           0,
                                           animation.FrameWidth,
                                           animation.FrameHeight),
                             Color.White);
        }

        public void Play(Animation animation)
        {
            if (this.animation == animation)
                return;

            this.animation = animation;

            this.animation.CurrentFrame = 0;

            timer = 0;
        }

        public void Stop()
        {
            timer = 0f;

            animation.CurrentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer > animation.FrameSpeed)
            {
                timer = 0f;

                animation.CurrentFrame++;

                if (animation.CurrentFrame >= animation.FrameCount)
                    animation.CurrentFrame = 0;
            }
        }
    }
}
