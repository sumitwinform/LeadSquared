using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LeadSquaredSumit
{
    class Program
    {
        static void Main(string[] args)
        {
            //*****
            //For Testing: Give your desired heavy text file path here            
            string fileName = @"wordCollection12500W2500.txt";
            //*****

            CustomFileProcessor file = new CustomFileProcessor(fileName);
            
            DateTime dtStart = DateTime.Now;
            Console.WriteLine("Start AT: " + DateTime.Now.ToLongTimeString());

            FileStream fs = new FileStream(fileName, FileMode.Open);
            string size = fs.Length.ToString();
            fs.Close();
            Console.WriteLine("File Size: " + size + " Bytes");            
            file.ProcessFile();
            string charCount = file.GetNonEmptyCharCount().ToString();
            string wordcount = file.GetWordCount().ToString();
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine("Total Char Count = " + charCount);
            Console.WriteLine("Total Word Count = " + wordcount);
            Console.WriteLine("");
            Console.WriteLine("END AT: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("");
            Console.WriteLine("Processing Time Taken :" + (dtEnd - dtStart).TotalSeconds + " Seconds");

            //Hold down to see the result.
            Console.Read();
        }


    }

}
