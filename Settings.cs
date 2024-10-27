using System.IO;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace ChangeLobbyBgm
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            string filepath = Path.Combine(modEntry.Path, "Settings.xml");
            using (StreamWriter writer = new StreamWriter(filepath))
                new XmlSerializer(GetType()).Serialize(writer, this);
        }

        public float defaultBpm = 100;
        public float fastBpm = 100;
        public bool fastMusic = true;
        public bool multiplyMusic = false;
        public bool customMusic = false;
        public string defaultMusicPath;
        public string fastMusicPath;
    }
}
