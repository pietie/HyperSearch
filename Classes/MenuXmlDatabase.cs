using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using HscLib;
using HyperSearch;

namespace HyperSpinClone.Classes
{
    [Serializable]
    [XmlRoot("menu")]
    public class MenuXmlDatabase
    {
        [XmlIgnore]
        public string FilePath { get; private set; }

        [XmlIgnore]
        public string SystemName { get; /*private*/ set; }

        public MenuXmlDatabase()
        {
            GameList = new List<GameXmlDatabase>();
        }

        public GameXmlDatabase this[int index]
        {
            get { return this.GameList[index]; }
        }

        [XmlElement("header")]
        public MenuHeader header { get; set; }

        [XmlElement("game")]
        public List<GameXmlDatabase> GameList { get; set; }

        public static MenuXmlDatabase LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(string.Format("File not found: {0}", filePath ?? "(null)"));

            string xmlStr = File.ReadAllText(filePath);

            XmlSerializer xs = new XmlSerializer(typeof(MenuXmlDatabase), new Type[] { typeof(GameXmlDatabase) });
            XmlReaderSettings settings = new XmlReaderSettings();

            settings.CloseInput = true;
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.CheckCharacters = false;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.ValidationType = ValidationType.None;

            xs.UnknownAttribute += new XmlAttributeEventHandler(xs_UnknownAttribute);
            xs.UnknownElement += new XmlElementEventHandler(xs_UnknownElement);
            xs.UnknownNode += new XmlNodeEventHandler(xs_UnknownNode);
            xs.UnreferencedObject += new UnreferencedObjectEventHandler(xs_UnreferencedObject);

            MenuXmlDatabase db = null;

            using (var xr = XmlReader.Create(new System.IO.StringReader(xmlStr), settings))
            {
                db = xs.Deserialize(xr) as MenuXmlDatabase;
            }

            foreach (var child in db.GameList)
            {
                child.ParentMenu = db;
            }

            DirectoryInfo di = new DirectoryInfo(filePath);

            db.FilePath = filePath;

            if (di != null && di.Parent != null)
                db.SystemName = di.Parent.Name;


            return db;
        }


        private static void xs_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {

        }

        private static void xs_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            // we don't care about unknown nodes 
        }

        private static void xs_UnknownElement(object sender, XmlElementEventArgs e)
        {
            // we don't care about unknown elements  
        }

        private static void xs_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            // we dont' care about unknown attributes
        }
    }

    [Serializable]
    [XmlRoot("game")]
    public class GameXmlDatabase
    {
        public GameXmlDatabase()
        {
        }

        [XmlIgnore]
        public SortedSet<string> AllGenres { get; set; }

        [XmlIgnore]
        public MenuXmlDatabase ParentMenu { get; set; }

        [XmlAttribute("SteamAppId")]
        public int SteamAppId { get; set; }


        // TODO: This is probably what is in HS2.0 ... but not sure if this is an attribue or an element
        [XmlElement]
        public string enabled { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string index { get; set; }

        [XmlAttribute]
        public string image { get; set; }

        [XmlElement]
        public string description { get; set; }

        [XmlElement]
        public string cloneof { get; set; }

        [XmlElement]
        public string crc { get; set; }

        [XmlElement]
        public string manufacturer { get; set; }

        [XmlElement]
        public string year { get; set; }

        [XmlElement]
        public string genre { get; set; }

        [XmlElement]
        public string rating { get; set; }

        public override string ToString()
        {
            if (this.ParentMenu == null) return this.name;
            return string.Format("{0}\\{1}", this.ParentMenu.SystemName ?? "", this.name);
        }

        [XmlIgnore]
        public Uri SystemImageSourceUri { get; private set; }

        [XmlIgnore]
        public FileInfo SystemImageSwfSourceFileInfo { get; private set; }

        [XmlIgnore]
        public Uri GameImageSourceUri { get; private set; }

        [XmlIgnore]
        public FileInfo GameImageSwfSourceFileInfo { get; private set; }

        public void InitUris()
        {
            SystemImageSourceUri = GetSafeUri(HyperSearch.Global.BuildFilePathInHyperspinDir(@"Media\Main Menu\Images\Wheel\{0}.png", ParentMenu.SystemName));

            var systemSwf = HyperSearch.Global.BuildFilePathInHyperspinDir(@"Media\Main Menu\Images\Wheel\{0}.swf", ParentMenu.SystemName);

            if (File.Exists(systemSwf))
            {
                SystemImageSwfSourceFileInfo = new FileInfo(systemSwf);
            }

            if (string.IsNullOrEmpty(HyperSearch.Global.AlternativeGameWheelSourceFolder))
            {
               GameImageSourceUri = GetSafeUri(HyperSearch.Global.BuildFilePathInHyperspinDir(@"Media\{0}\Images\Wheel\{1}.png", ParentMenu.SystemName, name));

                var swfPath = HyperSearch.Global.BuildFilePathInHyperspinDir(@"Media\{0}\Images\Wheel\{1}.swf", ParentMenu.SystemName, name);

                if (File.Exists(swfPath))
                {
                    GameImageSwfSourceFileInfo = new FileInfo(swfPath);
                }
            }
            else
            {
                var p = HyperSearch.Global.BuildFilePathInHyperspinDir(@"Media\{0}\Images\{2}\{1}.png", ParentMenu.SystemName, name, HyperSearch.Global.AlternativeGameWheelSourceFolder);

                GameImageSourceUri = GetSafeUri(p);

                var swfPath = HyperSearch.Global.BuildFilePathInHyperspinDir(@"Media\{0}\Images\{2}\{1}.swf", ParentMenu.SystemName, name, HyperSearch.Global.AlternativeGameWheelSourceFolder);

                if (File.Exists(swfPath))
                {
                    GameImageSwfSourceFileInfo = new FileInfo(swfPath);
                }
            }
        }

        private Uri GetSafeUri(string path)
        {
            if (!File.Exists(path)) return null;

            return new Uri(path, UriKind.Absolute);
        }
    }

    [Serializable]
    [XmlRoot("header")]
    public class MenuHeader
    {
        public string listname { get; set; }
        public string lastlistupdate { get; set; }
        public string listversion { get; set; }
        public string exporterversion { get; set; }
    }
}
