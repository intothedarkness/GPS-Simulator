using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Utility
{
    /// <summary>
    /// This class presents the Xml serializer and deserializer wrapped using 
    /// generic methods that introduce strong type checking.
    /// 
    /// This class also caches the serializers that were created so they can be re-used
    /// </summary>
    static class Serializer
    {
        static Serializer()
        {
            CacheSerializers = true;
        }

        #region Fields

        private static Dictionary<Type, XmlSerializer> m_serializers = new Dictionary<Type, XmlSerializer>();

        #endregion

        /// <summary>
        /// A flag indicating weather to cache the serializers.
        /// Serializers are cached by default. This means that any calls to
        /// Serialize or Deserialize will create a class, and this class will 
        /// be saved in a dictionary so that further calls to serialize or 
        /// deserialize the class will be more efficient
        /// </summary>
        public static bool CacheSerializers { get; set; }

        /// <summary>
        /// Create a serializer for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static XmlSerializer CreateSerializer(Type type)
        {
            return CreateSerializer(type, null);
        }

        /// <summary>
        /// Create a serializer for the given type
        /// If a property or field returns an array, the extraTypes parameter specifies objects that
        /// can be inserted into the array.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        private static XmlSerializer CreateSerializer(Type type, Type[] types)
        {
            if (m_serializers.ContainsKey(type))
            {
                Trace.TraceInformation("Retrieving serializer {0} from cache.", new object[] { type.FullName });
                return m_serializers[type];
            }

            Trace.TraceInformation("Creating serializer {0}.", new object[] { type.FullName });
            XmlSerializer ser = ((types == null) || (types.Length == 0)) ? new XmlSerializer(type) : new XmlSerializer(type, types);
            if (CacheSerializers)
            {
                Trace.TraceInformation("Adding serializer {0} to cache.", new object[] { type.FullName });
                m_serializers.Add(type, ser);
            }

            return ser;
        }

        private static XmlReaderSettings DefaultXmlReaderSettings()
        {
            return new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Document
            };
        }

        private static XmlWriterSettings DefaultXmlWriterSettings()
        {
            return new XmlWriterSettings()
            {
                Encoding = Encoding.Unicode,
                NewLineChars = Environment.NewLine,
                NewLineOnAttributes = false,
                NewLineHandling = NewLineHandling.Replace,
                Indent = true
            };
        }

        private static void CheckXML(string xmlString)
        {
            if (!xmlString.StartsWith("<") && !xmlString.EndsWith(">"))
                throw new ArgumentException("parameter is not xml", "xmlString");
        }

        #region deserializers

        /// <summary>
        /// Deserialize an object of the given type from the xml stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream) where T : class
        {
            return Deserialize<T>(stream, null);
        }

        /// <summary>
        /// Deserialize an instance of type T from the given xml string
        /// </summary>
        /// <typeparam name="T">the type to deserialize</typeparam>
        /// <param name="xmlString">the xml string</param>
        /// <returns>an instance of the type or NULL if any errors in the XML</returns>
        public static T Deserialize<T>(string xmlString) where T : class
        {
            // check if this is XML
            CheckXML(xmlString);

            using (StringReader stringReader = new StringReader(xmlString))
                return Deserialize<T>(stringReader, null, null);
        }

        /// <summary>
        /// Deserialize an instance of type T from the given XmlElement
        /// Caches the serializer so that further instances of this
        /// call will be more efficient.
        /// </summary>
        /// <typeparam name="T">the type to deserialize</typeparam>
        /// <param name="element">an XmlElement that contains the node to deserialize</param>
        /// <returns>an instance of T deserialized from the XmlElement</returns>
        public static T Deserialize<T>(XmlElement element) where T : class
        {
            return Deserialize<T>(element, null);
        }

        /// <summary>
        /// Deserialize an object of the given type from the stream.
        /// The extra types are used if the object is an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream, Type[] types) where T : class
        {
            return Deserialize<T>(stream, types, null);
        }

        /// <summary>
        /// Deserialize an object of a given type from the xml string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlString, Type[] types) where T : class
        {
            // check if this is XML
            CheckXML(xmlString);

            using (StringReader stringReader = new StringReader(xmlString))
                return Deserialize<T>(stringReader, types, null);
        }

        /// <summary>
        /// Deserialize an object of the given type from the stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            try
            {
                Trace.Assert(type != null);
                return CreateSerializer(type).Deserialize(stream);
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xmlString)
        {
            // check if this is XML
            CheckXML(xmlString);

            try
            {
                Trace.Assert(type != null);
                using (StringReader reader = new StringReader(xmlString))
                    return CreateSerializer(type).Deserialize(reader);
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// Serialize an object of the given type to a string
        /// </summary>
        /// <param name="type">Type to Deserialize</param>
        /// <param name="reader">XmlReader to use</param>
        /// <returns>string containing the serialized obejct</returns>
        public static object Deserialize(Type type, XmlReader reader)
        {
            try
            {
                Trace.Assert(type != null);
                return CreateSerializer(type).Deserialize(reader);
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static T Deserialize<T>(XmlElement element, Type[] types) where T : class
        {
            return Deserialize<T>(element.OuterXml, types);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="types"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static T Deserialize<T>(Stream stream, Type[] types, XmlReaderSettings settings) where T : class
        {
            T data = default(T);
            try
            {
                XmlSerializer ser = CreateSerializer(typeof(T), types);

                if (settings == null)
                    settings = DefaultXmlReaderSettings();

                using (XmlReader reader = XmlReader.Create(stream, settings))
                    data = ser.Deserialize(reader) as T;
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException("Exception when trying to deserialize from file: {0}", ex))
                {
                    throw;
                }
            }
            return data;
        }

        private static T Deserialize<T>(TextReader reader, Type[] types, XmlReaderSettings settings) where T : class
        {
            T data = default(T);
            try
            {
                XmlSerializer ser = CreateSerializer(typeof(T), types);
                if (settings == null)
                    settings = DefaultXmlReaderSettings();

                using (XmlReader xmlReader = XmlReader.Create(reader, settings))
                    data = ser.Deserialize(xmlReader) as T;
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException("Exception when trying to deserialize from file: {0}", ex))
                {
                    throw;
                }
            }
            return data;
        }

        /// <summary>
        /// This method deserializes the contents of a file into the object of the given type
        /// This method is used in preference to the base XmlSerializers because it's a generic 
        /// version that introduces strong type checking, and caches the serializers.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="fileName">file name to read</param>
        /// <returns>The deserialized object</returns>
        public static T DeserializeFromFile<T>(string fileName) where T : class
        {
            using (Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                return Deserialize<T>(stream, null, null);
        }

        #endregion

        #region serializers
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj) where T : class
        {
            return Serialize<T>(obj, null, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            try
            {
                Trace.Assert(obj != null);
                XmlSerializer serializer = CreateSerializer(obj.GetType());
                StringWriter sw = new StringWriter();
                serializer.Serialize((TextWriter)sw, obj);
                return sw.ToString();
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public static void Serialize<T>(Stream stream, T obj) where T : class
        {
            Serialize<T>(stream, obj, null, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public static void Serialize(Stream stream, object obj)
        {
            try
            {
                Trace.Assert(obj != null);
                CreateSerializer(obj.GetType()).Serialize(stream, obj);
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        public static void Serialize(XmlWriter writer, object obj)
        {
            try
            {
                Trace.Assert(obj != null);
                CreateSerializer(obj.GetType()).Serialize(writer, obj);
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, XmlSerializerNamespaces ns) where T : class
        {
            return Serialize<T>(obj, null, null, ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, XmlWriterSettings settings) where T : class
        {
            return Serialize<T>(obj, null, settings, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="ns"></param>
        public static void Serialize<T>(Stream stream, T obj, XmlSerializerNamespaces ns) where T : class
        {
            Serialize<T>(stream, obj, null, null, ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, XmlWriterSettings settings, XmlSerializerNamespaces ns) where T : class
        {
            return Serialize<T>(obj, null, settings, ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        public static void Serialize<T>(Stream stream, T obj, XmlWriterSettings settings) where T : class
        {
            Serialize<T>(stream, obj, null, settings, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <param name="ns"></param>
        public static void Serialize<T>(Stream stream, T obj, XmlWriterSettings settings, XmlSerializerNamespaces ns) where T : class
        {
            Serialize<T>(stream, obj, null, settings, ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="types"></param>
        /// <param name="settings"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        private static string Serialize<T>(object obj, Type[] types, XmlWriterSettings settings, XmlSerializerNamespaces ns) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("Object is null when trying to serialize");
            StringWriter sw = new StringWriter();
            try
            {
                XmlSerializer ser = CreateSerializer(typeof(T), types);
                if (settings == null)
                    settings = DefaultXmlWriterSettings();

                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    if (ns != null)
                        ser.Serialize(writer, obj, ns);
                    else
                        ser.Serialize(writer, obj);
                }
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }

            return sw.ToString();
        }

        private static void Serialize<T>(Stream stream, object obj, Type[] types, XmlWriterSettings settings, XmlSerializerNamespaces ns) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("Object is null when trying to serialize");
            try
            {
                XmlSerializer ser = CreateSerializer(typeof(T), types);
                if (settings == null)
                    settings = DefaultXmlWriterSettings();

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    if (ns != null)
                        ser.Serialize(writer, obj, ns);
                    else
                        ser.Serialize(writer, obj);
                }
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
        }

        private static void Serialize<T>(TextWriter writer, object obj, Type[] types, XmlWriterSettings settings, XmlSerializerNamespaces ns) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("Object is null when trying to serialize");

            try
            {
                XmlSerializer ser = CreateSerializer(typeof(T), types);
                if (settings == null)
                    settings = DefaultXmlWriterSettings();

                using (XmlWriter.Create(writer, settings))
                {
                    if (ns != null)
                        ser.Serialize(writer, obj, ns);
                    else
                        ser.Serialize(writer, obj);
                }
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static XmlElement SerializeToElementDirect(object obj)
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    CloseOutput = true,
                    Indent = true
                };

                string xmlString = Serialize(obj, settings);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);
                return doc.DocumentElement;
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static XmlElement SerializeToElement<T>(T obj) where T : class
        {
            return SerializeToElement<T>(obj, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static XmlElement SerializeToElement<T>(T obj, XmlSerializerNamespaces ns) where T : class
        {
            return SerializeToElement<T>(obj, ns, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ns"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static XmlElement SerializeToElement<T>(T obj, XmlSerializerNamespaces ns, Type[] types) where T : class
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    CloseOutput = true,
                    Indent = true
                };
                string xmlString = Serialize<T>(obj, types, settings, ns);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);
                return doc.DocumentElement;
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Serialize(object obj, XmlWriterSettings settings)
        {
            try
            {
                StringWriter writer = new StringWriter();

                XmlSerializer ser = CreateSerializer(obj.GetType());
                if (settings == null)
                    settings = DefaultXmlWriterSettings();

                using (XmlWriter.Create(writer, settings))
                {
                    //if (ns != null)
                    //    ser.Serialize(writer, obj, ns);
                    //else
                    ser.Serialize(writer, obj);
                }

                return writer.ToString();
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// Serialize the object to an element
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static XmlElement SerializeToElement(Type type, object obj)
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    CloseOutput = true,
                    Indent = true
                };

                StringWriter sw = new StringWriter();

                string xmlString = Serialize(obj, settings);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);
                return doc.DocumentElement;
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        public static void SerializeToFile<T>(T obj, string fileName) where T : class
        {
            SerializeToFile<T>(obj, fileName, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <param name="ns"></param>
        public static void SerializeToFile<T>(T obj, string fileName, XmlSerializerNamespaces ns) where T : class
        {
            SerializeToFile<T>(obj, fileName, null, ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <param name="settings"></param>
        public static void SerializeToFile<T>(T obj, string fileName, XmlWriterSettings settings) where T : class
        {
            SerializeToFile<T>(obj, fileName, settings, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <param name="settings"></param>
        /// <param name="ns"></param>
        public static void SerializeToFile<T>(T obj, string fileName, XmlWriterSettings settings, XmlSerializerNamespaces ns) where T : class
        {
            try
            {
                XmlSerializer serializer = CreateSerializer(typeof(T));
                if (settings == null)
                    settings = DefaultXmlWriterSettings();

                using (XmlWriter writer = XmlWriter.Create(fileName, settings))
                {
                    if (ns != null)
                        serializer.Serialize(writer, obj, ns);
                    else
                        serializer.Serialize(writer, obj);
                }
            }
            catch (Exception ex)
            {
                if (ExceptionManager.CriticalException(ex))
                    throw;
            }
        }
        #endregion
    }
       /// <summary>
    /// Generic way to handle exceptions
    /// </summary>
    public static class ExceptionManager
    {
        /// <summary>
        /// Indicates if this class should re-throw exceptions
        /// </summary>
        public static bool Throw { get; set; }

        /// <summary>
        /// Logs the critical exception, and optionally re-throws it
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool CriticalException(Exception ex)
        {
            Trace.TraceError(ex.ToString());
            if (Throw)
                throw ex;
            return false;
        }

        /// <summary>
        /// Logs the critical exception, and optionally re-throws it
        /// </summary>
        /// <param name="formatString"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool CriticalException(string formatString, Exception ex)
        {
            Trace.TraceError(formatString, ex);
            if (Throw)
                throw ex;
            return false;
        }
    }

}
