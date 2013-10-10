using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

/*
 This is a good site if you need information on the MusicXML file structure:
 * http://www.musicxml.com/UserManuals/MusicXML/Content/EL-MusicXML.htm
 * 
 * MuseScore is a nice (free!) application that can read and write MusicXML files; I'm not sure what your ultimate goals are for this program,
 * but if you need to create test files, or test-read a file that you created, it might be useful:
 * http://musescore.org/
 *
 * Here are a couple of articles that explain some of the theory behind the TimeFrequency functions (WARNING - Horribly advanced math involved):
 * http://en.wikipedia.org/wiki/Frequency_domain
 * http://en.wikipedia.org/wiki/Short-time_Fourier_transform
 *
 * This site may be useful if you need information on note frequencies:
 * http://inst.eecs.berkeley.edu/~ee20/sp97/demos/lec2/music.html
 * 
 * Here is a good explanation of how note frequencies are calculated:
 * http://www.phys.unsw.edu.au/jw/notes.html
 * 
 * NAudio is a very useful (and free!) audio management library useable in C#. It may help you to simplify some of your current code (WaveFile.cs) and make it very easy to add audio playback
 * or other advanced features later:
 * http://naudio.codeplex.com/
 * 
 * Here is a detailed description of the WAV file format:
 * http://www.sonicspot.com/guide/wavefiles.html
 */

// random comment
namespace digaudconsole
{
    class Program
    {
        public enum PitchConversion { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B };

        public static int numThreads = 4;
        public static int countForThread;
        long duration;
        public static WaveFile m_WaveIn;
        public static TimeFrequency timeFrequency;
        public static float[] m_PixelArray;
        public static MusicNote[] m_SheetMusic;
        public static double m_BeatsPerMinute = 70;
        public static Stopwatch timer = new Stopwatch();
        public static Stopwatch timer1 = new Stopwatch(); 


        public static float[] WaveArray1 = new float[] { };
        public static float[] WaveArray2 = new float[] { };
        public static float[] WaveArray3 = new float[] { };
        public static float[] WaveArray4 = new float[] { };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            

            

            Start();


        }


        /// <summary>
        /// Hmmm... starts the program...
        /// </summary>
        /// 

        static void writeFile(float[] wave)
        {
            string txtfile = "c:\\Users\\tv1\\Desktop\\test.txt";
            StreamWriter sw = new StreamWriter(txtfile);
            foreach (float a in wave)
            {
                sw.WriteLine(a);
            }
            sw.Close();
        }
        static void Start()
        {
            string filename = "jupiter.wav";
            string xmlfile = "jupiter.xml";

            // Load the WAV file and create and populate a WaveFile object. Assign that object to m_WaveIn.
            LoadWave(filename);
            
            

            // Create a "Spectrum Analysis" type map of the WAV file and set m_PixelArray with the resulting data. (Currently unused)
            
            //writeFile(m_WaveIn.m_Wave);
            Console.WriteLine("The size of the wave file is:" + m_WaveIn.m_Wave.Count() + "\nPress Return to continue:");
            Console.ReadLine();
            timer.Start();
            ArrayManip(m_WaveIn.m_Wave);
            threadstart();
            //FrequencyDomain(m_WaveIn.m_Wave);
            //calculate and display time
            long duration = timer.ElapsedMilliseconds;
            Console.Clear();
            Console.WriteLine("Finished processing: " + duration + " milliseconds");
            // Read and parse a MusicXML file.
            m_SheetMusic = ReadXML(xmlfile);

            
            Console.WriteLine("Process is Finished \nPress Enter to exit");
            Console.ReadKey();
        }

        public static void start(float[] Wave, int count)
        {
            timer1 = new Stopwatch();
            timer1.Start();
            var bw = new BackgroundWorker();

            // define the event handlers
            bw.DoWork += (sender, args) =>
            {
                // do your lengthy stuff here -- this will happen in a separate thread
                FrequencyDomain(WaveArray1);
                foreach (float a in Wave)
                {
                    m_WaveIn.m_Wave[count] = a;
                    count++;
                }

            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                }// if an exception occurred during DoWork,
                //    MessageBox.Show(args.Error.ToString());  // do your error handling here

                timer1.Stop();
                Console.WriteLine("This thread took: " + timer1.ElapsedMilliseconds);
                // Do whatever else you want to do after the work completed.
                // This happens in the main UI thread.

            };

            bw.RunWorkerAsync(); // starts the background worker

