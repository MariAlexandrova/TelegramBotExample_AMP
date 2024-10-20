using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotExample_AMP.Entity
{
    // Category - категория товара
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Category() { }

        public override string ToString()
        {
            return $"{Id} - {Name}";
        }
    }
}
