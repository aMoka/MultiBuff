using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi;
using TerrariaApi.Server;
using TShockAPI;

namespace MultiBuff
{
    [ApiVersion(1, 16)]
    public class MB : TerrariaPlugin
    {
        public static mbConfig config { get; set; }
        public static string configDir { get { return Path.Combine(TShock.SavePath, "PluginConfigs"); } }
        public static string configPath { get { return Path.Combine(configDir, "MBConfig.json"); } }
        public static List<int> validEverBuffs = new List<int>();
        

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
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        }
        #endregion;

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }
        }
        #endregion;

        public MB(Main game)
            : base(game)
        {
            Order = 1;

            config = new mbConfig();
        }

        #region OnInitialize
        public void OnInitialize(EventArgs args)
        {
            #region validEverBuffsList
            validEverBuffs.Add(1); //Obsidian Skin
            validEverBuffs.Add(2); //Regeneration
            validEverBuffs.Add(3); //Swiftness
            validEverBuffs.Add(4); //Gills
            validEverBuffs.Add(5); //Ironskin
            validEverBuffs.Add(6); //Mana Regeneration
            validEverBuffs.Add(7); //Magic Power
            validEverBuffs.Add(8); //Featherfall
            validEverBuffs.Add(9); //Spelunker
            validEverBuffs.Add(10); //Invisibility
            validEverBuffs.Add(11); //Shine
            validEverBuffs.Add(12); //Night Owl
            validEverBuffs.Add(13); //Battle
            validEverBuffs.Add(14); //Thorns
            validEverBuffs.Add(15); //Water Walking
            validEverBuffs.Add(16); //Archery
            validEverBuffs.Add(17); //Hunter
            validEverBuffs.Add(18); //Gravitation
            validEverBuffs.Add(25); //Tipsy
            validEverBuffs.Add(26); //Well Fed
            validEverBuffs.Add(29); //Clairvoyance
            validEverBuffs.Add(48); //Honey
            validEverBuffs.Add(58); //Rapid Healing
            validEverBuffs.Add(59); //Shadow Dodge
            validEverBuffs.Add(62); //Ice Barrier
            validEverBuffs.Add(63); //Panic!
            validEverBuffs.Add(71); //Weapon Imbue: Venom
            validEverBuffs.Add(73); //Weapon Imbue: Cursed Flames
            validEverBuffs.Add(74); //Weapon Imbue: Fire
            validEverBuffs.Add(75); //Weapon Imbue: Gold
            validEverBuffs.Add(76); //Weapon Imbue: Ichor
            validEverBuffs.Add(77); //Weapon Imbue: Nanites
            validEverBuffs.Add(78); //Weapon Imbue: Confetti
            validEverBuffs.Add(79); //Weapon Imbue: Poison
            validEverBuffs.Add(93); //Ammo Box
            validEverBuffs.Add(95); //Beetle Endurance (15%)	 
            validEverBuffs.Add(96); //Beetle Endurance (30%)	 
            validEverBuffs.Add(97); //Beetle Endurance (45%)	 
            validEverBuffs.Add(98); //Beetle Might (10%)	 
            validEverBuffs.Add(99); //Beetle Might (20%)	 
            validEverBuffs.Add(100); //Beetle Might (30%)
            validEverBuffs.Add(104); //Mining
            validEverBuffs.Add(105); //Heartreach
            validEverBuffs.Add(106); //Calm
            validEverBuffs.Add(107); //Builder
            validEverBuffs.Add(108); //Titan
            validEverBuffs.Add(109); //Flipper
            validEverBuffs.Add(110); //Summoning
            validEverBuffs.Add(111); //Dangersense
            validEverBuffs.Add(112); //Ammo Reservation
            validEverBuffs.Add(113); //Lifeforce
            validEverBuffs.Add(114); //Endurance
            validEverBuffs.Add(115); //Rage
            validEverBuffs.Add(116); //Inferno
            validEverBuffs.Add(117); //Wrath
            validEverBuffs.Add(121); //Fishing
            validEverBuffs.Add(122); //Sonar
            validEverBuffs.Add(123); //Crate
            validEverBuffs.Add(124); //Warmth

            #endregion;

            Commands.ChatCommands.Add(new Command("mb.buff.self", multiBuff, "multibuff", "mb"));
            Commands.ChatCommands.Add(new Command("mb.buff.others", giveMultiBuff, "gmb"));
            Commands.ChatCommands.Add(new Command("mb.buff.others", giveMultiBuffAll, "gmba"));
            Commands.ChatCommands.Add(new Command("mb.buffset.self", buffSet, "bset"));
            Commands.ChatCommands.Add(new Command("mb.buffset.others", giveBuffSet, "gbset"));
            Commands.ChatCommands.Add(new Command("mb.buffset.other", giveBuffSetAll, "gbseta"));
            Commands.ChatCommands.Add(new Command("mb.admin.reload", mbReload, "reloadmb"));
            Commands.ChatCommands.Add(new Command("mb.everbuff", everBuff, "everbuff", "eb"));

            Tools.initializeTimers();
            Tools.everBuffTimer.Enabled = true;
            SetUpConfig();
        }
        #endregion;

        #region OnJoin
        public void OnJoin(GreetPlayerEventArgs args)
        {
            Tools.Players.Add(new MBPlayer(args.Who));

            var player = TShock.Players[args.Who];
            var MBPlayer = Tools.GetPlayer(args.Who);
        }
        #endregion

        #region OnLeave
        public void OnLeave(LeaveEventArgs args)
        {
            var player = Tools.GetPlayer(args.Who);
            Tools.Players.RemoveAll(pl => pl.Index == args.Who);
        }
        #endregion

        #region multiBuff
        public static void multiBuff(CommandArgs args)
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
                if (id < 0 && !config.AllowDebuffs && !validEverBuffs.Contains(id))
                {
                    args.Player.SendErrorMessage(string.Format("Invalid buff{0}", 
                        (!validEverBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                    return;
                }
                else
                {
                    args.Player.SetBuff(id, 60 * time);
                    addedBuffs.Add(TShock.Utils.GetBuffName(id));
                }
            }
            args.Player.SendSuccessMessage("You have buffed yourself with {0}!", 
                String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + ", and " + addedBuffs.LastOrDefault());
        }
        #endregion;

        #region giveMB
        public static void giveMultiBuff(CommandArgs args)
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
                    if (id < 0 && !config.AllowDebuffs && !validEverBuffs.Contains(id))
                    {
                        args.Player.SendErrorMessage(string.Format("Invalid buff{0}",
                            (!validEverBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                        return;
                    }
                    else
                    {
                        foundplr[0].SetBuff(id, 60 * time);
                        addedBuffs.Add(TShock.Utils.GetBuffName(id));
                    }
                }
                args.Player.SendSuccessMessage("You have buffed {0} with {1}!", foundplr[0].Name, 
                            String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + ", and " + addedBuffs.LastOrDefault());
                foundplr[0].SendSuccessMessage("{0} buffed you with {1}!",
                    args.Player.Name, 
                    String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + ", and " + addedBuffs.LastOrDefault());
            }
        }
        #endregion;

        #region giveMBAll
        public static void giveMultiBuffAll(CommandArgs args)
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
                if (id < 0 && !config.AllowDebuffs && !validEverBuffs.Contains(id))
                {
                    args.Player.SendErrorMessage(string.Format("Invalid buff{0}",
                        (!validEverBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                    return;
                }
                else
                {
                    foreach (MBPlayer plr in Tools.Players)
                    {
                        plr.TSPlayer.SetBuff(id, 60 * time);
                    }
                    addedBuffs.Add(TShock.Utils.GetBuffName(id));
                }
            }
            TSPlayer.All.SendInfoMessage("{0} buffed everyone with {1}!", args.Player.Name, 
                String.Join(", ", addedBuffs.ToArray(), 0, addedBuffs.Count - 1) + ", and " + addedBuffs.LastOrDefault());
        }
        #endregion;

        #region buffSet
        public static void buffSet(CommandArgs args)
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

        #region giveBuffSet
        public static void giveBuffSet(CommandArgs args)
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

        #region giveBuffSetAll
        public static void giveBuffSetAll(CommandArgs args)
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
                    foreach (MBPlayer plr in Tools.Players)
                    {
                        plr.TSPlayer.SetBuff(buff, 60 * ((time == 0) ? pair.Time : time));
                    }
                }
                TSPlayer.All.SendSuccessMessage("{0} buffed everyone with the {1} set!",
                    args.Player.Name, args.Parameters[0]);
            }
            else
                args.Player.SendErrorMessage("Invalid buff set!");
            }
        #endregion

        #region mbReload
        public static void mbReload(CommandArgs args)
        {
            SetUpConfig();
            args.Player.SendInfoMessage("Attempted to reload the config file");
        }
        #endregion

        #region everBuff
        public static void everBuff(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                var player = Tools.GetPlayer(args.Player.Index);
                player.isEverBuff = !player.isEverBuff;

                args.Player.SendSuccessMessage("Everbuffs are now " + (player.isEverBuff ? "on." : "off."));
            }
            else
            {
                string str = args.Parameters[0];

                if (str == "-allon")
                {
                    foreach (MBPlayer plr in Tools.Players)
                    {
                        plr.isEverBuff = true;
                    }
                    args.Player.SendSuccessMessage("You have activated everbuffs for everyone!");
                    TSPlayer.All.SendSuccessMessage("{0} has activated everbuffs for everyone!", args.Player.Name);
                    return;
                }
                else if (str == "-alloff")
                {
                    foreach (MBPlayer plr in Tools.Players)
                    {
                        plr.isEverBuff = false;
                    }
                    args.Player.SendSuccessMessage("You have deactivated everbuffs for everyone!");
                    TSPlayer.All.SendSuccessMessage("{0} has deactivated everbuffs for everyone!", args.Player.Name);
                    return;
                }
                var foundplr = TShockAPI.TShock.Utils.FindPlayer(str);

                if (foundplr.Count > 1 && !(str == "-allon" || str == "-alloff"))
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, foundplr.Select(p => p.Name));
                }
                else if (foundplr.Count < 1 && !(str == "-allon" || str == "-alloff"))
                {
                    args.Player.SendErrorMessage(foundplr.Count + " players matched.");
                }
                else
                {
                    TShockAPI.TSPlayer ply = foundplr[0];
                    var player = Tools.GetPlayer(ply.Index);

                    player.isEverBuff = !player.isEverBuff;

                    args.Player.SendSuccessMessage(string.Format("You have {0}tivated everbuffs on {1}.",
                        (player.isEverBuff ? "ac" : "deac"), ply.Name));

                    ply.SendInfoMessage(string.Format("{0} has {1}tivated everbuffs on you.",
                        args.Player.Name, (player.isEverBuff ? "ac" : "deac")));
                }
            }
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
                    config = mbConfig.Read(configPath);
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