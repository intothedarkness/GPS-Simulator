using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Utility
{
    /// <summary>
    /// Helpers for use with Microsoft XSLT processor
    /// </summary>
    static class XsltHelper
    {
        /// <summary>
        /// Transform the input xmlString to an output string using the given xslt string
        /// Throws exceptions so higher-level constructs can handle them
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="xsltString"></param>
        /// <returns></returns>
        public static string Transform(string xmlString, string xsltString)
        {
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings(false, true);

            // load the xslt
            using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
                xslTrans.Load(reader, settings, null);

            // load the input into a reader, create the output writer that dumps into a StringWriter
            StringWriter sw = new StringWriter();
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            using (XmlTextWriter writer = new XmlTextWriter(sw))
                xslTrans.Transform(reader, writer);

            // return the generated string
            return sw.ToString();
        }

        /// <summary>
        /// Transform the xml string using the given xslt string and write output to
        /// the given stream
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="xsltString"></param>
        /// <param name="stream"></param>
        public static void Transform(string xmlString, string xsltString, Stream stream)
        {
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings(false, true);

            // load the xslt
            using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
                xslTrans.Load(reader, settings, null);

            // load the input into a reader, create the output writer that dumps into a StringWriter
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Unicode))
            {
                writer.Settings.CloseOutput = false;
                xslTrans.Transform(reader, writer);
            }
        }
    }
}
