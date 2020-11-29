using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace LeadSquaredSumit
{
    /// <summary>
    /// This is core file processing class which hold reference to bytechunk with chunk starting and chunk ending byte index within file.
    /// Class determines the number of words and characters with in specific chunk separately.
    /// This class meant to be use by Main class CustomFileProcessor only.
    /// </summary>
    class FilePointers
    {
        int StartByteIndex { get; set; }
        int EndByteIndex { get; set; }  
        
        public long NonEmptyCharCount { get; set; }//
        public long WordCount { get; set; }
        string filePath = string.Empty;
        Thread t1;
        Thread t2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex">Starting Byte index of chunk</param>
        /// <param name="endIndex">Ending Byte index of chunk</param>
        /// <param name="fileAddress">file to be operated</param>
        public FilePointers(int startIndex, int endIndex, string fileAddress)
        {
            StartByteIndex = startIndex;
            EndByteIndex = endIndex;
            NonEmptyCharCount = 0;
            WordCount = 0;
            filePath = fileAddress;            
            t1 = new Thread(SequenceCharCounter);
            t2 = new Thread(SequenceWordCounter);
        }


        void SequenceCharCounter()
        {
            int runningCount = 0;
            using (BinaryReader fs = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                fs.BaseStream.Seek(StartByteIndex, SeekOrigin.Begin);
                for (int runningPointer = StartByteIndex; runningPointer <= EndByteIndex; runningPointer++)
                {
                    byte[] ar = new byte[1];                    
                    int h = fs.Read(ar, 0, 1);
                    if (h <= 0)
                    {
                        break;
                    }
                    char[] c = System.Text.Encoding.UTF8.GetChars(ar);
                    //Avoid the empty char counting
                    if (c[0] != ' ' && c[0] != '\0')
                    {
                        runningCount++;
                    }
                }
            }
            this.NonEmptyCharCount = runningCount;
        }
        void SequenceWordCounter()
        {

            using (BinaryReader fs = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                fs.BaseStream.Seek(StartByteIndex, SeekOrigin.Begin);
                long runningwordCount = 0;
                int runningCount = 0;

                for (int runningPointer = StartByteIndex; runningPointer <= EndByteIndex; runningPointer++)
                {
                    byte[] ar = new byte[1];
                    int h = fs.Read(ar, 0, 1);
                    if (h == 0)
                    {
                        break;
                    }
                    char[] c = System.Text.Encoding.UTF8.GetChars(ar);
                    
                    //Consideration Made: a wors my be formed only either alphobet and/or number but not special characteers
                    if ((c[0] >= 97 && c[0] <= 122) || (c[0] >= 65 && c[0] <= 90) || (c[0] >= 48 && c[0] <= 57))
                    {
                        runningCount++;
                    }
                    else
                    {
                        if (runningCount > 0)
                        {
                            runningwordCount++;
                        }
                        runningCount = 0;

                        
                    }
                }
                if(runningCount>0)
                {
                    runningwordCount++;
                }
                this.WordCount = runningwordCount;
            }
        }
               
        public void ProcessFile()
        {
            t1.Start();            
            t2.Start();
        }
        public void Join()
        {
           t1.Join();
            t2.Join();
        }
    }

    /// <summary>
    /// This class Utilizes the FilePointers class and process the file. In this class we decide the number of chunks
    /// the file to be splited in to for processing async
    /// </summary>
    public class CustomFileProcessor
    {
        //int bytecount=File("").GetAttributes.bytecount
        int splitCorrectionFactor = 0;
        int splitCount;
        long totaByteCount;
        long NonEmptyCharCount;
        long wordCount;
        string filePath = string.Empty;
        public CustomFileProcessor( string FilePath)
        {
            FileInfo fn = new FileInfo(FilePath);
            this.totaByteCount = fn.Length;
            this.filePath = FilePath;
        }

        /// <summary>
        /// Starts the processing of file.
        /// </summary>
        /// <returns>Returns True if operation completed successfully otherwise false</returns>
        public bool ProcessFile()
        {
            bool flagProcessedSuccessfully = true;
            
            List<FilePointers> lstFileByteChunks = new List<FilePointers>();
            int chunkSize = 0;
            if(this.totaByteCount<=5000)
            {
                chunkSize = 100;
            }
            else if(this.totaByteCount <= 10000)
            {
                chunkSize = 5000;
            }
            else if (this.totaByteCount <= 100000)
            {
                chunkSize = 20000;
            }
            else if (this.totaByteCount <= 1000000)
            {
                chunkSize = 50000;
            }
            else if (this.totaByteCount <= 10000000)
            {
                chunkSize = 100000;
            }
            else
            {
                // chunkSize = 200000;
                chunkSize = 1100000;

            }

            try
            {
                //Create Collection of Byte Chunk Processors
                int counter = 0;
                for (int k = 0; k <= totaByteCount - 1; k += chunkSize+1)
                {
                    counter++;
                    FilePointers ptr = new FilePointers(k, k + chunkSize, filePath);
                    lstFileByteChunks.Add(ptr);

                    //Set Split Correction Factor
                    bool flagOneisBreaking = false;
                    bool flagTwoisBreaking = false;
                    byte[] bar = new byte[1];
                    BinaryReader fs = new BinaryReader(new FileStream(this.filePath, FileMode.Open, FileAccess.Read));
                    fs.BaseStream.Seek(k, SeekOrigin.Begin);
                    int r = fs.Read(bar, 0, 1);
                    char[] c = System.Text.Encoding.UTF8.GetChars(bar);
                    if((c[0]>=97 && c[0]<=122) || (c[0] >= 65 && c[0] <= 90) || (c[0] >= 48 && c[0] <= 57))
                    {
                        flagOneisBreaking = true;                        
                    }

                    if (k > 0)
                    {
                        byte[] par = new byte[1];
                        BinaryReader ps = new BinaryReader(new FileStream(this.filePath, FileMode.Open, FileAccess.Read));
                        ps.BaseStream.Seek(k - 1, SeekOrigin.Begin);
                        int p = ps.Read(par, 0, 1);
                        char[] pchar = System.Text.Encoding.UTF8.GetChars(par);
                        if ((pchar[0] >= 97 && pchar[0] <= 122) || (pchar[0] >= 65 && pchar[0] <= 90) || (pchar[0] >= 48 && pchar[0] <= 57))
                        {
                            flagTwoisBreaking = true;                            
                        }
                    }

                    if(flagOneisBreaking && flagTwoisBreaking)
                    {
                        splitCorrectionFactor++;
                    }
                }                
                //Now start processing chunks 1 by 1 asynchroneously
                foreach(FilePointers p in lstFileByteChunks)
                {
                    p.ProcessFile();
                }

                //Wait and join if any of the chunk still being executed
                foreach(FilePointers p in lstFileByteChunks)
                {
                    p.Join();
                }

                foreach(FilePointers p in lstFileByteChunks)
                {
                    this.NonEmptyCharCount += p.NonEmptyCharCount;
                    this.wordCount += p.WordCount;
                }
            }
            catch (Exception ex)
            {
                flagProcessedSuccessfully = false;
            }
            return flagProcessedSuccessfully;
        }

        public long GetNonEmptyCharCount()
        {
            return NonEmptyCharCount;
        }
        public long GetWordCount()
        {
            return this.wordCount - (splitCorrectionFactor);
        }
    }
}
