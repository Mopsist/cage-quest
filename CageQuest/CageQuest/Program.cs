using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CageQuest
{
    public static class UserInputHandler
    {
        public static void HandleUserInput(string userInput)
        {
            if (userInput == "q")
                throw new QuitGameException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            JObject scripts = new JObject();
            using (StreamReader reader = new StreamReader("Resources/scripts.json"))
            {
                string jsonString = reader.ReadToEnd();
                scripts = JObject.Parse(jsonString);
            }

            try
            {
                Console.WriteLine(scripts["intro"].Value<string>());
                Console.WriteLine("Продолжить? [Да] - любая клавиша, [Выход] - q");

                UserInputHandler.HandleUserInput(Console.ReadLine());

                Console.WriteLine(scripts["prologue"].Value<string>());



            }
            catch (QuitGameException ex)
            {
                Console.WriteLine("Выход из игры. Чтобы закрыть консоль нажмите что-нибудь");
                Console.ReadKey();
            }

        }
    }
}
