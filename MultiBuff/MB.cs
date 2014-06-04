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
        public static List<int> validPermaBuffs = new List<int>();
        

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
            #region validPermaBuffsList
            validPermaBuffs.Add(1); //Obsidian Skin
            validPermaBuffs.Add(2); //Regeneration
            validPermaBuffs.Add(3); //Swiftness
            validPermaBuffs.Add(4); //Gills
            validPermaBuffs.Add(5); //Ironskin
            validPermaBuffs.Add(6); //Mana Regeneration
            validPermaBuffs.Add(7); //Magic Power
            validPermaBuffs.Add(8); //Featherfall
            validPermaBuffs.Add(9); //Spelunker
            validPermaBuffs.Add(10); //Invisibility
            validPermaBuffs.Add(11); //Shine
            validPermaBuffs.Add(12); //Night Owl
            validPermaBuffs.Add(13); //Battle
            validPermaBuffs.Add(14); //Thorns
            validPermaBuffs.Add(15); //Water Walking
            validPermaBuffs.Add(16); //Archery
            validPermaBuffs.Add(17); //Hunter
            validPermaBuffs.Add(18); //Gravitation
            validPermaBuffs.Add(25); //Tipsy
            validPermaBuffs.Add(26); //Well Fed
            validPermaBuffs.Add(29); //Clairvoyance
            validPermaBuffs.Add(48); //Honey
            validPermaBuffs.Add(58); //Rapid Healing
            validPermaBuffs.Add(59); //Shadow Dodge
            validPermaBuffs.Add(62); //Ice Barrier
            validPermaBuffs.Add(63); //Panic!
            validPermaBuffs.Add(71); //Weapon Imbue: Venom
            validPermaBuffs.Add(73); //Weapon Imbue: Cursed Flames
            validPermaBuffs.Add(74); //Weapon Imbue: Fire
            validPermaBuffs.Add(75); //Weapon Imbue: Gold
            validPermaBuffs.Add(76); //Weapon Imbue: Ichor
            validPermaBuffs.Add(77); //Weapon Imbue: Nanites
            validPermaBuffs.Add(78); //Weapon Imbue: Confetti
            validPermaBuffs.Add(79); //Weapon Imbue: Poison
            validPermaBuffs.Add(93); //Ammo Box
            validPermaBuffs.Add(95); //Beetle Endurance (15%)	 
            validPermaBuffs.Add(96); //Beetle Endurance (30%)	 
            validPermaBuffs.Add(97); //Beetle Endurance (45%)	 
            validPermaBuffs.Add(98); //Beetle Might (10%)	 
            validPermaBuffs.Add(99); //Beetle Might (20%)	 
            validPermaBuffs.Add(100); //Beetle Might (30%)
            validPermaBuffs.Add(104); //Mining
            validPermaBuffs.Add(105); //Heartreach
            validPermaBuffs.Add(106); //Calm
            validPermaBuffs.Add(107); //Builder
            validPermaBuffs.Add(108); //Titan
            validPermaBuffs.Add(109); //Flipper
            validPermaBuffs.Add(110); //Summoning
            validPermaBuffs.Add(111); //Dangersense
            validPermaBuffs.Add(112); //Ammo Reservation
            validPermaBuffs.Add(113); //Lifeforce
            validPermaBuffs.Add(114); //Endurance
            validPermaBuffs.Add(115); //Rage
            validPermaBuffs.Add(116); //Inferno
            validPermaBuffs.Add(117); //Wrath
            validPermaBuffs.Add(121); //Fishing
            validPermaBuffs.Add(122); //Sonar
            validPermaBuffs.Add(123); //Crate
            validPermaBuffs.Add(124); //Warmth

            #endregion;

            Commands.ChatCommands.Add(new Command("tshock.buff.self", multiBuff, "multibuff", "mb"));
            Commands.ChatCommands.Add(new Command("tshock.buff.others", giveMultiBuff, "gmultibuff", "gmb"));
            Commands.ChatCommands.Add(new Command("mb.buffset.self", buffSet, "bset"));
            Commands.ChatCommands.Add(new Command("mb.buffset.others", giveBuffSet, "gbset"));
            Commands.ChatCommands.Add(new Command("mb.admin.reload", mbReload, "reloadmb"));
            Commands.ChatCommands.Add(new Command("mb.permabuff.set", permaBuff, "permabuff", "pb"));

            Tools.initializeTimers();
            Tools.permaBuffTimer.Enabled = true;
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
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /mb <buff1 id/name> [buff2 id/name] ...");
                return;
            }
            foreach (string buffs in args.Parameters)
            {
                int id = 0;
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
                if (id < 0 && !config.AllowDebuffs && !validPermaBuffs.Contains(id))
                {
                    args.Player.SendErrorMessage(string.Format("Invalid buff{0}", 
                        (!validPermaBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                }
                else
                {
                    args.Player.SetBuff(id, 3600 * config.DefaultMBTime);
                    args.Player.SendSuccessMessage("You have buffed yourself with {0}({1})!",
                        TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                }
            }
        }
        #endregion;

        #region giveMB
        public static void giveMultiBuff(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /gmb <-all/player> <buff1 id/name> [buff2 id/name] ...");
                return;
            }
            int id = 0;
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (foundplr.Count < 1 && !(args.Parameters[0] == "-all"))
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            else if (foundplr.Count > 1 && !(args.Parameters[0] == "-all"))
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
                    if (id < 0 && !config.AllowDebuffs && !validPermaBuffs.Contains(id))
                    {
                        args.Player.SendErrorMessage(string.Format("Invalid buff{0}", 
                            (!validPermaBuffs.Contains(id) && !config.AllowDebuffs) ? ": debuff!" : "!"));
                        return;
                    }
                    else
                    {
                        if (args.Parameters[0] == "-all")
                        {
                            foreach (TSPlayer player in TShock.Players)
                            {
                                player.SetBuff(id, 3600 * config.DefaultMBTime);
                                addedBuffs.Add(TShock.Utils.GetBuffName(id));
                            }
                        }
                        else
                        {
                            foundplr[0].SetBuff(id, 3600 * config.DefaultMBTime);
                            addedBuffs.Add(TShock.Utils.GetBuffName(id));
                        }
                    }
                }
                if (args.Parameters[0] == "-all")
                {
                    TSPlayer.All.SendInfoMessage("{0} buffed everyone with {1}!",
                        args.Player.Name, string.Join(", ", addedBuffs));
                }
                else
                {
                    args.Player.SendSuccessMessage("You have buffed {0} with {1}!",
                        foundplr[0].Name, string.Join(", ", addedBuffs));
                    foundplr[0].SendSuccessMessage("{0} buffed you with {1}!",
                        args.Player.Name, string.Join(", ", addedBuffs));
                }
            }
        }
        #endregion;

        #region buffSet
        public static void buffSet(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /bset <-list/buffset name>");
                return;
            }
            if (args.Parameters[0].ToLower() == "-list")
            {
                List<string> allBSets = new List<string>(config.BuffSets.Keys);
                if (allBSets.Count < 1)
                    args.Player.SendSuccessMessage("There are no buff sets configured!");
                else
                    args.Player.SendSuccessMessage("Buff sets: {0}", string.Join(", ", allBSets));
                return;
            }
            BTPair pair;
            if (config.BuffSets.TryGetValue(args.Parameters[0], out pair))                          //get the values from the string (dictionary key) from cmd
            {
                foreach (int buff in pair.Buffs)                                                    //get each int in the List<int> from the values of the buffset
                {
                    args.Player.SetBuff(buff, 3600 * pair.Time);
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
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /gbset <-all/player> <buffset name>");
                return;
            }
            BTPair pair;
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[1]);
            if (foundplr.Count < 1 && !(args.Parameters[0] == "-all"))
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            else if (foundplr.Count > 1 && !(args.Parameters[0] == "-all"))
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
                if (config.BuffSets.TryGetValue(args.Parameters[0], out pair))
                {
                    foreach (int buff in pair.Buffs)
                    {
                        if (args.Parameters[0] == "-all")
                        {
                            foreach (TSPlayer player in TShock.Players)
                            {
                                player.SetBuff(buff, 3600 * pair.Time);
                            }
                        }
                        else
                            foundplr[0].SetBuff(buff, 3600 * pair.Time);
                    }
                    if (args.Parameters[0] == "-all")
                    {
                        TSPlayer.All.SendInfoMessage("{0} buffed everyone with the {1} set!",
                            args.Player.Name, args.Parameters[1]);
                    }
                    else
                    {
                    args.Player.SendSuccessMessage("Buffed {0} with the {1} set!", 
                        foundplr[0].Name, args.Parameters[1]);
                    foundplr[0].SendSuccessMessage("{0} buffed you with the {1} set!", 
                        args.Player.Name, args.Parameters[1]);
                    }
                }
                else
                    args.Player.SendErrorMessage("Invalid buff set!");
            }
            
        }
        #endregion

        #region mbReload
        public static void mbReload(CommandArgs args)
        {
            SetUpConfig();
            args.Player.SendInfoMessage("Attempted to reload the config file");
        }
        #endregion

        #region permaBuff
        public static void permaBuff(CommandArgs args)
        {
            var player = Tools.GetPlayer(args.Player.Index);
            player.isPermaBuff = !player.isPermaBuff;

            args.Player.SendSuccessMessage("Permabuffs are now " + (player.isPermaBuff ? "on" : "off"));
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