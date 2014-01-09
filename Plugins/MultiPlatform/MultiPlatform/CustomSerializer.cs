using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MultiPlatform
{
    // Static class for custom serialization
    public static class CustomSerializer
    {
        private const byte stringIdentifier = 0xFF;
        private const byte boolIdentifier = 0xFE;
        private const byte doubleIdentifier = 0xFD;

        private const byte stringEnder = 0x00;

        private const byte boolSize = 0x01;
        private const byte doubleSize = 0x08;


        public static void SerializeString(MemoryStream ms, string data)
        {
            byte[] identifier = new byte[] { CustomSerializer.stringIdentifier };
            byte[] serializedData = Encoding.UTF8.GetBytes(data);
            ms.Write(identifier, 0, identifier.Length);
            ms.Write(serializedData, 0, serializedData.Length);
            serializedData = new byte[] { CustomSerializer.stringEnder };
            ms.Write(serializedData, 0, serializedData.Length);
        }

        public static void SerializeDouble(MemoryStream ms, double data)
        {
            byte[] identifier = new byte[] { CustomSerializer.doubleIdentifier };
            byte[] serializedData = new byte[CustomSerializer.doubleSize] { 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] converted = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < converted.Length && i < serializedData.Length; i++)
                    serializedData[i] = converted[i];
            }
            else
            {
                for (int i = 0; i < converted.Length && i < serializedData.Length; i++)
                    serializedData[i] = converted[converted.Length - 1 - i];
            }
            ms.Write(identifier, 0, identifier.Length);
            ms.Write(serializedData, 0, serializedData.Length);
        }

        public static void SerializeBool(MemoryStream ms, bool data)
        {
            byte[] identifier = new byte[] { CustomSerializer.boolIdentifier };
            byte[] serializedData = new byte[CustomSerializer.boolSize] { 0 };
            byte[] converted = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < converted.Length && i < serializedData.Length; i++)
                    serializedData[i] = converted[i];
            }
            else
            {
                for (int i = 0; i < converted.Length && i < serializedData.Length; i++)
                    serializedData[i] = converted[converted.Length - 1 - i];
            }
            ms.Write(identifier, 0, identifier.Length);
            ms.Write(serializedData, 0, serializedData.Length);
        }



        public static string DeserializeString(byte[] data, ref int pos)
        {
            if (pos < 0 || pos >= data.Length)
                throw new ArgumentOutOfRangeException("pos");

            if (data[pos] != CustomSerializer.stringIdentifier)
                throw new IGridforceMessageDeserializeException("The referenced data array doesn't contain a string at position " + pos.ToString());

            int searchPos = pos;

            searchPos++;

            List<byte> byteList = new List<byte>();

            while (searchPos < data.Length && data[searchPos] != CustomSerializer.stringEnder)
            {
                byteList.Add(data[searchPos]);
                searchPos++;
            }

            if (data[searchPos] != CustomSerializer.stringEnder)
                throw new IGridforceMessageDeserializeException("The referenced data array doesn't contain a string at position " + pos.ToString());

            searchPos++;

            pos = searchPos;

            return Encoding.UTF8.GetString(byteList.ToArray(), 0, byteList.Count);
        }

        public static double DeserializeDouble(byte[] data, ref int pos)
        {
            if (pos < 0 || pos >= data.Length)
                throw new ArgumentOutOfRangeException("pos");

            if (data[pos] != CustomSerializer.doubleIdentifier || data.Length - pos < CustomSerializer.doubleSize)
                throw new IGridforceMessageDeserializeException("The referenced data array doesn't contain a double at position " + pos.ToString());

            pos++;

            byte[] byteArray = new byte[CustomSerializer.doubleSize];
            for (int i = 0; i < CustomSerializer.doubleSize; i++)
                byteArray[i] = data[pos + i];
            pos += CustomSerializer.doubleSize;

            if (!(BitConverter.IsLittleEndian))
            {
                Array.Reverse(byteArray);
                return BitConverter.ToDouble(byteArray, CustomSerializer.doubleSize - sizeof(double));
            }
            else
            {
                return BitConverter.ToDouble(byteArray, 0);
            }
        }

        public static bool DeserializeBool(byte[] data, ref int pos)
        {
            if (pos < 0 || pos >= data.Length)
                throw new ArgumentOutOfRangeException("pos");

            if (data[pos] != CustomSerializer.boolIdentifier || data.Length - pos < CustomSerializer.boolSize)
                throw new IGridforceMessageDeserializeException("The referenced data array doesn't contain a bool at position " + pos.ToString());

            pos++;

            byte[] byteArray = new byte[CustomSerializer.boolSize];
            for (int i = 0; i < CustomSerializer.boolSize; i++)
                byteArray[i] = data[pos + i];
            pos += CustomSerializer.boolSize;

            if (!(BitConverter.IsLittleEndian))
            {
                Array.Reverse(byteArray);
                return BitConverter.ToBoolean(byteArray, CustomSerializer.boolSize - sizeof(double));
            }
            else
            {
                return BitConverter.ToBoolean(byteArray, 0);
            }
        }
    }


    // Exception for deserializing IGridforceMessages
    public class IGridforceMessageDeserializeException : Exception
    {
        // Constructor
        public IGridforceMessageDeserializeException(string message)
            : base(message)
        {
        }
    }
}
