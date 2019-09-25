using RETRD.Helpers;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Xml.Serialization;

namespace RETRD.Models.Configuration
{
    [XmlRoot(ElementName = "RDCMan")]
    public class RDCManager
    {
        [XmlAttribute(AttributeName = "programVersion")]
        public double ProgramVersion { get; set; } = 2.7;

        [XmlAttribute(AttributeName = "schemaVersion")]
        public double SchemaVersion { get; set; } = 3;

        [XmlElement(ElementName = "file")]
        public File File { get; set; }

        [XmlElement(ElementName = "connected")]
        public string Connected { get; set; } = string.Empty;

        [XmlElement(ElementName = "favorites")]
        public string Favorites { get; set; } = string.Empty;

        [XmlElement(ElementName = "recentlyUsed")]
        public string RecentlyUsed { get; set; } = string.Empty;
    }

    public class File
    {
        [XmlElement(ElementName = "credentialsProfiles")]
        public string CredentialProfiles { get; set; } = string.Empty;

        [XmlElement("properties")]
        public FileProperties Properties { get; set; }

        [XmlElement(ElementName = "group")]
        public List<Group> Groups { get; set; }

        [XmlElement(ElementName = "server")]
        public List<Server> Servers { get; set; }
    }

    public class Group
    {
        [XmlElement("properties")]
        public GroupProperties Properties { get; set; }

        [XmlElement(ElementName = "group")]
        public List<Group> Groups { get; set; }

        [XmlElement(ElementName = "server")]
        public List<Server> Servers { get; set; }

        [XmlIgnore]
        public string Path { get; set; }

        public static explicit operator Group(DirectoryEntry ou)
        {
            return new Group
            {
                Properties = new GroupProperties
                {
                    Name = StringHelpers.RemoveLdapPrefix(ou.Name)
                },

                Path = ou.Path
            };
        }
    }

    public class Server
    {
        [XmlElement("properties")]
        public ServerProperties Properties { get; set; }

        public static explicit operator Server(DirectoryEntry computer)
        {
            return new Server
            {
                Properties = new ServerProperties
                {
                    Name = StringHelpers.RemoveLdapPrefix(computer.Name)
                }
            };
        }
    }

    public interface IProperties
    {
        [XmlElement(ElementName = "name")]
        string Name { get; set; }
    }

    public class GroupProperties : IProperties
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "expanded")]
        public bool Expanded { get; set; } = false;
    }

    public class FileProperties : GroupProperties { }

    public class ServerProperties : IProperties
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
    }
}
