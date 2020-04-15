using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ILNet.Tools
{
    public class CheckerInput
    {

        public bool CheckInput(string val)
        {
            string str = @"\d{3,4}-\d{8}";
            Regex regex = new Regex(str);
            if (Regex.IsMatch(val.ToLower(), "/response|group_concat|cmd|sysdate|xor|declare|db_name|char| and| or|truncate|" +
                "asc|desc|drop|table|count|from|select|insert|update|delete|union|into|load_file|outfile/"))
            {
                Console.WriteLine("Illegal input...");
                return false;
            }
            else
            {
                if (Regex.IsMatch(val, str))
                {
                    Console.WriteLine("Password format is correct...");
                    return true;
                }
                else
                {
                    Console.WriteLine("Password format error...");
                    return false;
                }
            }

        }
    }
}
