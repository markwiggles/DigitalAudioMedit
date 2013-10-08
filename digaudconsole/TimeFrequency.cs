using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace digaudconsole
{
    /// <summary>
    /// This class performs a Time/Frequency analysis of audio data passed in as an array of floats using a Fast Fourier Transformation algorithm.
    /// </summary>
    public class TimeFrequency
    {
        public float[][] m_TimeFrequencyData;
        public int m_WindowSampleSize;
        private Complex[] m_Twiddles;

        /// <summary>
        /// Initialize the object
        /// </summary>
        /// <param name="waveDataArray">The actual sound data as an array of floats between -1.0 and 1.0</param>
        /// <param name="windowSampleSize">The number of sound samples to aggregate into one result</param>
        public TimeFrequency(float[] waveDataArray, int windowSampleSize)
        {
            Complex imaginaryOne = Complex.ImaginaryOne;
            m_WindowSampleSize = windowSampleSize;

            // Twiddles are used in the FFT to repeatedly perform an operation on each element of an array using a pre-calculated series of values.
            // Like most other parts of the algorithm, just assume they're there for some magical purpose :)
            m_Twiddles = new Complex[m_WindowSampleSize];

            for (int sampleIndex = 0; sampleIndex < m_WindowSampleSize; sampleIndex++)
            {
                double a = 2 * Math.PI * sampleIndex / (double)m_WindowSampleSize;  // Replaced the defined constant with the built-in. More accurate :)
                m_Twiddles[sampleIndex] = Complex.Pow(Complex.Exp(-imaginaryOne), (float)a);
            }

            m_TimeFrequencyData = new float[m_WindowSampleSize / 2][];

            // 'nearestSampleWindowCount' is an int that represents a rounded-up value of how many "Sample Windows" the sound data will be divided in to.
            // In the case of "Jupiter.wav", the sound data in the WAV file is 2,382,848 bytes. With a Sample Window of 2048 bytes, we'll have 1164 blocks of data.
            int nearestSampleWindowCount = (int)Math.Ceiling((double)waveDataArray.Length / (double)m_WindowSampleSize);

            // Now we multiply that rounded value by the Window Sample size to get a nice tidy length for the array. In this case, 2,383,872  - Slightly larger
            // than the actual sound data length, so the sound data is guaranteed to fit in the array.
            int complexDataArraySize = nearestSampleWindowCount * m_WindowSampleSize;

            // Create the complexDataArray and copy the wave data in to it.
            Complex[] complexDataArray = new Complex[complexDataArraySize];
            for (int index = 0; index < complexDataArraySize; index++)
            {
                if (index < waveDataArray.Length)
                {
                    complexDataArray[index] = waveDataArray[index];
                }
                else
                {
                    // If the complexDataArray is larger than the actual wave data, just fill the extra space with zeroes.
                    complexDataArray[index] = Complex.Zero;
                }
            }

            // Create the second dimension of the m_TimeFrequencyData array. With our default settings ("Jupiter.wav", etc.)
            // the m_TimeFrequencyData array will contain 1024 outer floats, and each of those will have an array of 2328 floats: m_TimeFrequencyData[1024][2328]
            // This "grid" will store the Time/Frequency data of "Jupiter.wav"
            int columns = 2 * complexDataArraySize / m_WindowSampleSize;
            for (int index = 0; index < m_WindowSampleSize / 2; index++)
            {
                m_TimeFrequencyData[index] = new float[columns];
            }

            // Now start the Voodoo math :)
            m_TimeFrequencyData = ShortTimeFourierTransform(complexDataArray, m_WindowSampleSize);
        }


        /// <summary>
        /// This will determine the frequency and phase of the WAV audio data (Complex[] complexDataArray)
        /// </summary>
        /// <param name="sourceComplexDataArray">An array of Complex objects representing the wave data</param>
        /// <param name="windowSampleSize">The number of sound samples to aggregate into one result - NOTE: this is a Class variable (m_WindowSampleSize), so it isn't necessary to pass it to the function</param>
        /// <returns>A 2-dimensional array representing frequencies present in the wave over time</returns>
        float[][] ShortTimeFourierTransform(Complex[] sourceComplexDataArray, int windowSampleSize)
        {
            int sourceComplexDataArrayLength = sourceComplexDataArray.Length;
            float fftMax = 0;

            float[][] finalTransformedArray = new float[windowSampleSize / 2][];

            for (int index = 0; index < windowSampleSize / 2; index++)
            {
                finalTransformedArray[index] = new float[2 * (int)Math.Floor((double)sourceComplexDataArrayLength / (double)windowSampleSize)];
            }

            Complex[] untransformedComplexArray = new Complex[windowSampleSize];
            Complex[] tempTransformedComplexArray = new Complex[windowSampleSize];

            Console.WriteLine((2 * Math.Floor((double)sourceComplexDataArrayLength / (double)windowSampleSize) - 1));
            Console.ReadLine();
            //Todo threading here
            for (int windowIndex = 0; windowIndex < 2 * Math.Floor((double)sourceComplexDataArrayLength / (double)windowSampleSize) - 1; windowIndex++)
            {
                for (int windowSampleIndex = 0; windowSampleIndex < windowSampleSize; windowSampleIndex++)
                {
                    untransformedComplexArray[windowSampleIndex] = sourceComplexDataArray[windowIndex * (windowSampleSize / 2) + windowSampleIndex];
                }

                tempTransformedComplexArray = FastFourierTransformation(untransformedComplexArray);
                // up to here
                for (int windowSampleIndex = 0; windowSampleIndex < windowSampleSize / 2; windowSampleIndex++)
                {
                    finalTransformedArray[windowSampleIndex][windowIndex] = (float)Complex.Abs(tempTransformedComplexArray[windowSampleIndex]);

                    if (finalTransformedArray[windowSampleIndex][windowIndex] > fftMax)
                    {
                        fftMax = finalTransformedArray[windowSampleIndex][windowIndex];
                    }
                }
            }
            Console.WriteLine("Blah");
            Console.ReadLine();
             
            for (int windowIndex = 0; windowIndex < 2 * Math.Floor((double)sourceComplexDataArrayLength / (double)windowSampleSize) - 1; windowIndex++)
            {
                for (int windowSampleIndex = 0; windowSampleIndex < windowSampleSize / 2; windowSampleIndex++)
                {
                    finalTransformedArray[windowSampleIndex][windowIndex] /= fftMax;
                }
            }

            return finalTransformedArray;
        }


        /// <summary>
        /// Calculates Time/Frequency of the wave data
        /// </summary>
        /// <param name="transformedDataArray">A prepared array containing wave data</param>
        /// <returns>The transformed array of frequencies over time</returns>
        Complex[] FastFourierTransformation(Complex[] transformedDataArray)
        {
            int transformedDataArrayLength = transformedDataArray.Length;

            Complex[] finalComplexDataArray = new Complex[transformedDataArrayLength];

            // NEED TO MEMSET TO ZERO? (*No, each element in the array will be initialized to zero. If you ever rewrite this in C, then yes :)

            if (transformedDataArrayLength == 1)
            {
                finalComplexDataArray[0] = transformedDataArray[0];
            }
            else
            {
                Complex[] evenTransformedComplexArray = new Complex[transformedDataArrayLength / 2];
                Complex[] oddTransformedComplexArray = new Complex[transformedDataArrayLength / 2];
                Complex[] evenTempComplexArray = new Complex[transformedDataArrayLength / 2];
                Complex[] oddTempComplexArray = new Complex[transformedDataArrayLength / 2];

                for (int index = 0; index < transformedDataArrayLength; index++)
                {
                    if (index % 2 == 0)
                    {
                        evenTempComplexArray[index / 2] = transformedDataArray[index];
                    }

                    if (index % 2 == 1)
                    {
                        oddTempComplexArray[(index - 1) / 2] = transformedDataArray[index];
                    }
                }

                evenTransformedComplexArray = FastFourierTransformation(evenTempComplexArray);
                oddTransformedComplexArray = FastFourierTransformation(oddTempComplexArray);

                for (int index = 0; index < transformedDataArrayLength; index++)
                {
                    finalComplexDataArray[index] = evenTransformedComplexArray[(index % (transformedDataArrayLength / 2))] + oddTransformedComplexArray[(index % (transformedDataArrayLength / 2))] * m_Twiddles[index * m_WindowSampleSize / transformedDataArrayLength];
                }
            }

            return finalComplexDataArray;
        }
    }
}
