using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiPlatformTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string msg = "This is a string!";
            Console.WriteLine("Original String: " + msg);
            Console.WriteLine();
            Console.WriteLine("Using Left(msg, 9) VB: " + MultiPlatformTestVB.functions.getLeft(msg));
            Console.WriteLine("Using Right(msg, 3) VB: " + MultiPlatformTestVB.functions.getRight(msg));
            Console.WriteLine("Using Mid(msg, 5, 3) VB: " + MultiPlatformTestVB.functions.getMid(msg));
            Console.WriteLine("Using Instr(3, msg, \"str\") VB: " + MultiPlatformTestVB.functions.getInstr(msg));
            Console.WriteLine("Using Replace(msg, 'a', '*'+'b',1,2) VB: " + MultiPlatformTestVB.functions.getReplace(msg));
            Console.WriteLine();
            Console.WriteLine("Using Substr() - Left Extraction: " + msg.Substring(0,9));
            Console.WriteLine("Using Substr() - Right Extraction: " + msg.Substring(msg.Length-3));
            Console.WriteLine("Using Substr() - Mid Extraction: " + msg.Substring(5-1,3));
            Console.WriteLine("Using IndexOf() - Instr Extraction: " + (msg.IndexOf("str",3)+ 1).ToString());
            Console.WriteLine();
        }
    }
}
