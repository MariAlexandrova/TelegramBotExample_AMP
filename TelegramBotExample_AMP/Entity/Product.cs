using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotExample_AMP.Entity
{
    // Product - класс товара
    internal class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Article {  get; set; }
        public int CategoryId { get; set; }   
        public Category Category { get; set; }
        public string Url => $"https://www.wildberries.ru/catalog/{Article}/detail.aspx?targetUrl=SN";
        public Product(){}
        public override string ToString()
        {
            return $"{Id} - {Name} - Артикул {Article} - Категория {Category}";
        }
    }
}
