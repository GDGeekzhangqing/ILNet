using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace TestEnum
{
    class Program
    {
        static void Main(string[] args)
        {
            Player player = new Player();
            player.ID = 666;
            player.Name = "Hello World";
            Weapon weaponA = new Weapon { des = "这是第一个数组" };
            Weapon weaponB = new Weapon { des = "这是第二个数组" };

            player.weaponLst.Add(weaponA);
            player.weaponLst.Add(weaponB);

            var json = JsonConvert.SerializeObject(player);         
            var bson = BsonSerializer.Deserialize<BsonDocument>(json);
        

            Console.WriteLine("打印数据："+bson.ToString());

            var pd = JsonConvert.DeserializeObject<Player>(json);
            Console.WriteLine("反序列化为对象："+pd);

            Console.ReadKey();

        }
    }
}
