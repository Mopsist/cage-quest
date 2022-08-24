﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
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

    public class PlayerSession
    {
        public List<Point> VisitedLocations { get; set; } = new List<Point>();
        public Stack<Step> Path { get; set; } = new Stack<Step>();
    }

    public static class Message
    {
        public static void Text(string text)
        {
            Console.WriteLine(text);
        }

        public static void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void Controls(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    public class Program
    {
        public static Location CurrentLocation { get; set; }
        public static PlayerDirection CurrentDirection { get; set; }
        public static bool MusicOn { get; set; }

        public static List<Location> _allLocations = new List<Location>();
        public static PlayerSession _currentSession = new PlayerSession();
        public static JObject _scripts;

        private static SoundsService _soundsService = new SoundsService();

        static void Main(string[] args)
        {
            Console.SetWindowSize(150, 30);

            _soundsService.PlayBackgroundMusic();
            MusicOn = true;

            //TODO Delete after scripts.json will be completed
            using (StreamReader reader = new StreamReader("Resources/scripts.json"))
            {
                string jsonString = reader.ReadToEnd();
                _scripts = JObject.Parse(jsonString);
            }

            //using (StreamReader reader = new StreamReader("Resources/scripts_encrypted.txt"))
            //{
            //    string encryptedScripts = reader.ReadToEnd();
            //    string jsonString = EncryptionService.Decrypt(encryptedScripts);
            //    scripts = JObject.Parse(jsonString);
            //}

            //TODO Implement the encryption approach from scripts.json
            using (StreamReader reader = new StreamReader("Resources/locations.json"))
            {
                string jsonString = reader.ReadToEnd();
                _allLocations = JsonConvert.DeserializeObject<List<Location>>(jsonString);
            }


            try
            {
                var gameFound = LoadSavedGame();

                Message.Info("Добро пожаловать в Cage Quest!");
                Message.Info("↓↓↓ ГЛАВНОЕ МЕНЮ ↓↓↓\n");

                var startNewGame = LoadMainMenu(gameFound);


                if (startNewGame)
                {
                    Message.Text(_scripts["intro"].Value<string>());
                    Message.Controls("Продолжить? [Да] - W");
                    HandleUserInput(Console.ReadLine(), true);

                    Message.Text(_scripts["prologue"].Value<string>());
                    Message.Controls("Продолжить? [Да] - W");
                    HandleUserInput(Console.ReadLine(), true);

                    var startingPoint = new Step(new Point(4, 0), CurrentDirection);
                    _currentSession.Path.Push(startingPoint);

                    Message.Info("Подсказка: Для вызова справки нажмите [I]");

                    LoadLocation(startingPoint);
                }
                else
                {
                    Message.Info("Вы вернулись! Загружаем сохранение...");
                    LoadLocation(_currentSession.Path.Pop());
                }

            }
            catch (QuitGameException ex)
            {
                SaveProgress();
                Message.Info("Выход из игры. Чтобы закрыть консоль нажмите что-нибудь");
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

                _currentSession.Path.Push(step);

                //Show location text/description of location
                Message.Text(location.Text);

                if (!_currentSession.VisitedLocations.Contains(location))
                {
                    _currentSession.VisitedLocations.Add(location);
                }
                else
                {
                    Message.Info("Вы здесь уже были.");
                }
            }
            else
            {
                Message.Info("Туда прохода нет!");

                location = CurrentLocation;
            }

            //Message.Info($"DEBUG: текущая локация ({step.Point.X},{step.Point.Y}) ");

            var userActions = string.Empty;
            switch (location.Type)
            {
                case LocationType.Enter:
                    userActions = "[Идти вперед] - W";
                    break;
                case LocationType.Tunnel:
                    userActions = "[Идти вперед] - W, [Назад] - S";
                    break;
                case LocationType.Fork:
                    foreach(var option in location.Options)
                    {
                        switch (option)
                        {
                            case UserAction.GoLeft:
                                userActions += "[Налево] - A ";
                                break;
                            case UserAction.GoRight:
                                userActions += "[Направо] - D ";
                                break;
                            case UserAction.GoForward:
                                userActions += "[Вперед] - W ";
                                break;
                        }
                    }
                    userActions += "[Назад] - S";
                    break;
                case LocationType.DeadEnd:
                    userActions = "[Назад] - S";
                    break;
                case LocationType.End:
                    Thread.Sleep(20000);
                    Message.Controls("[Очнуться] - любая клавиша");
                    Console.ReadLine();
                    Message.Text(_scripts["epilogue"].Value<string>());
                    Message.Controls("[Выход] - любая клавиша");
                    Console.ReadLine();
                    return;
            }
            Message.Controls(userActions);
            var nextStep = HandleUserInput(Console.ReadLine());

            //it can be null if user saved progress(t) or triggered info panel(i)
            if (nextStep is not null)
                LoadLocation(nextStep);
            else
                LoadLocation(step);
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
                case "i":
                    Message.Info("--------СПРАВКА--------");
                    Message.Info("T - [Сохранить]");
                    Message.Info("Q - [Сохранить и выйти]");
                    Message.Info("U - [Включить/выключить музыку]");
                    Message.Info("-----------------------");
                    return null;
                case "q":
                    throw new QuitGameException();
                case "t":
                    SaveProgress();
                    Message.Info("Прогресс сохранен");
                    return null;
                case "u":
                    if (MusicOn)
                    {
                        _soundsService.StopBackgroundMusic();
                        MusicOn = false;
                    }
                    else
                    {
                        _soundsService.PlayBackgroundMusic();
                        MusicOn = true;
                    }
                    return null;
                case "a":
                    nextStep.Point = turner.GoLeft();
                    nextStep.Direction = ChangeDirection(UserAction.GoLeft);
                    break;
                case "d":
                    nextStep.Point = turner.GoRight();
                    nextStep.Direction = ChangeDirection(UserAction.GoRight);
                    break;
                case "s":
                    _currentSession.Path.Pop();
                    var previousLocation = _currentSession.Path.Pop();
                    nextStep.Point = previousLocation.Point;
                    nextStep.Direction = previousLocation.Direction;
                    break;
                case "w":
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

        public static void SaveProgress()
        {
            //maybe delete "WriteIndented = true" option after development
            var jsonProgress = System.Text.Json.JsonSerializer.Serialize(_currentSession, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText("Resources/saved-progress.json", jsonProgress);
        }

        public static bool LoadSavedGame()
        {
            var loaded = false;
            try
            {
                var path = "Resources/saved-progress.json";
                if (File.Exists(path))
                {
                    var jsonString = File.ReadAllText(path);
                    var playerSession = System.Text.Json.JsonSerializer.Deserialize<PlayerSession>(jsonString);
                    playerSession.Path = new Stack<Step>(playerSession.Path);
                    _currentSession = playerSession;

                    loaded = true;
                }
            }
            catch
            {
                Message.Info("Ошибка при загрузке сохранения :(");
            }

            return loaded;
        }

        /// <summary>
        /// Load the main menu of the game 
        /// </summary>
        /// <param name="gameFound"></param>
        /// <returns>
        /// <see langword="true"/> - if player starts a new game, 
        /// <see langword="false"/> - if continues saved game
        /// </returns>
        public static bool LoadMainMenu(bool gameFound)
        {
            if (gameFound)
                Message.Info("[Продолжить игру] - C");
            Message.Info("[Новая игра] - N");
            Message.Info("[Выход] - Q");
            var userDecision = Console.ReadLine().ToLower();

            switch (userDecision)
            {
                case "n":
                    if (gameFound)
                    {
                        Message.Info("Вы уверены, что хотите начать сначала? Сохраненный прогресс будет утерян.");
                        Message.Info("[Начать сначала] - N");
                        Message.Info("[Продолжить сохраненную игру] - C");
                        switch (Console.ReadLine().ToLower())
                        {
                            case "c":
                                return false;
                            case "n":
                                return true;
                            case "q":
                                throw new QuitGameException();
                            default:
                                return false;
                        }
                    }

                    return true;
                case "c":
                    return !gameFound;
                case "q":
                    throw new QuitGameException();
                default:
                    return LoadMainMenu(gameFound);
            }
        }
        

    }
}
