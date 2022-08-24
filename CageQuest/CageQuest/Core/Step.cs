namespace CageQuest.Core
{
    public class Step
    {
        public Point Point { get; set; }
        public PlayerDirection Direction { get; set; }

        public Step()
        {
        }

        public Step(Point p, PlayerDirection d)
        {
            Point = p;
            Direction = d;
        }
    }
}
