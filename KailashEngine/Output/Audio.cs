using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;



namespace KailashEngine.Output
{
    class Audio
    {

        int _source;
        int _buffer;
        string _filename;
        AudioContext _context;


        public Audio(string filename)
        {
            _filename = filename;








        }

        public void playSound(Vector3 l_position, Vector3 l_dir, Vector3 l_up)
        {
            int state;

            //_context.MakeCurrent();

            _buffer = AL.GenBuffer();
            _source = AL.GenSource();

            int channels, bits_per_sample, sample_rate;
            byte[] sound_data = LoadWave(File.Open(_filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
            AL.BufferData(_buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);


            Console.WriteLine(AL.GetError().ToString());

            AL.Source(_source, ALSourcei.Buffer, _buffer);



            Vector3 s_pos = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 s_vel = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 s_dir = -s_pos;
            AL.Source(_source, ALSource3f.Position, ref s_pos);
            AL.Source(_source, ALSource3f.Velocity, ref s_vel);
            AL.Source(_source, ALSource3f.Direction, ref s_dir);
            AL.Source(_source, ALSourceb.SourceRelative, false);

            Vector3 s_temp_p;
            Vector3 s_temp_v;
            Vector3 s_temp_d;
            AL.GetSource(_source, ALSource3f.Position, out s_temp_p);
            AL.GetSource(_source, ALSource3f.Velocity, out s_temp_v);
            AL.GetSource(_source, ALSource3f.Direction, out s_temp_d);
            Console.WriteLine(s_temp_p + "\n" + s_temp_v + "\n" + s_temp_d);


            


            Vector3 l_vel = new Vector3();
            Vector3 L_dir = -l_dir;
            AL.Listener(ALListener3f.Position, ref l_position);
            AL.Listener(ALListenerfv.Orientation, ref L_dir, ref l_up);
            AL.Listener(ALListener3f.Velocity, ref l_vel);
            Vector3 l_temp_p;
            Vector3 l_temp_l;
            Vector3 l_temp_u;
            AL.GetListener(ALListener3f.Position, out l_temp_p);
            AL.GetListener(ALListenerfv.Orientation, out l_temp_l, out l_temp_u);
            Console.WriteLine("\t" + l_temp_p + "\n" + "\t" + l_temp_l + "\n" + "\t" + l_temp_u + "\n");




            AL.SourcePlay(_source);




            Trace.Write("Playing");

            //Query the _source to find out when it stops playing.
            do
            {
                Thread.Sleep(250);
                Trace.Write(".");
                AL.GetSource(_source, ALGetSourcei.SourceState, out state);
            }
            while ((ALSourceState)state == ALSourceState.Playing);



            Trace.WriteLine("");

            AL.SourceStop(_source);
            AL.DeleteSource(_source);
            AL.DeleteBuffer(_buffer);

        }


        // Loads a wave/riff audio file.
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                Console.WriteLine(data_signature);
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }




    }
}
