using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Terraria;
using TShockAPI;

namespace MultiBuff
{
    public class Tools
    {
        public static List<MBPlayer> Players = new List<MBPlayer>();
        public static Timer everBuffTimer = new Timer(100);

        public static void initializeTimers()
        {
            everBuffTimer.Elapsed += new ElapsedEventHandler(eBTimer);
        }

        public static void eBTimer(object sender, ElapsedEventArgs args)
        {
            int count = 0;
            foreach (MBPlayer player in Players)
            {
                if (player.isEverBuff)
                {
                    foreach (int activeBuff in player.TSPlayer.TPlayer.buffType)
                    {
                        player.TSPlayer.SetBuff(activeBuff, 32400);
                        player.TSPlayer.TPlayer.AddBuff(activeBuff, 32400);
                    }
                    count++;
                }
            }
        }

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
