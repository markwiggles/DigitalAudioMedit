using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DigitalAudio
    {
    /// <summary>
    /// This class parses a standard WAV file and populates its member variables. It assumes a specific ordering of header information (which will work), but I would recommend using a library such as NAudio.
    /// In addition to simplifying your code, you would also have the ability to read and process multiple types of audio files if you'd like:
    /// http://naudio.codeplex.com/
    /// </summary>
    public class WaveFile
        {
        public float[] m_Wave; // The actual sound data as a FLOAT array
        public byte[] m_Data; // The actual sound data as a BYTE array
        public char[] m_ChunkID = new char[4];
        public int m_ChunkSize;
        public char[] m_Format = new char[4];
        public char[] m_SubChunk1ID = new char[4];
        public int m_SubChunk1Size;
        public char[] m_SubChunk2ID = new char[4];
        public int m_SubChunk2Size;
        public short m_AudioFormat;
        public short m_NumChannels;
        public int m_SampleRate;
        public int m_ByteRate;
        public short m_BlockAlign;
        public short m_BitsPerSample;

        public WaveFile( FileStream file )
            {
            BinaryReader binaryReader = new BinaryReader( file );

            // Pull out blocks of info for this WAV file sequentially and populate the member variables.
            m_ChunkID = binaryReader.ReadChars( 4 );
            m_ChunkSize = binaryReader.ReadInt32();
            m_Format = binaryReader.ReadChars( 4 );
            m_SubChunk1ID = binaryReader.ReadChars( 4 );
            m_SubChunk1Size = binaryReader.ReadInt32();
            m_AudioFormat = binaryReader.ReadInt16();
            m_NumChannels = binaryReader.ReadInt16();
            m_SampleRate = binaryReader.ReadInt32();
            m_ByteRate = binaryReader.ReadInt32();
            m_BlockAlign = binaryReader.ReadInt16();
            m_BitsPerSample = binaryReader.ReadInt16();
            m_SubChunk2ID = binaryReader.ReadChars( 4 );
            m_SubChunk2Size = binaryReader.ReadInt32();

            int sampleCount = m_SubChunk2Size / (m_BitsPerSample / 8);
            m_Data = new byte[sampleCount];
            m_Wave = new float[sampleCount];

            // Read the actual sound data as an array of BYTES
            m_Data = binaryReader.ReadBytes( sampleCount );

            // Convert the BYTE array to a FLOAT array
            // Each float will be in the range of -1.0 to 1.0 so,
            // BYTE     FLOAT
            // ----     -----
            // 0        -1.0
            // 128       0.0
            // 255       1.0    (technically, it will be .992, but close enough. 256 would be be exactly 1.0 but a byte ranges from 0-255 )
            for (int index = 0; index < sampleCount; index++)
                {
                m_Wave[index] = ((float)m_Data[index] - 128) / 128;
                }
            }
        }
    }
