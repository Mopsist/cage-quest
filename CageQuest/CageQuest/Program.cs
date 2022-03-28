using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CageQuest
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point()
        {
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(Location location)
        {
            X = location.X;
            Y = location.Y;
        }

        public static explicit operator Point(JObject obj)
        {
            return new Point(
                obj["x"].Value<int>(), 
                obj["y"].Value<int>());
        }

    }

    public class Location : Point
    {
        public LocationType Type { get; set; }
        public string Text { get; set; }
        public IEnumerable<UserAction> Options { get; set; }
    }

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

    public class Program
    {
        public static Location CurrentLocation { get; set; }
        public static PlayerDirection CurrentDirection { get; set; }

        public static List<Point> _visitedLocations = new List<Point>();
        public static List<Location> _allLocations = new List<Location>();
        public static Stack<Step> _path = new Stack<Step>();

        static void Main(string[] args)
        {
            JObject scripts = new JObject();
            using (StreamReader reader = new StreamReader("Resources/scripts.json"))
            {
                string jsonString = reader.ReadToEnd();
                scripts = JObject.Parse(jsonString);
            }

            using (StreamReader reader = new StreamReader("Resources/locations.json"))
            {
                string jsonString = reader.ReadToEnd();               
                _allLocations = JsonConvert.DeserializeObject<List<Location>>(jsonString);
            }

            try
            {
                Console.WriteLine(scripts["intro"].Value<string>());
                Console.WriteLine("Продолжить? [Да] - F, [Выход] - Q");
                HandleUserInput(Console.ReadLine(), true);

                Console.WriteLine(scripts["prologue"].Value<string>());
                Console.WriteLine("Продолжить? [Да] - F, [Выход] - Q");
                HandleUserInput(Console.ReadLine(), true);

                var startingPoint = new Step(new Point(4, 0), CurrentDirection);
                _path.Push(startingPoint);
                LoadLocation(startingPoint);

            }
            catch (QuitGameException ex)
            {
                Console.WriteLine("Выход из игры. Чтобы закрыть консоль нажмите что-нибудь");
                Console.ReadKey();
            }

        }

        public static void LoadLocation(Step step)
        {
            var location = _allLocations.FirstOrDefault(location => location.X == step.Point.X && location.Y == step.Point.Y);

            if(location != null)
            {
                CurrentLocation = location;
                CurrentDirection = step.Direction;

                _path.Push(step);

                //Show location text/description of location
                Console.WriteLine(location.Text);

                if (!_visitedLocations.Contains(location))
                {
                    _visitedLocations.Add(location);
                }
                else
                {
                    Console.WriteLine("Вы здесь уже были.");
                }
            }
            else
            {
                Console.WriteLine("Туда прохода нет!");

                location = CurrentLocation;
            }

            Console.WriteLine($"DEBUG: текущая локация ({step.Point.X},{step.Point.Y}) ");

            var userActions = string.Empty;
            switch (location.Type)
            {
                case LocationType.Enter:
                    userActions = "[Идти вперед] - F, [Выход] - Q";
                    break;
                case LocationType.Tunnel:
                    userActions = "[Идти вперед] - F, [Назад] - B, [Выход] - Q";
                    break;
                case LocationType.Fork:
                    foreach(var option in location.Options)
                    {
                        switch (option)
                        {
                            case UserAction.GoLeft:
                                userActions += "[Налево] - L ";
                                break;
                            case UserAction.GoRight:
                                userActions += "[Направо] - R ";
                                break;
                            case UserAction.GoForward:
                                userActions += "[Вперед] - F ";
                                break;
                        }
                    }
                    userActions += "[Назад] - B, [Выход] - Q";
                    break;
                case LocationType.DeadEnd:
                    userActions = "[Назад] - B, [Выход] - Q";
                    break;
                case LocationType.End:
                    userActions = "[Выход] - любая клавиша";
                    break;
            }
            Console.WriteLine(userActions);
            var nextStep = HandleUserInput(Console.ReadLine());

            LoadLocation(nextStep);
        }

        public static Step HandleUserInput(string userInput, bool anyKey = false)
        {
            //TODO Think about better solution
            if (anyKey)
            {
                switch (userInput.ToLower())
                {
                    case "q":
                        throw new QuitGameException();
                    default:
                        return null;
                }
            }

            var nextStep = new Step();

            var turner = new Turner(CurrentDirection, CurrentLocation);

            switch (userInput.ToLower())
            {
                case "q":
                    throw new QuitGameException();
                case "l":
                    nextStep.Point = turner.GoLeft();
                    nextStep.Direction = ChangeDirection(UserAction.GoLeft);
                    break;
                case "r":
                    nextStep.Point = turner.GoRight();
                    nextStep.Direction = ChangeDirection(UserAction.GoRight);
                    break;
                case "b":
                    _path.Pop();
                    var previousLocation = _path.Pop();
                    nextStep.Point = previousLocation.Point;
                    nextStep.Direction = previousLocation.Direction;
                    break;
                case "f":
                    nextStep.Point = turner.GoForward();
                    nextStep.Direction = CurrentDirection;
                    break;
                default:
                    nextStep.Point = turner.GoForward();
                    nextStep.Direction = CurrentDirection;
                    break;
            }

            return nextStep;
        }

        public static PlayerDirection ChangeDirection(UserAction directionChange)
        {
            PlayerDirection nextDirection = CurrentDirection;
            switch (CurrentDirection) 
            {
                case PlayerDirection.North:
                    nextDirection = directionChange == UserAction.GoLeft
                        ? PlayerDirection.West
                        : PlayerDirection.East;
                    break;
                case PlayerDirection.East:
                    nextDirection = directionChange == UserAction.GoLeft
                        ? PlayerDirection.North
                        : PlayerDirection.South;
                    break;
                case PlayerDirection.South:
                    nextDirection = directionChange == UserAction.GoLeft
                        ? PlayerDirection.East
                        : PlayerDirection.West;
                    break;
                case PlayerDirection.West:
                    nextDirection = directionChange == UserAction.GoLeft
                        ? PlayerDirection.South
                        : PlayerDirection.North;
                    break;
            }
            return nextDirection;
        }
    }
}
