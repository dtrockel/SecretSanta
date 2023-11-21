using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretSanta
{
    public class Participant
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public Participant? Partner { get; set; }
        public Participant? SecretSanta { get; set; }
        public bool IsDrawn { get; set; }
    }
}
