using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageQuest.Core
{
    public class PlayerSession
    {
        public List<Point> VisitedLocations { get; set; } = new List<Point>();
        public Stack<Step> Path { get; set; } = new Stack<Step>();
    }
}
