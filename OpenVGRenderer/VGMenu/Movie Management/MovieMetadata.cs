using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VGMenu
{
    [Serializable]
    public class MovieMetadatas
    {
        public MovieMetadatas()
        {
            Metadatas = new List<MovieMetadata>();
        }
        [XmlArray]
        public List<MovieMetadata> Metadatas { get; set; }
    }

    [Serializable]
    public class MovieMetadata
    {
        [XmlElement]
        public string FolderName { get; set; }

        [XmlElement]
        public bool MetaAvailable { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public int Id { get; set; }

        [XmlElement]
        public string IMDBId { get; set; }


     
        
        [XmlIgnore]
        public string Description { get; set; }
        [XmlElement("Description")]
        public System.Xml.XmlCDataSection DescriptionCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Description);
            }
            set
            {
                Description = value.Value;
            }
        }

    }
}
