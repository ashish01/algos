using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace tp
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph graph = Graph.Read(new StreamReader(@"C:\Users\ashish\code\algos\tp\graph.txt", Encoding.ASCII));
            //graph.PrintDot(Console.Out);
            PrintTraversal.PrintDot(graph, Console.Out);
        }
    }
}
