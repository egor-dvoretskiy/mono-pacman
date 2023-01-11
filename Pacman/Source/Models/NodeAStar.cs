using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Source.Models
{
    public struct NodeAStar
    {
        public (int,int) Position { get; set; }

        public int StepApproach { get; set; }

        public int EuristicApproach { get; set; }

        public int Weight
        {
            get => StepApproach + EuristicApproach;
        }
    }
}
