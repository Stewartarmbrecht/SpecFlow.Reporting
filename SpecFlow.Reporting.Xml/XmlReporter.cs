﻿using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace SpecFlow.Reporting.Xml
{
    public class XmlReporter : Reporter
    {
        public XmlWriterSettings XmlWriterSettings { get; private set; }

        public string XsltFile { get; set; }

        public XmlReporter()
        {
            XmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };
        }

        public override void WriteToStream(Stream stream)
        {
            if (string.IsNullOrEmpty(XsltFile))
            {
                InternalWriteToStream(stream);
            }
            else
            {
                WriteTransformedToStream(stream, XsltFile);
            }
        }

        private void InternalWriteToStream(Stream stream)
        {
            using (var writer = XmlTextWriter.Create(stream, XmlWriterSettings))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(Report.GetType());
                serializer.Serialize(writer, Report);
            };
        }

        protected virtual void WriteTransformedToStream(Stream outputStream, string xsltFile)
        {
            using (var inputStream = new MemoryStream())
            {
                InternalWriteToStream(inputStream);
                inputStream.Seek(0, SeekOrigin.Begin);

                using (var reader = XmlReader.Create(inputStream))
                {
                    var xslt = new XslCompiledTransform();
                    xslt.Load(xsltFile);

                    // Create the writer.
                    using (var writer = XmlWriter.Create(outputStream, XmlWriterSettings))
                    {
                        xslt.Transform(reader, writer);
                    }
                }
            }
        }
    }
}