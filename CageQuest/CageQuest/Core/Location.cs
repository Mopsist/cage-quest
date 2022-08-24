using System.Collections.Generic;

namespace CageQuest.Core
{
    public class Location : Point
    {
        public LocationType Type { get; set; }
        public string Text { get; set; }
        public IEnumerable<UserAction> Options { get; set; }
    }
}
