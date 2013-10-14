using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
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
namespace DigitalAudio
    {
    class Program
        {
        public enum PitchConversion { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B };

        public static int numThreads = 1;
        public static int countForThread;

        public static WaveFile m_WaveIn;
        public static TimeFrequency timeFrequency;
        public static float[] m_PixelArray;
        public static MusicNote[] m_SheetMusic;
        public static double m_BeatsPerMinute = 70;
        public static Stopwatch timer = new Stopwatch();
        static bool isfin = true;

        static List<Thread> threadList = new List<Thread>();
        static List<float[]> wavelist = new List<float[]>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// 
        [STAThread]
        static void Main(string[] args)
        {
            do
            {
                Start();
                Console.WriteLine("Do you wish to run again? Yes or No");
                string ans = Console.ReadLine();
                if (ans == "Yes" || ans == "Y" || ans == "y" || ans == "yes")
                {
                    Start();
                }
                else
                {
                    isfin = false;
                    return;
                }
            } while (isfin);
            
        }


        /// <summary>
        /// writes to a test file...
        /// </summary>
        /// 

        static void WriteFile(float[] wave)
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
            string filename = DialogBox(false);
            string xmlfile = DialogBox(true);

            // Load the WAV file and create and populate a WaveFile object. Assign that object to m_WaveIn.
            LoadWave(filename);

            

            // Create a "Spectrum Analysis" type map of the WAV file and set m_PixelArray with the resulting data. (Currently unused)

            //writeFile(m_WaveIn.m_Wave);
            Console.WriteLine("The size of the wave file is:" + m_WaveIn.m_Wave.Count() + "\nEnter how many threads to use: ");
            string ans = Console.ReadLine();
            
            try
            {
                numThreads = int.Parse(ans); 
            }
            catch
            {
                Console.WriteLine("You need to enter again. Goodbye");
                Console.ReadKey();
                return;
            }
            timer = new Stopwatch();
            timer.Start();
        
            ThreadStart(m_WaveIn.m_Wave);
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

        public static void threadingProc(float[] Wave, int count)
        {
            
            Thread thread = new Thread(() =>
                {
                    FrequencyDomain(Wave);
                    Array.Copy(Wave, 0, m_WaveIn.m_Wave, count, countForThread);
                });
            threadList.Add(thread);
           

        }


        public static void ThreadStart(float[] waveFile)
        {

            threadList = new List<Thread>();
            countForThread = waveFile.Count() / numThreads;
            wavelist = new List<float[]>();

            //Split the array into numthreads
            int start = 0;
            for (int i = 0; i < numThreads; i++)
            {
                float[] temp = new float[countForThread];
                Array.Copy(m_WaveIn.m_Wave, start, temp, 0, countForThread);
                wavelist.Add(temp);
                start += countForThread;
            }

            //start threads
            start = 0;
            foreach (float[] temp in wavelist)
            {
                threadingProc(temp, start);
                start += countForThread;
            }
            foreach (Thread thread in threadList)
            {
                thread.Start();
            }

            foreach (Thread thread in threadList)
            {
                thread.Join();
            }

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

        public static string DialogBox(bool isXml)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            if (isXml)
            {
                openFileDialog.Filter = "XML File|*.xml";
                openFileDialog.Title = "Select a XML File";
            }
            else
            {
                openFileDialog.Filter = "Wave File|*.wav";
                openFileDialog.Title = "Select a Wave File";
            }
            

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            else return null;
        }
        }
    }
