using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotExample_AMP.Entity;
using TelegramBotExample_AMP.Service;
using TelegramBotExample_AMP.TgBotApi; 

namespace TelegramBotExample_AMP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Токен телеграмм-бота
            string token = @"7367609949:AAHgKegNHLBaVgviG83jOT6Lg8rx28VjF1U";
            //Строка соединения с бд
            string connectionString = 
                @"Data Source=217.28.223.127,17160; 
                User Id=user_7360f;
                Password=Qf9&*Sx8Xj7=2;
                Initial Catalog=db_f0e35;
                TrustServerCertificate=Yes;";

            /*@"Data Source=DESKTOP-RMUTB0V\SQLEXPRESS; 
                Initial Catalog=TelegramBot_db; 
                Integrated Security=SSPI";*/
            //Запуск бота
            var bot = new Bot(token, connectionString);
            bot.StartReceiving();
        }
    }
}
