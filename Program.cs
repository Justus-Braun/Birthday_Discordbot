using System;

namespace Birthday_Discordbot
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            MysqlClass mysql = new MysqlClass();
            mysql.Test();
        }
    }
}