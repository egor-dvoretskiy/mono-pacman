using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public class GhostAnimationCall
    {
        public string Left { get; set; }

        public string Right { get; set; }

        public string Up { get; set; }

        public string Down { get; set; }

        public string Vulnerable { get; set; }

        public string ConsumedLeft { get; set; }

        public string ConsumedRight { get; set; }

        public string ConsumedUp { get; set; }

        public string ConsumedDown { get; set; }
    }
}
