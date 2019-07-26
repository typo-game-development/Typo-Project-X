/*
    Author: Francesco Podda
    Date: 24/05/2019
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

namespace Tomba.Utilities
{
    /// <summary>
    /// Class used to serialize every class instance.
    /// </summary>
    public static class Serializer
    {

        #region " Settings "

        private static System.Runtime.Serialization.DataContractSerializerSettings _serializersettings = new DataContractSerializerSettings();
        private static XmlReaderSettings _xmlReaderSettings = new XmlReaderSettings();
        private static XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings();
        public static SerializationSettings Settings = new SerializationSettings();

        #endregion

        /// <summary>
        /// Settings used to serialize objects.
        /// </summary>
        public class SerializationSettings
        {
            public string RootDirectory = System.IO.Path.Combine("\\Exported", "\\BaseSettings", "\\Objects\\");
            public string FileExtension = ".vcobj";

            public bool SerializerReadOnlyTypes = true;
            public bool SerializerPreservedObjectReferences = false;
            public bool SerializerIgnoreExtensionDataObject = false;
            public bool XMLIndent = true;

            public bool XMLNewLineOnAttributes = true;
            public bool XMLOmitXmlDeclaration = false;
            public bool XMLCheckCharacters = true;

            public IEnumerable<Type> SerializerKnownTypes = null;
            public System.Text.Encoding XMLEncoding = System.Text.Encoding.UTF8;
        }

        /// <summary>
        /// Used to get unknown nested classes that are extending from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static System.Collections.Generic.List<Type> GetUnknownTypes<T>()
        {
            System.Collections.Generic.IEnumerable<Type> types = from assemblies in System.AppDomain.CurrentDomain.GetAssemblies()
                                                                 from type in assemblies.GetTypes()
                                                                 where (type.IsClass
                                                                             && (!type.IsGenericType)
                                                                             && (!type.IsSpecialName)
                                                                             && (!type.IsSealed)
                                                                             && (!type.IsAbstract)
                                                                             && (type.BaseType != null)
                                                                             && (type.IsSubclassOf(typeof(T))))
                                                                 select type;
            return types.ToList();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="objToSerialize"></param>
        /// <param name="headerComment"></param>
        /// <param name="encoding"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public static System.IO.MemoryStream SerializeToStream<T, O>(O objToSerialize, bool headerComment, System.Text.Encoding encoding, params Type[] knownTypes)
        {
            List<Type> newKnownTypes = new List<Type>();
            System.Text.StringBuilder fileHeaderComment = new System.Text.StringBuilder();
            DataContractSerializer ser;
            XmlWriter writer;
            System.IO.MemoryStream IOStream = new System.IO.MemoryStream();
            try
            {
                newKnownTypes = GetUnknownTypes<T>();
                newKnownTypes.AddRange(knownTypes);
                _xmlWriterSettings.Indent = true;
                _xmlWriterSettings.NewLineOnAttributes = true;
                _xmlWriterSettings.OmitXmlDeclaration = false;
                _xmlWriterSettings.CheckCharacters = true;
                _xmlWriterSettings.Encoding = encoding;
                _serializersettings.PreserveObjectReferences = true;
                _serializersettings.KnownTypes = newKnownTypes;
                writer = XmlWriter.Create(IOStream, _xmlWriterSettings);
                if (headerComment)
                {
                    fileHeaderComment.AppendLine("");
                    fileHeaderComment.AppendLine("############################################################################");
                    fileHeaderComment.AppendLine(("- Date: " + System.DateTime.Now.ToString("yyyy/MM/dd")));
                    fileHeaderComment.AppendLine(("- Time: " + System.DateTime.Now.ToString("HH:mm:ss")));
                    fileHeaderComment.AppendLine("");
                    fileHeaderComment.AppendLine("############################################################################");
                    // Write comment header to xml file
                    writer.WriteComment(fileHeaderComment.ToString());
                }
                ser = new DataContractSerializer(typeof(O).UnderlyingSystemType, _serializersettings);
                // Write serialized recipe to xml file
                ser.WriteObject(writer, objToSerialize);
                writer.Close();
                return IOStream;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="objToSerialize"></param>
        /// <param name="headerComment"></param>
        /// <param name="encoding"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public static byte[] SerializeToBytes<T, O>(O objToSerialize, bool headerComment, System.Text.Encoding encoding, params Type[] knownTypes)
        {
            return SerializeToStream<T, O>(objToSerialize, headerComment, encoding, knownTypes).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="objToSerialize"></param>
        /// <param name="headerComment"></param>
        /// <param name="encoding"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public static string SerializeToXML<T, O>(O objToSerialize, bool headerComment, System.Text.Encoding encoding, params Type[] knownTypes)
        {
            return encoding.GetString(SerializeToBytes<T, O>(objToSerialize, headerComment, encoding, knownTypes));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="objToSerialize"></param>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="headerComment"></param>
        /// <param name="encoding"></param>
        /// <param name="knownTypes"></param>
        public static void SerializeToFile<T, O>(O objToSerialize, string filePath, string fileName, bool headerComment, System.Text.Encoding encoding, params Type[] knownTypes)
        {
            List<Type> newKnownTypes = new List<Type>();
            System.Text.StringBuilder fileHeaderComment = new System.Text.StringBuilder();
            DataContractSerializer ser;
            newKnownTypes = GetUnknownTypes<T>();
            newKnownTypes.AddRange(knownTypes);
            _xmlWriterSettings.Indent = true;
            _xmlWriterSettings.NewLineOnAttributes = true;
            _xmlWriterSettings.OmitXmlDeclaration = false;
            _xmlWriterSettings.CheckCharacters = true;
            _xmlWriterSettings.Encoding = encoding;
            _serializersettings.PreserveObjectReferences = true;
            _serializersettings.KnownTypes = newKnownTypes;
            if (!System.IO.Directory.Exists(Settings.RootDirectory))
            {
                System.IO.Directory.CreateDirectory(Settings.RootDirectory);
            }

            if (!System.IO.Directory.Exists((Settings.RootDirectory + "\\Temp\\")))
            {
                System.IO.Directory.CreateDirectory((Settings.RootDirectory + "\\Temp\\"));
            }

            XmlWriter writer = XmlWriter.Create((Settings.RootDirectory + ("\\Temp\\" + ".tmp")), _xmlWriterSettings);
            if (headerComment)
            {
                // Generate comment header
                fileHeaderComment.AppendLine("");
                fileHeaderComment.AppendLine("############################################################################");
                fileHeaderComment.AppendLine(("- Date: " + System.DateTime.Now.ToString("yyyy/MM/dd")));
                fileHeaderComment.AppendLine(("- Time: " + System.DateTime.Now.ToString("HH:mm:ss")));
                fileHeaderComment.AppendLine("");
                fileHeaderComment.AppendLine("############################################################################");
                // Write comment header to xml file
                writer.WriteComment(fileHeaderComment.ToString());
            }

            ser = new DataContractSerializer(typeof(T), _serializersettings);
            // Write serialized recipe to xml file
            ser.WriteObject(writer, objToSerialize);
            writer.Close();
            // Dim checkSum As String = HUtilities.GetSHA256(Settings.RootDirectory & "\Temp\" & ".tmp")
            // IO.File.Move(Settings.RootDirectory & "\Temp\" & ".tmp", Settings.RootDirectory & checkSum & Settings.FileExtension)
            // IO.Directory.Delete(Settings.RootDirectory & "\Temp")
            // Return checkSum
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T1 DeserializeFromBytes<T, T1>(byte[] data)
        {
            List<Type> newKnownTypes = new List<Type>();
            System.IO.MemoryStream IOStream = new System.IO.MemoryStream(data);
            try
            {
                newKnownTypes = GetUnknownTypes<T>();
                _serializersettings.SerializeReadOnlyTypes = true;
                _serializersettings.IgnoreExtensionDataObject = false;
                _serializersettings.KnownTypes = newKnownTypes;
                _serializersettings.PreserveObjectReferences = true;
                _xmlReaderSettings.CheckCharacters = true;
                _xmlReaderSettings.IgnoreComments = true;
                XmlReader fs = XmlReader.Create(IOStream, _xmlReaderSettings);
                DataContractSerializer ser = new DataContractSerializer(typeof(T1), _serializersettings);
                //  Deserialize the data and read it from the instance.
                T1 deserializedClass = (T1)ser.ReadObject(fs);
                fs.Close();
                return deserializedClass;
                // Else
                // Throw New Exception("Invalid checksum: file is corrupted or has been modified from an external source.")
                // End If
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return default;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T1 DeserializeFromFile<T, T1>(string filePath)
        {
            List<Type> newKnownTypes = new List<Type>();
            string[] filePathSplit = filePath.Split(char.Parse("\\"));
            string fileName = filePathSplit[(filePathSplit.Count() - 1)].Split(char.Parse(".vcobj"))[0];
            try
            {
                // If HUtilities.GetSHA256(filePath) = fileName Then
                newKnownTypes = GetUnknownTypes<T>();
                _serializersettings.SerializeReadOnlyTypes = true;
                _serializersettings.IgnoreExtensionDataObject = false;
                _serializersettings.KnownTypes = newKnownTypes;
                _serializersettings.PreserveObjectReferences = true;
                _xmlReaderSettings.CheckCharacters = true;
                _xmlReaderSettings.IgnoreComments = true;
                XmlReader fs = XmlReader.Create(filePath, _xmlReaderSettings);
                DataContractSerializer ser = new DataContractSerializer(typeof(T1), _serializersettings);
                //  Deserialize the data and read it from the instance.
                T1 deserializedClass = (T1)ser.ReadObject(fs);
                fs.Close();
                return deserializedClass;
                // Else
                // Throw New Exception("Invalid checksum: file is corrupted or has been modified from an external source.")
                // End If
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return default;
            }
        }
    }
}

