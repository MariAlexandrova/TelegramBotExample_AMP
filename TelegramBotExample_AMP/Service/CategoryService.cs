using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotExample_AMP.Entity;

namespace TelegramBotExample_AMP.Service
{
    // CategoryService - сервис для работы с категориями - обеспечивает выполнение операций с сущностями
    
    internal class CategoryService
    {
        private readonly String _connectionString= @"
                Data Source=DESKTOP-RMUTB0V\SQLEXPRESS; 
                Initial Catalog=PlaySphereProducts; 
                Integrated Security=SSPI";

        public CategoryService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<Category> GetAllCategories() {

            var categories=new List<Category>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT Id, Name FROM Category_t";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    //var gamesMenu = new InlineKeyboardMarkup();
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                        });
                    }
                }
                connection.Close();
            }
            return categories;
        }
    }
}
