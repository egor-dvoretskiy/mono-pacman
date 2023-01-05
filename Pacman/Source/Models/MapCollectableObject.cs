using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pacman.Source.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class MapCollectableObject : Sprite
    {

        public MapCollectableObject(Texture2D texture, Vector2 position) 
            : base(texture, position)
        {
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public bool IsIntesectsWithPlayer(Player player)
        {
            return
                player.Position.X + player.Width / 2 > BoxIntersectionForCollectableObject.X + BoxIntersectionForCollectableObject.Width &&
                player.Position.X - player.Width / 2 < BoxIntersectionForCollectableObject.X &&
                player.Position.Y - player.Height / 2 < BoxIntersectionForCollectableObject.Y &&
                player.Position.Y + player.Height / 2 > BoxIntersectionForCollectableObject.Y + BoxIntersectionForCollectableObject.Height;
        }
    }
}