            // execution continues here in parallel to the background worker
        }
    

        public static void threadstart()
        {
            /*
            Thread thread = new Thread(() =>
                {
                    int count = 0;
                    FrequencyDomain(WaveArray1);
                    foreach (float a in WaveArray1)
                    {
                        m_WaveIn.m_Wave[count] = a;
                        count++;
                    }
                });
            thread.Start();
            Thread thread1 = new Thread(() =>
            {
                int count = countForThread;
                FrequencyDomain(WaveArray2);

                foreach (float a in WaveArray2)
                {
                    m_WaveIn.m_Wave[count] = a;
                    count++;
                }
            });
            thread1.Start();
            Thread thread2 = new Thread(() =>
            {
                int count = countForThread + countForThread;
                FrequencyDomain(WaveArray3);
                foreach (float a in WaveArray3)
                {
                    m_WaveIn.m_Wave[count] = a;
                    count++;
                }
            });
            thread2.Start();
            Thread thread3 = new Thread(() =>
            {
                int count = countForThread * 3;
                FrequencyDomain(WaveArray4);

                foreach (float a in WaveArray4)
                {
                    m_WaveIn.m_Wave[count] = a;
                    count++;
                }

            });
            thread3.Start();
            */
            start(WaveArray1, 0);
            start(WaveArray2, countForThread);
            start(WaveArray2, countForThread + countForThread);
            start(WaveArray2, countForThread + countForThread + countForThread);

        }

        public static void ArrayManip(float[] waveFile)
        {
            countForThread = waveFile.Count() / numThreads;
           /*
            float[] a, b, c, d;
            a = new float[countForThread];
            b = new float[countForThread];
            c = new float[countForThread];
            d = new float[countForThread];

            int start= 0;
            int finish = countForThread;
            int count = 0;
            for (int i = start; i < finish; i++)
            {
                a[count] = waveFile[i];
                count++;
            }

            start = finish;
            finish = finish + numThreads;
            count = 0;
            for (int i = start; i < finish; i++)
            {
                b[count] = waveFile[i];
            }
            start = finish;
            finish = finish + numThreads;
            count = 0;
            for (int i = start; i < finish; i++)
            {
                c[count] = waveFile[i];
            } start = finish;
            finish = finish + numThreads;
            count = 0;
            for (int i = start; i < finish; i++)
            {
                d[count] = waveFile[i];
            }

            WaveArray1 = a;
            WaveArray2 = b;
            WaveArray3 = c;
            WaveArray4 = d;
            */
            WaveArray1 = new float[countForThread];
            WaveArray2 = new float[countForThread];
            WaveArray3 = new float[countForThread];
            WaveArray4 = new float[countForThread];
            Array.Copy(m_WaveIn.m_Wave, 0, WaveArray1, 0, countForThread);
            Array.Copy(m_WaveIn.m_Wave, countForThread, WaveArray2, 0, countForThread);
            Array.Copy(m_WaveIn.m_Wave, countForThread * 2, WaveArray3, 0, countForThread);
            Array.Copy(m_WaveIn.m_Wave, countForThread * 3, WaveArray4, 0, countForThread);

        }
        /// <summary>
        /// Reads and parses a MusicXML file. The note data from the file is converted to data suitable for printing a score.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>An array of MusicNote objects. Each object represents one note in the MusicXML file.</returns>
        public static MusicNote[] ReadXML(string filename)
        {
            List<string> stepList = new List<string>(100);
            List<int> octaveList = new List<int>(100);
            List<int> durationList = new List<int>(100);
            List<int> alterList = new List<int>(100);
            MusicNote[] scoreArray;
            int noteCount = 0;
            bool isSharp;

            // Open the MusicXML file.
            FileStream xmlFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (xmlFileStream == null)
            {
                System.Console.Write("Failed to Open File!");
            }

            // Create an XmlTextReader from the MusicXML input file
            XmlTextReader xmlTextReader = new XmlTextReader(filename);

            // Initialize isFinished, set it to true in the 'while' loop when the file has been parsed
            bool isFinished = false;
            while (!isFinished)
            {
                isSharp = false;

                // Begin parsing the file. Skip each entry until we find a 'note' element or reach the end of the file.
                while ((!xmlTextReader.Name.Equals("note") || xmlTextReader.NodeType == XmlNodeType.EndElement) && !isFinished)
                {
                    xmlTextReader.Read();
                    if (xmlTextReader.ReadState == ReadState.EndOfFile)
                    {
                        isFinished = true;
                    }
                }

                xmlTextReader.Read();
                xmlTextReader.Read();
                if (xmlTextReader.Name.Equals("rest"))
                {
                    // Do nothing
                }
                else if (xmlTextReader.Name.Equals("pitch"))
                {
                    while (!xmlTextReader.Name.Equals("step"))
                    {
                        xmlTextReader.Read();
                    }

                    xmlTextReader.Read();
                    stepList.Add(xmlTextReader.Value);
                    while (!xmlTextReader.Name.Equals("octave"))
                    {
                        if (xmlTextReader.Name.Equals("alter") && xmlTextReader.NodeType == XmlNodeType.Element)
                        {
                            xmlTextReader.Read();
                            alterList.Add(int.Parse(xmlTextReader.Value));
                            isSharp = true;
                        }

                        xmlTextReader.Read();
                    }

                    xmlTextReader.Read();

                    if (!isSharp)
                    {
                        alterList.Add(0);
                    }

                    isSharp = false;
                    octaveList.Add(int.Parse(xmlTextReader.Value));
                    while (!xmlTextReader.Name.Equals("duration"))
                    {
                        xmlTextReader.Read();
                    }

                    xmlTextReader.Read();

                    durationList.Add(int.Parse(xmlTextReader.Value));

                    //System.Console.Out.Write("Note ~ Pitch: " + stepList[noteCount] + alterList[noteCount] + " Octave: " + octaveList[noteCount] + " Duration: " + durationList[noteCount] + "\n");

                    noteCount++;
                }
            }

            scoreArray = new MusicNote[noteCount];

            double c0 = 16.351625;  // 16.351625 "C" in Hertz

            // Iterate through the data we parsed from the MusicXML file, create and populate a MusicNote object for each note in the file.
            for (int noteNumber = 0; noteNumber < noteCount; noteNumber++)
            {
                // Convert the "letter name" of each note to a note number using the PitchConversion enum.
                int step = (int)Enum.Parse(typeof(PitchConversion), stepList[noteNumber]);

                // Calculate the frequency of the note (in Hertz), correct the frequency if the alterList element for this note is != 0
                double frequency = c0 * Math.Pow(2, octaveList[noteNumber]) * (Math.Pow(2, ((double)step + (double)alterList[noteNumber]) / 12));

                // Now create a new MusicNote object. Pass the frequency of the note and the calculated duration.
                // The duration is re-calculated to represent the number of samples per time interval. For example:
                // In "Jupiter.xml", the duration values are as follows,
                // 1=1/16
                // 2=1/8
                // 3=1/8 Dotted
                // 4=1/4
                //
                // For a sample rate of 44100 and a tempo of 70, a quarter note (duration = 4) would be converted to 37,800 which is the number of samples that equal a 1/4 note.
                // An 1/8 note would be converted to 18,900, etc.
                scoreArray[noteNumber] = new MusicNote(frequency, (double)durationList[noteNumber] * 60 * m_WaveIn.m_SampleRate / (4 * m_BeatsPerMinute));
            }

            return scoreArray;
        }


        /// <summary>
        /// This is used to create a "Spectrum Analysis" of the WAV, could be used to graph wave frequency over time.
        /// The array produced (m_PixelArray) is not currently being used by the application.
        /// </summary>
        public static void FrequencyDomain(float[] file)
        {
            // Create a new TimeFrequency object, passing the actual sound data (m_WaveIn.m_Wave) as an array of floats and setting Sample Window to 2048
            // The Sample Window tells the Fourier Transform function how many samples to aggregate into one result. The window of samples is analyzed to determine
            // the predominant frequency in that particular range of samples.
            timeFrequency = new TimeFrequency(file, 2048);

            m_PixelArray = new float[timeFrequency.m_TimeFrequencyData[0].Length * timeFrequency.m_WindowSampleSize / 2];

            for (int outerIndex = 0; outerIndex < timeFrequency.m_WindowSampleSize / 2; outerIndex++)
            {
                for (int innerIndex = 0; innerIndex < timeFrequency.m_TimeFrequencyData[0].Length; innerIndex++)
                {                  
                    m_PixelArray[outerIndex * timeFrequency.m_TimeFrequencyData[0].Length + innerIndex] = timeFrequency.m_TimeFrequencyData[outerIndex][innerIndex];
                }

            }
        }


        /// <summary>
        /// Loads the WAV file (filename) and parses relevant information (header, etc.)  into a WaveFile object. Sets member variable m_WaveIn.
        /// </summary>
        /// <param name="filename"></param>
        public static void LoadWave(string filename)
        {
            // Sound File
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (fileStream == null)
            {
                System.Console.Write("Failed to Open File!");
            }
            else
            {
                m_WaveIn = new WaveFile(fileStream);
            }
        }
    }
}
