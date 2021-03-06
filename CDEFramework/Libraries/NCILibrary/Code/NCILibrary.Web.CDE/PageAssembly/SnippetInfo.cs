﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Common.Logging;

namespace NCI.Web.CDE
{
    /// <summary>
    /// Holds the snippet information used to populate a slot on the page.
    /// </summary>
    public class SnippetInfo : IXmlSerializable
    {
        static ILog log = LogManager.GetLogger(typeof(SnippetInfo));

        private string _snippetData = null;
        private Dictionary<string, string> _configStrings = null;
        private bool correctedCDATA = false;
        private List<DisplayVersions> listOnlyDisplayFor = new List<DisplayVersions>();
        private SnippetInfoCollection _snippets;

        /*
         * Serializer for use with nested occurences of the Snippets element. (Used with sub-layouts.)
         * 
         * Creating an XmlSerializer object using the XmlSerializer(Type, XmlRootAttribute) overload results in a temporary assembly
         * being created. Assemblies are not garbage collected until the application shuts down.  (The framework caches the assembly
         * internally for simpler overloads.)
         */
        private static XmlSerializer nestedSnippetSerializer = new XmlSerializer(typeof(SnippetInfoCollection),new XmlRootAttribute("Snippets"));

        /// <summary>
        /// Gets and sets the path to the user control that will render this
        /// snippet.
        /// </summary>
        public string SnippetTemplatePath { get; set; }
        /// <summary>
        /// html data to be displayed on the page
        /// </summary>
        public string Data 
        {
            get 
            {
                // The snippet CDATA may contain CDATA as part of the data but percussion replaces the CDATA 
                // close tag with Replace ']]>' with ']]ENDCDATA' this ']]ENDCDATA' should be replaced with 
                // valid CDATA close tag ']]>' before it can be deserialized
                if (!correctedCDATA && !string.IsNullOrEmpty(_snippetData))
                {
                    _snippetData = _snippetData.Replace("]]ENDCDATA", "]]>");
                    correctedCDATA = true;
                }
                return _snippetData; 
            }
            set { _snippetData = value; } 
        }
        /// <summary>
        /// Dictionary (string, string) of config strings
        /// </summary>
        public Dictionary<string, string> ConfigStrings
        {
            get
            {
                return _configStrings;
            }
        }
        /// <summary>
        /// Slot to be used on the page rendered
        /// </summary>
        public string SlotName { get; set; }

        public string ContentID { get; set; }

        public DisplayVersions[] OnlyDisplayFor
        {
            get
            {

                return listOnlyDisplayFor.ToArray();
            }
        }

        /// <summary>
        /// Gets the collections of the snippets
        /// </summary>
        /// <value>The snippet infos.</value>
        [System.Xml.Serialization.XmlArray(ElementName = "Snippets", Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItem("SnippetInfo", Form = XmlSchemaForm.Unqualified)]
        public SnippetInfoCollection SnippetInfos
        {
            get 
            { 
                return _snippets; 
            }
        }

        /// <summary>
        /// A collection of SnippetInfo objects for the sub-layout control which are needed
        /// to render a sub-layout.
        /// </summary>
        /// <value></value>
        [System.Xml.Serialization.XmlIgnore()]
        public IEnumerable<SnippetInfo> Snippets
        {
            get
            {
                List<SnippetInfo> snippets = new List<SnippetInfo>();

                // Add all local snippets to the list to return.
                foreach (SnippetInfo snipt in _snippets)
                {
                    if (snipt.OnlyDisplayFor.Count() == 0 || snipt.OnlyDisplayFor.Contains(PageAssemblyContext.Current.DisplayVersion))
                    {
                        snippets.Add(snipt);
                    }
                }
                return snippets;
            }
        }

        ///// <summary>
        ///// The id of the CDR definition. 
        ///// </summary>
        //public string CDRId { get; set; }

        ///// <summary>
        ///// The name of the CDR Definition.
        ///// </summary>
        //public string CDRDefinitionName { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            SnippetInfo target = obj as SnippetInfo;

            if (target == null)
                return false;

            if (ContentID != target.ContentID)
                return false;

            if (SnippetTemplatePath != target.SnippetTemplatePath)
                return false;

            if (SlotName != target.SlotName)
                return false;

            if (Data != target.Data)
                return false;

            return true;
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            while (reader.Read())
            {
                if ((reader.LocalName == "SnippetInfo") && (reader.IsStartElement() == false)) { reader.Read(); break; }

                switch (reader.LocalName)
                {
                    case "Data":
                        {
                            Data = reader.ReadString();
                        }
                        break;
                    case "ConfigTextItem":
                        {
                            if (_configStrings == null)
                                _configStrings = new Dictionary<string,string>();

                            if (reader.GetAttribute("Name") == null)
                            {
                                log.Error("Invalid ConfigTextItem - Name cannot be empty");
                                break;
                            }
                                                         
                            try
                            {
                                _configStrings.Add(reader.GetAttribute("Name"), reader.ReadString());
                            }
                            catch (Exception ex)
                            {
                                log.Error("Invalid ConfigTextItem", ex);
                                break;
                            }
                        }
                        break;
                    case "SnippetTemplatePath":
                        {
                            SnippetTemplatePath = reader.ReadString();
                        }
                        break;
                    case "SlotName":
                        {
                            SlotName = reader.ReadString();
                        }
                        break;
                    case "ContentID":
                        {
                            ContentID = reader.ReadString();
                        }
                        break;
                    //case "CDRId":
                    //    string cdrId = reader.ReadString();
                    //    if (!string.IsNullOrEmpty(cdrId))
                    //        CDRId = cdrId;
                    //    break;

                    //case "CDRDefinitionName":
                    //    CDRDefinitionName = reader.ReadString();
                    //    break;
                    case "DisplayVersion":
                        {
                            listOnlyDisplayFor.Add((DisplayVersions)Enum.Parse(typeof(DisplayVersions), reader.ReadString()));
                        }
                        break;
                    case "Snippets":
                        {
                            //using (XmlTextReader snippetReader = new XmlTextReader(snippetXmlData.Trim(), XmlNodeType.Element, null))
                            //{
                            //    XmlSerializer serializer = new XmlSerializer(typeof(SnippetInfoCollection));
                            //    this._snippets = (SnippetInfoCollection)serializer.Deserialize(reader);
                            //}

                            
                            _snippets = (SnippetInfoCollection)nestedSnippetSerializer.Deserialize(reader);
                        }
                    break;

                }
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteElementString("SnippetTemplatePath", SnippetTemplatePath);
            writer.WriteElementString("SlotName", SlotName);
            writer.WriteStartElement("Data");
            writer.WriteCData(Data);
            writer.WriteElementString("ContentID", ContentID);

            writer.WriteEndElement();
        }

        #endregion
    }
}
