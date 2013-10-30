using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MultiBuff
{
    public class BTPair
    {
        public List<int> Buffs;
        public int Time;
        public BTPair(List<int> bListVal, int bTimeVal)
        {
            Buffs = bListVal;
            Time = bTimeVal;
        }
    }

    public class mbConfig
    {
        public bool AllowDebuffs = false;
        public int DefaultMBTime = 9;
        public Dictionary<string, BTPair> BuffSets;

        public static mbConfig Read(string path)
        {
            if (!File.Exists(path))
                return new mbConfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        public static mbConfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<mbConfig>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        public void Write(Stream stream)
        {
            {
                List<int> magbuffs = new List<int>() { 6, 7, 26, 29 };
                List<int> ranbuffs = new List<int>() { 3, 16, 17, 63 };
                BuffSets = new Dictionary<string, BTPair>();
                BuffSets.Add("magic", new BTPair(magbuffs, 5));
                BuffSets.Add("range", new BTPair(ranbuffs, 9));
            }

            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        public static Action<mbConfig> ConfigRead;
    }
}
