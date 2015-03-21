using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MultiBuff
{
    [ApiVersion(1, 17)]
    public class MB : TerrariaPlugin
    {
		public static MBConfig config { get; set; }
        public static string configDir { get { return Path.Combine(TShock.SavePath, "PluginConfigs"); } }
        public static string configPath { get { return Path.Combine(configDir, "MBConfig.json"); } }
		public static List<int> validBuffs = new List<int>();
        

        #region InfoStuff
        public override string Name
        { get { return "MultiBuff"; } }

        public override string Author
        { get { return "aMoka"; } }

        public override string Description
        { get { return "Micro-expanded buffing."; } }

        public override Version Version
        { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        #endregion;

        #region Initialize
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        }
        #endregion;

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
        }
        #endregion;

        public MB(Main game)
            : base(game)
        {
            Order = 1;

			config = new MBConfig();
        }

        #region OnInitialize
        public void OnInitialize(EventArgs args)
        {
            #region validEverBuffsList
            validBuffs.Add(1); //Obsidian Skin
            validBuffs.Add(2); //Regeneration
            validBuffs.Add(3); //Swiftness
            validBuffs.Add(4); //Gills
            validBuffs.Add(5); //Ironskin
            validBuffs.Add(6); //Mana Regeneration
            validBuffs.Add(7); //Magic Power
            validBuffs.Add(8); //Featherfall
            validBuffs.Add(9); //Spelunker
            validBuffs.Add(10); //Invisibility
            validBuffs.Add(11); //Shine
            validBuffs.Add(12); //Night Owl
            validBuffs.Add(13); //Battle
            validBuffs.Add(14); //Thorns
            validBuffs.Add(15); //Water Walking
            validBuffs.Add(16); //Archery
            validBuffs.Add(17); //Hunter
            validBuffs.Add(18); //Gravitation
            validBuffs.Add(25); //Tipsy
            validBuffs.Add(26); //Well Fed
            validBuffs.Add(29); //Clairvoyance
            validBuffs.Add(48); //Honey
            validBuffs.Add(58); //Rapid Healing
            validBuffs.Add(59); //Shadow Dodge
            validBuffs.Add(62); //Ice Barrier
            validBuffs.Add(63); //Panic!
            validBuffs.Add(71); //Weapon Imbue: Venom
            validBuffs.Add(73); //Weapon Imbue: Cursed Flames
            validBuffs.Add(74); //Weapon Imbue: Fire
            validBuffs.Add(75); //Weapon Imbue: Gold
            validBuffs.Add(76); //Weapon Imbue: Ichor
            validBuffs.Add(77); //Weapon Imbue: Nanites
            validBuffs.Add(78); //Weapon Imbue: Confetti
            validBuffs.Add(79); //Weapon Imbue: Poison
            validBuffs.Add(93); //Ammo Box
            validBuffs.Add(95); //Beetle Endurance (15%)	 
            validBuffs.Add(96); //Beetle Endurance (30%)	 
            validBuffs.Add(97); //Beetle Endurance (45%)	 
            validBuffs.Add(98); //Beetle Might (10%)	 
            validBuffs.Add(99); //Beetle Might (20%)	 
            validBuffs.Add(100); //Beetle Might (30%)
            validBuffs.Add(104); //Mining
            validBuffs.Add(105); //Heartreach
            validBuffs.Add(106); //Calm
            validBuffs.Add(107); //Builder
            validBuffs.Add(108); //Titan
            validBuffs.Add(109); //Flipper
            validBuffs.Add(110); //Summoning
            validBuffs.Add(111); //Dangersense
            validBuffs.Add(112); //Ammo Reservation
            validBuffs.Add(113); //Lifeforce
            validBuffs.Add(114); //Endurance
            validBuffs.Add(115); //Rage
            validBuffs.Add(116); //Inferno
            validBuffs.Add(117); //Wrath
            validBuffs.Add(121); //Fishing
            validBuffs.Add(122); //Sonar
            validBuffs.Add(123); //Crate
            validBuffs.Add(124); //Warmth

            #endregion;

			Commands.ChatCommands.Add(new Command("mb.buff.self", MultiBuff, "multibuff", "mb"));
			Commands.ChatCommands.Add(new Command("mb.buff.others", GiveMultiBuff, "gmb"));
			Commands.ChatCommands.Add(new Command("mb.buff.others", GiveMultiBuffAll, "gmba"));
			Commands.ChatCommands.Add(new Command("mb.buffset.self", BuffSet, "bset"));
			Commands.ChatCommands.Add(new Command("mb.buffset.others", GiveBuffSet, "gbset"));
			Commands.ChatCommands.Add(new Command("mb.buffset.other", GiveBuffSetAll, "gbseta"));
			Commands.ChatCommands.Add(new Command("mb.admin.reload", MBReload, "reloadmb"));
            SetUpConfig();
        }
        #endregion;

		#region MultiBuff
		public static void MultiBuff(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /mb <buff1 id/name> [buff2 id/name] ... [-t<seconds>]");
                return;
            }

            int time = config.DefaultMBTime;
            int chktime;
            string str = args.Parameters[args.Parameters.Count - 1];

            if (str.StartsWith("-t"))
            {
                if (!int.TryParse(str.Replace("-t", ""), out chktime))
                {
                    args.Player.SendErrorMessage("Invalid time! Using config-defined time.");
                    return;
                }
                else
                    time = chktime;

                args.Parameters.RemoveAt(args.Parameters.Count - 1);
            }
            List<string> addedBuffs = new List<string>();
            foreach (string buffs in args.Parameters)
            {
                int id;

                if (!int.TryParse(buffs, out id))
                {
                    var found = TShock.Utils.GetBuffByName(buffs);
                    if (found.Count == 0)
                    {
                        args.Player.SendErrorMessage("Invalid buff name!");
                        return;
                    }
                    else if (found.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, found.Select(b => Main.buffName[b]));
                        return;
                    }
                    id = found[0];
                }
				if (id < 0 && !config.AllowDebuffs && !validBuffs.Contains(id))
                {
                    args.Player.SendErrorMessage(string.Format("Invalid buff{0}",
						(!validBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                    return;
                }
                else
                {
                    args.Player.SetBuff(id, 60 * time);
                    addedBuffs.Add(TShock.Utils.GetBuffName(id));
                }
            }
            args.Player.SendSuccessMessage("You have buffed yourself with {0}!",
				String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + (addedBuffs.Count > 1 ? ", and " : "") + addedBuffs.LastOrDefault());
        }
        #endregion;

		#region GiveMultiBuff
		public static void GiveMultiBuff(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /gmb <player> <buff1 id/name> [buff2 id/name] ... [-t<seconds>]");
                return;
            }

            int id = 0;
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
            int time = config.DefaultMBTime;
            int chktime;
            string str = args.Parameters[args.Parameters.Count - 1];

            if (str.StartsWith("-t"))
            {
                if (!int.TryParse(str.Replace("-t", ""), out chktime))
                {
                    args.Player.SendErrorMessage("Invalid time! Using config-defined time.");
                    return;
                }
                else
                    time = chktime;

                args.Parameters.RemoveAt(args.Parameters.Count - 1);
            }
            if (foundplr.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            else if (foundplr.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, foundplr.Select(p => p.Name));
                return;
            }
            else
            {
                List<string> addedBuffs = new List<string>();
                foreach (string buffs in args.Parameters.Skip(1))
                {
                    if (!int.TryParse(buffs, out id))
                    {
                        var found = TShock.Utils.GetBuffByName(buffs);
                        if (found.Count == 0)
                        {
                            args.Player.SendErrorMessage("Invalid buff name!");
                            return;
                        }
                        else if (found.Count > 1)
                        {
                            TShock.Utils.SendMultipleMatchError(args.Player, found.Select(b => Main.buffName[b]));
                            return;
                        }
                        id = found[0];
                    }
					if (id < 0 && !config.AllowDebuffs && !validBuffs.Contains(id))
                    {
                        args.Player.SendErrorMessage(string.Format("Invalid buff{0}",
							(!validBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                        return;
                    }
                    else
                    {
                        foundplr[0].SetBuff(id, 60 * time);
                        addedBuffs.Add(TShock.Utils.GetBuffName(id));
                    }
                }
                args.Player.SendSuccessMessage("You have buffed {0} with {1}!", foundplr[0].Name,
							String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + (addedBuffs.Count > 1 ? ", and " : "") + addedBuffs.LastOrDefault());
                foundplr[0].SendSuccessMessage("{0} buffed you with {1}!",
                    args.Player.Name,
					String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + (addedBuffs.Count > 1 ? ", and " : "") + addedBuffs.LastOrDefault());
            }
        }
        #endregion;

		#region GiveMultiBuffAll
		public static void GiveMultiBuffAll(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /gmba <buff1 id/name> [buff2 id/name] ... [-t<seconds>]");
                return;
            }
            int id = 0;
            List<string> addedBuffs = new List<string>();
            int time = config.DefaultMBTime;
            int chktime;
            string str = args.Parameters[args.Parameters.Count - 1];

            if (str.StartsWith("-t"))
            {
                if (!int.TryParse(str.Replace("-t", ""), out chktime))
                {
                    args.Player.SendErrorMessage("Invalid time! Using config-defined time.");
                    return;
                }
                else
                    time = chktime;

                args.Parameters.RemoveAt(args.Parameters.Count - 1);
            }
            foreach (string buffs in args.Parameters)
            {
                if (!int.TryParse(buffs, out id))
                {
                    var found = TShock.Utils.GetBuffByName(buffs);
                    if (found.Count == 0)
                    {
                        args.Player.SendErrorMessage("Invalid buff name!");
                        return;
                    }
                    else if (found.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, found.Select(b => Main.buffName[b]));
                        return;
                    }
                    id = found[0];
                }
				if (id < 0 && !config.AllowDebuffs && !validBuffs.Contains(id))
                {
                    args.Player.SendErrorMessage(string.Format("Invalid buff{0}",
						(!validBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                    return;
                }
                else
                {
					foreach (TSPlayer player in TShock.Players.Where(p => p != null && p.Active))
                    {
						player.SetBuff(id, 60 * time);
                    }
                    addedBuffs.Add(TShock.Utils.GetBuffName(id));
                }
            }
            TSPlayer.All.SendInfoMessage("{0} buffed everyone with {1}!", args.Player.Name,
				String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + (addedBuffs.Count > 1 ? ", and " : "") + addedBuffs.LastOrDefault());
        }
        #endregion;

		#region BuffSet
		public static void BuffSet(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /bset <-list/buffset name> [-t<seconds>]");
                return;
            }
            if (args.Parameters[0].ToLower() == "-list")
            {
                List<string> allBSets = new List<string>(config.BuffSets.Keys);
                if (allBSets.Count < 1)
                    args.Player.SendInfoMessage("There are no buff sets configured!");
                else                
                    args.Player.SendInfoMessage("Buff sets: {0}.", string.Join(", ", allBSets));
                return;
            }
            BTPair pair;
            int time = 0;
            int chktime;
            string str = args.Parameters[args.Parameters.Count - 1];

            if (str.StartsWith("-t"))
            {
                if (!int.TryParse(str.Replace("-t", ""), out chktime))
                {
                    args.Player.SendErrorMessage("Invalid time! Using config-defined time.");
                    return;
                }
                else
                    time = chktime;

                args.Parameters.RemoveAt(args.Parameters.Count - 1);
            }
            if (config.BuffSets.TryGetValue(args.Parameters[0], out pair))                          //get the values from the string (dictionary key) from cmd
            {
                foreach (int buff in pair.Buffs)                                                    //get each int in the List<int> from the values of the buffset
                {
                    args.Player.SetBuff(buff, 60 * ((time == 0) ? pair.Time : time));
                }
                args.Player.SendSuccessMessage("Buffed with the {0} set!", args.Parameters[0]);
            }
            else
                args.Player.SendErrorMessage("Invalid buff set!");
        }
        #endregion

		#region GiveBuffSet
		public static void GiveBuffSet(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /gbset <player> <buffset name> [-t<seconds>]");
                return;
            }
            BTPair pair;
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
            int time = 0;
            int chktime;
            string str = args.Parameters[args.Parameters.Count - 1];

            if (str.StartsWith("-t"))
            {
                if (!int.TryParse(str.Replace("-t", ""), out chktime))
                {
                    args.Player.SendErrorMessage("Invalid time! Using config-defined time.");
                    return;
                }
                else
                    time = chktime;

                args.Parameters.RemoveAt(args.Parameters.Count - 1);
            }
            if (foundplr.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            else if (foundplr.Count > 1)
            {
                List<string> foundPlayers = new List<string>();
                foreach (TSPlayer player in foundplr)
                {
                    foundPlayers.Add(player.Name);
                }
                TShock.Utils.SendMultipleMatchError(args.Player, foundPlayers);
                return;
            }
            else
            {
                if (config.BuffSets.TryGetValue(args.Parameters[1], out pair))
                {
                    foreach (int buff in pair.Buffs)
                    {
                        foundplr[0].SetBuff(buff, 60 * ((time == 0) ? pair.Time : time));
                    }
                    args.Player.SendSuccessMessage("Buffed {0} with the {1} set!", 
                        foundplr[0].Name, args.Parameters[1]);
                    foundplr[0].SendSuccessMessage("{0} buffed you with the {1} set!", 
                        args.Player.Name, args.Parameters[1]);
                }
                else
                    args.Player.SendErrorMessage("Invalid buff set!");
            }
            
        }
        #endregion

		#region GiveBuffSetAll
		public static void GiveBuffSetAll(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /gbseta <buffset name> [-t<seconds>]");
                return;
            }
            BTPair pair;
            int time = 0;
            int chktime;
            string str = args.Parameters[args.Parameters.Count - 1];

            if (str.StartsWith("-t"))
            {
                if (!int.TryParse(str.Replace("-t", ""), out chktime))
                {
                    args.Player.SendErrorMessage("Invalid time! Using config-defined time.");
                    return;
                }
                else
                    time = chktime;

                args.Parameters.RemoveAt(args.Parameters.Count - 1);
            }
            if (config.BuffSets.TryGetValue(args.Parameters[0], out pair))
            {
                foreach (int buff in pair.Buffs)
                {
					foreach (TSPlayer player in TShock.Players.Where(p => p != null && p.Active))
                    {
                        player.SetBuff(buff, 60 * ((time == 0) ? pair.Time : time));
                    }
                }
                TSPlayer.All.SendInfoMessage("{0} buffed everyone with the {1} set!",
                    args.Player.Name, args.Parameters[0]);
            }
            else
                args.Player.SendErrorMessage("Invalid buff set!");
            }
        #endregion

		#region MBReload
		public static void MBReload(CommandArgs args)
        {
            SetUpConfig();
            args.Player.SendInfoMessage("Attempted to reload the config file");
        }
        #endregion

        #region SetUpConfig
        public static void SetUpConfig()
        {
            try
            {
                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);

                if (File.Exists(configPath))
					config = MBConfig.Read(configPath);
                else
                    config.Write(configPath);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in MBConfig.json!");
                Console.ResetColor();
            }
        }
        #endregion
    }
}