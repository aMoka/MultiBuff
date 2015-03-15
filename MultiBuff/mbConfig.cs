using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MultiBuff
{
    public class BTPair
    {
        public List<int> Buffs;
        public int Time;
        public BTPair(List<int> bListVal, int bTimeVal)
        {
            Buffs = bListVal;                               //The list of buffs
            Time = bTimeVal;                                //The time (in seconds) all buffs in the set lasts
        }
    }

    public class MBConfig
    {
        public bool AllowDebuffs = false;                   //Allows gmb and mb to have debuffs in the command line
        public int DefaultMBTime = 540;                       //The default time (in seconds) gmb and mb sets a buff
        public Dictionary<string, BTPair> BuffSets;         //BuffSets Dictionary

		public static MBConfig Read(string path)
        {
            if (!File.Exists(path))
				return new MBConfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

		public static MBConfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
				var cf = JsonConvert.DeserializeObject<MBConfig>(sr.ReadToEnd());
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
                List<int> magbuffs = new List<int>() { 6, 7, 26, 29 };          //new example buff list
                List<int> ranbuffs = new List<int>() { 3, 16, 17, 63 };         
                BuffSets = new Dictionary<string, BTPair>();                    
                BuffSets.Add("magic", new BTPair(magbuffs, 300));                 //Add to BuffSets Dictionary(string, List<int> bListVal, int bTimeVal)
                BuffSets.Add("range", new BTPair(ranbuffs, 540));                 
            }

            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

		public static Action<MBConfig> ConfigRead;
    }
}
