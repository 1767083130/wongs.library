using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wongs.Common
{
    /// <summary>
    /// BinarySerialize class,provider basic Serialize and Deserialize method.
    /// </summary>
    public class BinarySerialize<T>
    {
        private string _strFilePath = string.Empty;

        public void Serialize(T obj, string strFilePath)
        {
            _strFilePath = strFilePath;
            FileInfo fi = new FileInfo(_strFilePath);
            //if (fi.Exists)
            //    throw new ArgumentException("File specified is exist already!");
            using (FileStream fs = new FileStream(_strFilePath, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// Deserialize an instance of T.
        /// </summary>
        /// <typeparam name="T">Any type.</typeparam>
        /// <returns>The result of deserialized.</returns>
        public T DeSerialize(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
                throw new ArgumentException("File specified is not exist!");
            T t;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    t = (T)formatter.Deserialize(fs);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return t;
        }
    }
    
}
