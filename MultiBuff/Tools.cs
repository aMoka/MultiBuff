using System;
using System.Collections.Generic;

namespace MultiBuff
{
    public class Tools
    {
        public static List<MBPlayer> Players = new List<MBPlayer>();

        public static List<MBPlayer> GetPlayerList(string name)
        {
            foreach (MBPlayer player in Players)
            {
                if (player.name.ToLower().Contains(name.ToLower()))
                {
                    return new List<MBPlayer>() { player };
                }
            }
            return new List<MBPlayer>();
        }

        public static MBPlayer GetPlayer(int index)
        {
            foreach (MBPlayer player in Players)
                if (player.Index == index)
                    return player;

            return null;
        }
    }
}
