using Minfin.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Repository rep = new Repository();
            rep.AddRecord("1", "q", "q", "1", "q", "q", "", 2, DateTime.Now);
            Console.ReadKey();
        }
    }
}
