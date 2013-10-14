using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DigitalAudio
{
    public class ThreadingTask
    {
        int numThreads;
        float[] waveFile;
        

        public ThreadingTask(float[] waveFile, int numThreads)
        {
            this.numThreads = numThreads;
            this.waveFile = waveFile;
        }

        public void test()
        {

        }
        private void ArrayManip()
        {
            int countForThread = waveFile.Count() / numThreads;
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
            System.ComponentModel.BackgroundWorker[] bwlist = new System.ComponentModel.BackgroundWorker[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                bwlist[i] = new System.ComponentModel.BackgroundWorker();

                // define the event handlers
                bwlist[i].DoWork += (sender, args) =>
                {
                    // do your lengthy stuff here -- this will happen in a separate thread
                    //FrequencyDomain(WaveArray1);

                };

                bwlist[i].RunWorkerCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                    }// if an exception occurred during DoWork,
                    //    MessageBox.Show(args.Error.ToString());  // do your error handling here

                    // Do whatever else you want to do after the work completed.
                    // This happens in the main UI thread.

                };

                bwlist[i].RunWorkerAsync();
            }
            /*
            WaveArray1 = new float[countForThread];
            WaveArray2 = new float[countForThread];
            WaveArray3 = new float[countForThread];
            WaveArray4 = new float[countForThread];
            Array.Copy(m_WaveIn.m_Wave, 0, WaveArray1, 0, countForThread);
            Array.Copy(m_WaveIn.m_Wave, countForThread, WaveArray2, 0, countForThread);
            Array.Copy(m_WaveIn.m_Wave, countForThread * 2, WaveArray3, 0, countForThread);
            Array.Copy(m_WaveIn.m_Wave, countForThread * 3, WaveArray4, 0, countForThread);
            */
        }

        public float[] start()
        {
            return null;
        }


    }
}
