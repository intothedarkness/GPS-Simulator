using System;
using System.IO;
using System.Xml;
using SharpGpx.Properties;
using Utility;

namespace SharpGpx
{
    /// <summary>
    /// parent class for GPX file format
    /// </summary>
    public class GpxClass : GPX1_1.gpxType
    {
        /// <summary>
        /// 
        /// </summary>
        public GpxClass()
        {
        }

        /// <summary>
        /// Create this class from the child class
        /// </summary>
        /// <param name="gpx"></param>
        public GpxClass(GPX1_1.gpxType gpx)
        {
            this.creator = gpx.creator;
            this.extensions = gpx.extensions;
            this.metadata = gpx.metadata;
            this.version = gpx.version;
            this.rte = new GPX1_1.rteTypeCollection(gpx.rte);
            this.trk = new GPX1_1.trkTypeCollection(gpx.trk);
            this.wpt = new GPX1_1.wptTypeCollection(gpx.wpt);
        }

        /// <summary>
        /// Parse an XML string and return an object model that has the contents of the
        /// GPX file
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static GpxClass FromXml(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            string xml = xmlString;

            if (doc.DocumentElement != null && doc.DocumentElement.NamespaceURI == "http://www.topografix.com/GPX/1/0")
                xml = XsltHelper.Transform(xmlString, Resources.gpx10to11);

            GPX1_1.gpxType gpx = Serializer.Deserialize<GPX1_1.gpxType>(xml);
            return new GpxClass(gpx);

        }

        /// <summary>
        /// Load and parse the given GPS file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static GpxClass FromFile(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
                return FromXml(reader.ReadToEnd());
        }

        /// <summary>
        /// Read Gpx from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static GpxClass FromStream(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
                return FromXml(reader.ReadToEnd());
        }

        internal GPX1_1.gpxType ToGpx1_1()
        {
            return new GPX1_1.gpxType()
            {
                creator = this.creator,
                extensions = this.extensions,
                metadata = this.metadata,
                version = this.version,
                rte = this.rte == null ? new GPX1_1.rteTypeCollection() : new GPX1_1.rteTypeCollection(this.rte),
                trk = this.trk == null ? new GPX1_1.trkTypeCollection() : new GPX1_1.trkTypeCollection(this.trk),
                wpt = this.wpt == null ? new GPX1_1.wptTypeCollection() : new GPX1_1.wptTypeCollection(this.wpt),
            };

        }

        // TODO: NOT WORKING
        internal GPX1_0.gpx ToGpx1_0()
        {
            return new GPX1_0.gpx()
            {
                creator = this.creator,
                version = this.version,
                //rte = new GPX1_0.gpxRteCollection(this.rte),
                //trk = new GPX1_0.gpxTrkCollection(this.trk),
                //wpt = new GPX1_0.gpxWptCollection(this.wpt),
            };

        }

        /// <summary>
        /// Save the GPX file to a string, using the given version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string ToXml(GpxVersion version)
        {
            string xmlString = Serializer.Serialize<GPX1_1.gpxType>(this.ToGpx1_1());
            return
                (version == GpxVersion.GPX_1_0) ?
                XsltHelper.Transform(xmlString, Resources.gpx11to10) :
                xmlString;
        }

        /// <summary>
        /// Save the GPX file to a string with the most recent version
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            return ToXml(GpxVersion.GPX_1_1);
        }

        /// <summary>
        /// Save the GPX file to a stream
        /// </summary>
        /// <param name="stream"></param>
        public void ToStream(Stream stream)
        {
            ToStream(stream, GpxVersion.GPX_1_1);
        }

        /// <summary>
        /// Save the GPX file to a stream in the given version
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="version"></param>
        private void ToStream(Stream stream, GpxVersion version)
        {
            if (version == GpxVersion.GPX_1_1)
                Serializer.Serialize<GPX1_1.gpxType>(stream, this.ToGpx1_1());
            else
            {
                XsltHelper.Transform(
                    Serializer.Serialize<GPX1_1.gpxType>(this.ToGpx1_1()),
                    Resources.gpx11to10,
                    stream);
            }
        }

        /// <summary>
        /// write to a file
        /// </summary>
        /// <param name="fileName"></param>
        public void ToFile(string fileName)
        {
            using (FileStream stream = File.OpenWrite(fileName))
                ToStream(stream, GpxVersion.GPX_1_1);
        }

        /// <summary>
        ///Check tge verion of the file and return true if it is 
        ///XML and is a version of this file we can read.
        /// </summary>
        /// <param name="xmlFileName"></param>
        /// <returns></returns>
        public static bool CheckVersion(string xmlFileName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFileName);

                return (doc.DocumentElement != null &&
                   (doc.DocumentElement.NamespaceURI == "http://www.topografix.com/GPX/1/0" ||
                     doc.DocumentElement.NamespaceURI == "http://www.topografix.com/GPX/1/1"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error reading {0}: file is not xml:\r\n{1}",
                    xmlFileName, ex);
                return false;
            }
        }

    }
}
