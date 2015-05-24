using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VGMenu
{
    [Serializable]
    public class SeriesMetadatas
    {
        public SeriesMetadatas()
        {
            Metadatas = new List<SeriesMetadata>();
        }
        [XmlArray]
        public List<SeriesMetadata> Metadatas { get; set; }
    }

    [Serializable]
    public class SeriesMetadata
    {
        public SeriesMetadata()
        {
            Seasons = new List<SeasonMetadata>();
        }

        [XmlElement]
        public string FolderName { get; set; }

        [XmlElement]
        public bool MetaAvailable { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public int Id { get; set; }

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

        [XmlArray]
        public List<SeasonMetadata> Seasons { get; set; }
    }

    public class SeasonMetadata
    {
        public SeasonMetadata()
        {
            Episodes = new List<EpisodeMetadata>();
        }

        [XmlElement]
        public int Number { get; set; }

        [XmlElement]
        public int Id { get; set; }

        [XmlArray]
        public List<EpisodeMetadata> Episodes { get; set; }
    }

    public class EpisodeMetadata
    {
        [XmlElement]
        public int Number { get; set; }

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

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Filename { get; set; }
    }

}
