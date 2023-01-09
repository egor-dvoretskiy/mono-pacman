using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class Map
    {
        public TiledMap TiledMap { get; set; }

        public int[,] MatrixMap { get; set; }
    }
}
