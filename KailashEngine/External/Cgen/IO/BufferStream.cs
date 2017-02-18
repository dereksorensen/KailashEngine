#region License
// ================================================================================
// Cgen.Audio - Audio Submodules of CygnusJam Game Engine
// Copyright (C) 2015 Alghi Fariansyah (com@cxo2.me)
// 
// This software is provided 'as-is', without any express or implied
// warranty.In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software.If you use this software
//    in a product, an acknowledgement in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
// ================================================================================
#endregion

namespace Cgen.IO
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    /// <summary>
    /// Represent a Stream that using memory as the backing store.
    /// </summary>
    public class BufferStream : MemoryStream
    {
        private BinaryReader _reader;
        private BinaryWriter _writer;

        /// <summary>
        /// Gets or Sets the current position within the stream.
        /// </summary>
        public new long Position
        {
            get { return base.Position; }
            set { base.Position = value; }
        }

        /// <summary>
        /// Gets the length of the Stream in bytes.
        /// </summary>
        public new long Length
        {
            get { return base.Length; }
        }

        /// <summary>
        /// Gets a value indicating whether the current position of stream is less than the length of stream.
        /// </summary>
        public bool HasRemaining
        {
            get
            {
                int count = (int)(Length - Position);
                return count > 0;
            }
        }

        /// <summary>
        /// Construct a new <see cref="BufferStream"/>.
        /// </summary>
        public BufferStream()
            : base()
        {
            _reader = new BinaryReader(this);
            _writer = new BinaryWriter(this);
        }

        /// <summary>
        /// Construct a new <see cref="BufferStream"/>.
        /// </summary>
        /// <param name="data">Initial array of bytes for the stream.</param>
        public BufferStream(byte[] data)
            : base(data, true)
        {
            _reader = new BinaryReader(this);
            _writer = new BinaryWriter(this);
        }

        /// <summary>
        /// Returns the next available character and does not advance the byte or character position.
        /// </summary>
        /// <returns></returns>
        public int Peek()
        {
            return _reader.PeekChar();
        }

        /// <summary>
        /// Reads a boolean value from the current stream and advances the current position by one byte.
        /// </summary>
        public bool GetBool()
        {
            return _reader.ReadBoolean();
        }

        /// <summary>
        /// Writes a boolean value from the current stream and advances the current position by one byte.
        /// </summary>
        public void Write(bool value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads the next character from the current stream
        /// and advance the current position of the stream in accordance with the Encoding used
        /// and the specific character being read from the stream.
        /// </summary>
        /// <returns></returns>
        public char GetChar()
        {
            return _reader.ReadChar();
        }

        /// <summary>
        /// Writes a unicode characters to the current stream
        /// and advances the current position  of the stream in accordance with the Encoding used
        /// and the specific character being read from the stream.
        /// </summary>
        /// <param name="ch">The non-surrogate, Unicode charater to write.</param>
        public void Write(char ch)
        {
            _writer.Write(ch);
        }

        /// <summary>
        /// Reads count characters from the current stream,
        /// return data in character array,
        /// and advance the current position of the stream in accordance with the Encoding used
        /// and the specific character being read from the stream.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <returns></returns>
        public char[] GetChars(int count)
        {
            return _reader.ReadChars(count);
        }

        /// <summary>
        /// Writes a section of a character array to the current stream,
        /// and advances the current position of the stream in accordance with the Encoding used
        /// and perhaps the specific characters being written to the stream.
        /// </summary>
        /// <param name="chars">A characters array containing the data to write.</param>
        public void Write(char[] chars)
        {
            _writer.Write(chars);
        }

        /// <summary>
        /// Writes a section of a character array to the current stream,
        /// and advances the current position of the stream in accordance with the Encoding used
        /// and perhaps the specific characters being written to the stream.
        /// </summary>
        /// <param name="chars">A characters array containing the data to write.</param>
        /// <param name="index">The starting point in buffer at which to begin writing.</param>
        /// <param name="count">The number of characters to write.</param>
        public void Write(char[] chars, int index, int count)
        {
            _writer.Write(chars, index, count);
        }

        /// <summary>
        /// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer seven bits at a time.
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return _reader.ReadString();
        }

        /// <summary>
        /// Writes a length-prefixed to this stream with current encoding of the writer stream,
        /// and advance the current position of the stream in accordance with the Encoding used
        /// and the specific character being read from the stream.
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads count characters as a string with specified encoding and advances the current position by count characters.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <param name="encoding">The Encoding to decodes the array of bytes into a string, null to use default encoding.</param>
        /// <returns>Null terminated string.</returns>
        public string GetString(int count, Encoding encoding = default(Encoding))
        {
            byte[] charData = GetBytes(count);
            return encoding.GetString(charData);
        }

        /// <summary>
        /// Writes a string with specified encoding and advances the current position by count characters.
        /// </summary>
        /// <param name="value">The string to written.</param>
        /// <param name="encoding">The Encoding to decodes the array of bytes into a string, null to use default encoding.</param>
        public void Write(string value, Encoding encoding = default(Encoding))
        {
            byte[] strData = encoding.GetBytes(value);
            Write(strData);
        }

        /// <summary>
        /// Reads the next bytes from the current stream and advances the current position by one byte.
        /// </summary>
        /// <returns></returns>
        public byte GetByte()
        {
            return _reader.ReadByte();
        }

        /// <summary>
        /// Writes an unsigned byte to the current stream and advances the current position by one byte.
        /// </summary>
        /// <param name="value">The unsigned byte to write.</param>
        /// <returns></returns>
        public void Write(byte value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads the signed bytes from the current stream and advances the current position by one byte.
        /// </summary>
        /// <returns></returns>
        public sbyte GetSByte()
        {
            return _reader.ReadSByte();
        }

        /// <summary>
        /// Writes an signed byte to the current stream and advances the current position by one byte.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        /// <returns></returns>
        public void Write(sbyte value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads count bytes from the current stream into byte array and advances the current position by count bytes.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns></returns>
        public byte[] GetBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        /// <summary>
        /// Writes a byte array to the underlying stream.
        /// </summary>
        /// <param name="buffer">A byte array containing the data to write.</param>
        /// <returns></returns>
        public void Write(byte[] buffer)
        {
            _writer.Write(buffer);
        }

        /// <summary>
        /// Returns the next available bytes from the current position and does not advances the current position.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRemaining()
        {
            long current = Position;

            int count = (int)(Length - Position);
            byte[] remaining = GetBytes(count);
            Position = current;

            return remaining;
        }
    
        /// <summary>
        /// Reads a 2-bytes signed integer from the current stream and advances the current position by two bytes.
        /// </summary>
        /// <returns></returns>
        public short GetShort()
        {
            return _reader.ReadInt16();
        }

        /// <summary>
        /// Writes a 2-bytes signed integer from the current stream and advances the current position by two bytes.
        /// </summary>
        /// <param name="value">The 2-bytes signed integer to write.</param>
        public void Write(short value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 4-bytes signed integer from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <returns></returns>
        public int GetInteger()
        {
            return _reader.ReadInt32();
        }

        /// <summary>
        /// Writes a 4-bytes signed integer from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <param name="value">The 4-bytes signed integer to write.</param>
        public void Write(int value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 8-bytes signed integer from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <returns></returns>
        public long GetLong()
        {
            return _reader.ReadInt64();
        }

        /// <summary>
        /// Writes a 8-bytes signed integer from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <param name="value">The 8-bytes signed integer to write.</param>
        public void Write(long value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 2-bytes signed integer from the current stream and advances the current position by two bytes.
        /// </summary>
        /// <returns></returns>
        public short GetInt16()
        {
            return _reader.ReadInt16();
        }

        /// <summary>
        /// Reads a 4-bytes signed integer from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <returns></returns>
        public int GetInt32()
        {
            return _reader.ReadInt32();
        }

        /// <summary>
        /// Reads a 8-bytes signed integer from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <returns></returns>
        public long GetInt64()
        {
            return _reader.ReadInt64();
        }

        /// <summary>
        /// Reads a 2-bytes unsigned integer from the current stream and advances the current position by two bytes.
        /// </summary>
        /// <returns></returns>
        public ushort GetUShort()
        {
            return _reader.ReadUInt16();
        }

        /// <summary>
        /// Writes a 2-bytes unsigned integer from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <param name="value">The 2-bytes unsigned integer to write.</param>
        public void Write(ushort value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 4-bytes unsigned integer from the current stream and advances the current position by two bytes.
        /// </summary>
        /// <returns></returns>
        public uint GetUInteger()
        {
            return _reader.ReadUInt32();
        }

        /// <summary>
        /// Writes a 4-bytes unsigned integer from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <param name="value">The 4-bytes unsigned integer to write.</param>
        public void Write(uint value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 8-bytes unsigned integer from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <returns></returns>
        public ulong GetULong()
        {
            return _reader.ReadUInt64();
        }

        /// <summary>
        /// Writes a 8-bytes unsigned integer from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <param name="value">The 8-bytes unsigned integer to write.</param>
        public void Write(ulong value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 2-bytes unsigned integer from the current stream and advances the current position by two bytes.
        /// </summary>
        /// <returns></returns>
        public ushort GetUInt16()
        {
            return _reader.ReadUInt16();
        }

        /// <summary>
        /// Reads a 4-bytes unsigned integer from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <returns></returns>
        public uint GetUInt32()
        {
            return _reader.ReadUInt32();
        }

        /// <summary>
        /// Reads a 8-bytes unsigned integer from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <returns></returns>
        public ulong GetUInt64()
        {
            return _reader.ReadUInt64();
        }

        /// <summary>
        /// Reads a 4-bytes floating point value from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <returns></returns>
        public float GetFloat()
        {
            return _reader.ReadSingle();
        }

        /// <summary>
        /// Writes a 4-bytes floating point value from the current stream and advances the current position by four bytes.
        /// </summary>
        /// <param name="value">The 4-bytes floating point value to write.</param>
        public void Write(float value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a 8-bytes floating point value from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <returns></returns>
        public double GetDouble()
        {
            return _reader.ReadDouble();
        }

        /// <summary>
        /// Writes a 8-bytes floating point value from the current stream and advances the current position by eight bytes.
        /// </summary>
        /// <param name="value">The 8-bytes floating point value to write.</param>
        public void Write(double value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a decimal from the current stream and advances the current position by sixteen bytes.
        /// </summary>
        /// <returns></returns>
        public decimal GetDecimal()
        {
            return _reader.ReadDecimal();
        }

        /// <summary>
        /// Writes a decimal from the current stream and advances the current position by sixteen bytes.
        /// </summary>
        /// <param name="value">The decimal value to write.</param>
        public void Write(decimal value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to buffer.
        /// </summary>
        /// <param name="buffer">When this method return, contains specified array with the values between offset and (offset + count - 1) replaced by characters read from the current stream.</param>
        /// <param name="offset">The byte offset in buffer at which to begin reading.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes successfully read.</returns>
        public new int Read(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, count);
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public new void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes the stream contents to array of bytes, regardless of the <see cref="BufferStream.Position"/> property.
        /// </summary>
        /// <returns></returns>
        public new byte[] ToArray()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Release all resources used by <see cref="BufferStream"/>.
        /// </summary>
        public new void Dispose()
        {
            _reader.Close();
            _writer.Close();
            
            base.Dispose();
        }
    }
}
