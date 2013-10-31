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
    [ApiVersion(1, 14)]
    public class MultiBuff : TerrariaPlugin
    {
        public static mbConfig config { get; set; }
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

        public MultiBuff(Main game)
            : base(game)
        {
            Order = 1;

            config = new mbConfig();
        }

        #region OnInitialize
        public void OnInitialize(EventArgs args)
        {
            #region validBuffsList
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
            validBuffs.Add(19); //Shadow Orb
            validBuffs.Add(25); //Tipsy
            validBuffs.Add(26); //Well Fed
            validBuffs.Add(27); //Fairy
            validBuffs.Add(28); //Werewolf
            validBuffs.Add(29); //Clairvoyance
            validBuffs.Add(34); //Merfolk
            validBuffs.Add(40); //Pet Bunny
            validBuffs.Add(41); //Baby Penguin
            validBuffs.Add(42); //Pet Turtle
            validBuffs.Add(43); //Paladin's Shield
            validBuffs.Add(45); //Baby Eater
            validBuffs.Add(48); //Honey
            validBuffs.Add(49); //Pygmies
            validBuffs.Add(50); //Baby Skeletron Head
            validBuffs.Add(51); //Baby Hornet
            validBuffs.Add(52); //Tiki Spirit
            validBuffs.Add(53); //Pet Lizard
            validBuffs.Add(54); //Pet Parrot
            validBuffs.Add(55); //Baby Truffle
            validBuffs.Add(56); //Pet Sapling
            validBuffs.Add(57); //Wisp
            validBuffs.Add(58); //Rapid Healing
            validBuffs.Add(59); //Shadow Dodge
            validBuffs.Add(60); //Leaf Crystal
            validBuffs.Add(61); //Baby Dinosaur
            validBuffs.Add(62); //Ice Barrier
            validBuffs.Add(63); //Panic!
            validBuffs.Add(64); //Baby Slime
            validBuffs.Add(65); //Eyeball Spring
            validBuffs.Add(66); //Baby Snowman
            validBuffs.Add(71); //Weapon Imbue: Venom
            validBuffs.Add(73); //Weapon Imbue: Cursed Flames
            validBuffs.Add(74); //Weapon Imbue: Fire
            validBuffs.Add(75); //Weapon Imbue: Gold
            validBuffs.Add(76); //Weapon Imbue: Ichor
            validBuffs.Add(77); //Weapon Imbue: Nanites
            validBuffs.Add(78); //Weapon Imbue: Confetti
            validBuffs.Add(79); //Weapon Imbue: Poison
            #endregion;

            Commands.ChatCommands.Add(new Command("tshock.buff.self", multiBuff, "multibuff", "mb"));
            Commands.ChatCommands.Add(new Command("tshock.buff.others", giveMultiBuff, "gmultibuff", "gmb"));
            Commands.ChatCommands.Add(new Command("mb.buffset.self", buffSet, "bset"));
            Commands.ChatCommands.Add(new Command("mb.buffset.others", giveBuffSet, "gbset"));
            Commands.ChatCommands.Add(new Command("mb.admin.reload", mbReload, "reloadmb"));

            SetUpConfig();
        }
        #endregion;

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
                if (!config.AllowDebuffs)
                {
                    if (id > 0 && validBuffs.Contains(id))
                    {
                        args.Player.SetBuff(id, 3600 * config.DefaultMBTime);
                        args.Player.SendSuccessMessage("You have buffed yourself with {0}({1})!",
                            TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                    }
                    else
                        args.Player.SendErrorMessage(string.Format("Invalid buff{0}",
                            validBuffs.Contains(id) ? ": debuff!" : "!"));
                }
                else
                {
                    if (id > 0)
                    {
                        args.Player.SetBuff(id, 3600 * config.DefaultMBTime);
                        args.Player.SendSuccessMessage("You have buffed yourself with {0}({1})!",
                            TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                    }
                    else
                        args.Player.SendErrorMessage("Invalid buff ID!");
                }
            }
        }
        #endregion;

        #region giveMultiBuff
        public static void giveMultiBuff(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /gmb <player> <buff1 id/name> [buff2 id/name] ...");
                return;
            }
            int id = 0;
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (foundplr.Count == 0)
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
                        }
                        id = found[0];
                    }
                    if (!config.AllowDebuffs)
                    {
                        if (id > 0 && validBuffs.Contains(id))
                        {
                            foundplr[0].SetBuff(id, 3600 * config.DefaultMBTime);
                            args.Player.SendSuccessMessage("You have buffed {0} with {1}({2})!",
                                foundplr[0].Name, TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                            foundplr[0].SendSuccessMessage("{0} buffed you with {1}({2})!",
                                args.Player.Name, TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                        }
                        else
                            args.Player.SendErrorMessage(string.Format("Invalid buff{0}", 
                                validBuffs.Contains(id) ? ": debuff!" : "!"));
                    }
                    else
                    {
                        if (id > 0)
                        {
                            foundplr[0].SetBuff(id, 3600 * config.DefaultMBTime);
                            args.Player.SendSuccessMessage("You have buffed {0} with {1}({2})!",
                                foundplr[0].Name, TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                            foundplr[0].SendSuccessMessage("{0} buffed you with {1}({2})!",
                                args.Player.Name, TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id));
                        }
                        else
                            args.Player.SendErrorMessage("Invalid buff!");
                    }
                }
            }
        }
        #endregion;

        #region buffSet
        public static void buffSet(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /bset <buffset name>");
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
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /gbset <player> <buffset name>");
                return;
            }
            BTPair pair;
            var foundplr = TShock.Utils.FindPlayer(args.Parameters[1]);
            if (foundplr.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid player!");
            }
            else if (foundplr.Count > 1)
            {
                List<string> foundPlayers = new List<string>();
                foreach (TSPlayer player in foundplr)
                {
                    foundPlayers.Add(player.Name);
                }
                TShock.Utils.SendMultipleMatchError(args.Player, foundPlayers);
            }
            else
            {
                if (config.BuffSets.TryGetValue(args.Parameters[0], out pair))
                {
                    foreach (int buff in pair.Buffs)
                    {
                        foundplr[0].SetBuff(buff, 3600 * pair.Time);
                    }
                    args.Player.SendSuccessMessage("Buffed {0} with the {1} set!", foundplr[0].Name, args.Parameters[0]);
                    foundplr[0].SendSuccessMessage("{0} buffed you with the {1} set!", args.Player.Name, args.Parameters[0]);
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