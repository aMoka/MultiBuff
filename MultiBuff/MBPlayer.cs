using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace MultiBuff
{
    public class MBPlayer
    {
        public int Index;
        public bool isEverBuff;
        public bool isEverDebuff;

        public string name { get { return Main.player[Index].name; } }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }

        public MBPlayer(int index)
        {
            Index = index;
        }
    }
}
