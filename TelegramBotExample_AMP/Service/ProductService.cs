using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotExample_AMP.Entity;

namespace TelegramBotExample_AMP.Service
{
    // ProductService - сервис для работы с товарами - обеспечивает выполнение операций с сущностями
    internal class ProductService
    {
        private readonly String _connectionString;

        public ProductService(string connectionString)
        {
            _connectionString = connectionString;
        }
        //Получаем полный список продуктов
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, Article, OederId, CategoryId FROM Product_t";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            Article = reader["Article"].ToString(),
                            CategoryId = (int)reader["CategoryId"]
                        });
                    }
                }
            }

            return products;
        }
        //Поиск продуктов
        public List<Product> SearchProductsByName(string name)
        {
            var products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, Article FROM Product_t WHERE Name LIKE @Name";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", $"%{name}%");

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Article = reader.GetString(2)
                        });
                    }
                }
            }
            return products;
        }
        // Получаем список игр по категориям
        public List<Product> GetProductsByCategory(int categoryId)
        {
            var products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, Article, CategoryId FROM Product_t WHERE CategoryId = @CategoryId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryId", categoryId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0), 
                            Name= reader.GetString(1), 
                            Article = reader.GetString(2),
                            CategoryId= categoryId
                        });
                    }
                }
            }
            return products;
        }
    }
}
