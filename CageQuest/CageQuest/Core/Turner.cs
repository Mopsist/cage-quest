namespace CageQuest.Core
{
    public class Turner
    {
        private PlayerDirection _direction;
        private Location _location;

        public Turner(PlayerDirection direction, Location location)
        {
            _direction = direction;
            _location = location;
        }

        public Point GoRight()
        {
            var nextLocation = new Point(_location);
            switch (_direction)
            {
                case PlayerDirection.North:
                    nextLocation.X++;
                    break;
                case PlayerDirection.East:
                    nextLocation.Y--;
                    break;
                case PlayerDirection.South:
                    nextLocation.X--;
                    break;
                case PlayerDirection.West:
                    nextLocation.Y++;
                    break;
            }
            return nextLocation;
        }

        public Point GoLeft()
        {
            var nextLocation = new Point(_location);
            switch (_direction)
            {
                case PlayerDirection.North:
                    nextLocation.X--;
                    break;
                case PlayerDirection.East:
                    nextLocation.Y++;
                    break;
                case PlayerDirection.South:
                    nextLocation.X++;
                    break;
                case PlayerDirection.West:
                    nextLocation.Y--;
                    break;
            }
            return nextLocation;
        }

        public Point GoForward()
        {
            var nextLocation = new Point(_location);
            switch (_direction)
            {
                case PlayerDirection.North:
                    nextLocation.Y++;
                    break;
                case PlayerDirection.East:
                    nextLocation.X++;
                    break;
                case PlayerDirection.South:
                    nextLocation.Y--;
                    break;
                case PlayerDirection.West:
                    nextLocation.X--;
                    break;
            }
            return nextLocation;
        }
    }
}
