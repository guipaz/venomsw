using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VenomSW.RuneAnalyzer;

namespace VenomSW
{
    public class Runner
    {
        public const string PROCESS_NAME = "mobizen";
        public const string PATH = "E:\\dev\\venomsw\\";
        public const bool DEBUG_MODE = true;

        public static bool RUN_DEBUG = false;
        public static Random random = new Random();

        private static BasicAnalyzer analyzer;

        Dictionary<string, Bitmap> referenceImages;
        int completedDungeons = 0;
        int runesAcquired = 0;

        public void Start()
        {
            analyzer = new BasicAnalyzer();
            referenceImages = new Dictionary<string, Bitmap>();
            State currentState = DefineStates();

            if (DEBUG_MODE)
            {
                Console.WriteLine("DEBUG MODE");
                Console.WriteLine("ESC - Close");
                Console.WriteLine("S - Save screenshot");
                Console.WriteLine("C - Show coordinates");
                Console.WriteLine("Enter - Run");
                Console.WriteLine("D - Run in debug mode (Press any key for next check)");
                Console.WriteLine("F - Save references");
                int savedImages = 0;
                while (true)
                {
                    ConsoleKey k = Console.ReadKey().Key;
                    Console.Write("\n");

                    if (k == ConsoleKey.Escape)
                        return;

                    if (k == ConsoleKey.T)
                    {
                        TesseractTest.Run();
                    }
                    else if (k == ConsoleKey.S)
                    {
                        Bitmap b = User32.CaptureApplication(PROCESS_NAME);
                        b.Save(PATH + "images\\saved\\saved_" + savedImages++ + ".png");
                        Console.WriteLine("Image saved");
                    }
                    else if (k == ConsoleKey.C)
                    {
                        Point p;
                        User32.GetCursorPos(out p);
                        Point p2 = ClickOnPointTool.GetWindowCoordinates(Process.GetProcessesByName("mobizen")[0].MainWindowHandle, p);
                        Console.WriteLine(p2);
                    }
                    else if (k == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else if (k == ConsoleKey.D)
                    {
                        RUN_DEBUG = true;
                        break;
                    } else if (k == ConsoleKey.F)
                    {
                        foreach (var pattern in Pattern.All)
                        {
                            pattern.reference.Save(PATH + "images\\references\\" + pattern.name + ".png");
                        }

                        Console.WriteLine("References saved");
                    }
                }
            }

            Console.Write("Repeat: ");
            string repeat = Console.ReadLine();
            int maxDungeons = 0;
            int.TryParse(repeat, out maxDungeons);

            Comparer c = new Comparer();
            bool active = true;

            while (active)
            {
                if (RUN_DEBUG)
                {
                    ConsoleKey k = Console.ReadKey().Key;
                }
                else
                {
                    Thread.Sleep(currentState.cooldown * 1000);
                }

                Bitmap screen = User32.CaptureApplication(PROCESS_NAME);
                Console.WriteLine("Checking...");
                foreach (Road r in currentState.roads)
                {
                    if (r.pattern != null && r.pattern.reference != null && c.IsValid(r.pattern, screen))
                    {
                        Console.WriteLine("Found pattern");
                        if (r.clickX != 0 || r.clickY != 0)
                        {
                            if (r.wait > 0)
                                Thread.Sleep(r.wait * 1000);
                            
                            Execute(r, screen);
                        }

                        // waits a second
                        Thread.Sleep(1000);

                        Console.WriteLine("Changing to " + r.destination.name);

                        // Counts dungeons completed
                        if (r.destination.name.Equals("Team selection") &&
                            !currentState.name.Equals("Energy shop leave"))
                        {
                            completedDungeons++;
                            Console.WriteLine("Dungeons completed: " + completedDungeons);

                            if (maxDungeons > 0 && completedDungeons >= maxDungeons)
                            {
                                Console.WriteLine("Maximum amount of dungeons reached");
                                active = false;
                            }
                        }

                        currentState = r.destination;

                        break;
                    }
                }
            }

            Console.WriteLine("VenomSW is done working, press any key to exit.");
            Console.ReadKey();
        }
        
        void Execute(Road road, Bitmap screen)
        {
            road.function?.Invoke(road);

            // clicks
            var proc = Process.GetProcessesByName("mobizen")[0];

            Rectangle rect = road.pattern.coordinates;
            if (road.pattern.click != Rectangle.Empty)
                rect = road.pattern.click;

            float ratio = screen.Width / (float) road.pattern.original.Size.Width;
            rect.X = (int)(rect.X * ratio);
            rect.Y = (int)(rect.Y * ratio);
            rect.Width = (int)(rect.Width * ratio);
            rect.Height = (int)(rect.Height * ratio);

            Point p = new Point(random.Next(rect.X + 1, rect.X + rect.Width - 1),
                                random.Next(rect.Y + 1, rect.Y + rect.Height - 1));

            ClickOnPointTool.ClickOnPoint(proc.MainWindowHandle, p);
        }

        State DefineStates()
        {
            State teamSelection = new State("Team selection", 2);
            State teamConfirmation = new State("Team confirmation", 5); // can ask for more energy, have internet instability or go to the dungeon

            State internetInstability = new State("Internet instability", 5); // goes to team confirmation

            // if there's the need for more energy
            State moreEnergyRequired = new State("More energy required", 2);
            State energyShop = new State("Energy shop", 3);
            State energyBuyingConfirmation = new State("Energy buying confirmation", 3);
            State energyBuyingOk = new State("Energy buying ok", 3);
            State energyShopLeave = new State("Energy shop leave", 3); // goes to team selection

            // entered the dungeon
            State defeatRevive = new State("Defeat revive", 2);
            State defeated = new State("Defeated", 2);
            State victory = new State("Victory", 2);
            State victoryChest = new State("Victory Chest", 2);
            State runeGained = new State("Rune gained", 2); // get button
            State miscGained = new State("Misc gained", 2); // ok button
            
            // custom patterns
            Pattern.Load();
            
            // selecting team
            teamSelection.roads = new Road[] { new Road( Pattern.Get("team_selection"), teamConfirmation, 537, 238) };
            teamConfirmation.roads = new Road[] { new Road(Pattern.Get("more_energy_required"), energyShop, 0, 0),
                                                  new Road(null, internetInstability, 0, 0),
                                                  new Road(Pattern.Get("defeat_revive"), defeatRevive, 425, 225),
                                                  new Road(Pattern.Get("victory"), victory, 340, 65)};

            // internet instability
            internetInstability.roads = new Road[] { new Road(null, teamConfirmation, 0, 0) };

            // buying more energy
            energyShop.roads = new Road[] { new Road(Pattern.Get("energy_shop"), energyBuyingConfirmation, 0, 0) };
            energyBuyingConfirmation.roads = new Road[] { new Road(Pattern.Get("energy_buying_confirmation"), energyBuyingOk, 0, 0) };
            energyBuyingOk.roads = new Road[] { new Road(Pattern.Get("energy_buying_ok"), energyShopLeave, 0, 0) };
            energyShopLeave.roads = new Road[] { new Road(Pattern.Get("energy_shop_leave"), teamSelection, 0, 0) };
            
            // farming
            defeatRevive.roads = new Road[] { new Road(Pattern.Get("defeated"), defeated, 350, 70) };
            defeated.roads = new Road[] { new Road(Pattern.Get("dungeon_replay"), teamSelection, 240, 190) };

            victory.roads = new Road[] { new Road(Pattern.Get("victory"), victoryChest, 340, 65, null, 1) };
            victoryChest.roads = new Road[] { new Road(Pattern.Get("rune_gained"), runeGained, 390, 275,
                                            (r) =>
                                            {
                                                Bitmap screen = User32.CaptureApplication(PROCESS_NAME);

                                                Rectangle crop = r.pattern.coordinates;
                                                float ratio = r.pattern.GetRatio(screen);
                                                crop.X = (int)(crop.X * ratio);
                                                crop.Y = (int)(crop.Y * ratio);
                                                crop.Width = (int)(crop.Width * ratio);
                                                crop.Height = (int)(crop.Height * ratio);

                                                Bitmap cropped = Comparer.Crop(screen, crop);

                                                //TODO verify instead of just call
                                                analyzer.ShouldGetRune(cropped);

                                                try
                                                {
                                                    cropped.Save(PATH + "\\runes\\rune_" + runesAcquired + ".png");
                                                } catch (Exception e)
                                                {
                                                    Console.WriteLine("Exception: " + e.Message);
                                                }
                                            }),
                                         new Road(Pattern.Get("misc_gained"), miscGained, 340, 280),
                                         new Road(Pattern.Get("victory"), victoryChest, 340, 65, null, 1)}; // try again

            runeGained.roads = new Road[] { new Road(Pattern.Get("dungeon_replay"), teamSelection, 240, 190) };
            miscGained.roads = new Road[] { new Road(Pattern.Get("dungeon_replay"), teamSelection, 240, 190) };

            return teamSelection; // starting node
        }

        Bitmap LoadReference(string name)
        {
            if (!referenceImages.ContainsKey(name))
            {
                string path = PATH + "configurer\\" + name + ".png";
                if (!File.Exists(path))
                    path = PATH + "images\\mobizen\\" + name + ".png";
                referenceImages[name] = new Bitmap(path);
            }

            return referenceImages[name];
        }
    }
}
