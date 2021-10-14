using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BepInEx_Mod_Switch_CLI
{
    class Program
    {
        public static string version = "1.0.0";
        public static string cliDir = Directory.GetCurrentDirectory();
        public static string bepConfig = cliDir + "/BepInEx/config/";
        public static string bepPlugins = cliDir + "/BepInEx/plugins/";
        public static string bepPatchers = cliDir + "/BepInEx/patchers/";

        public static string bmsProfilesDir = cliDir + "/bms_data/profiles/";
        public static string bmsProfilesList = cliDir + "/bms_data/profiles.dat";
        public static string bmsNexIDList = cliDir + "/bms_data/index.dat";
        public static string bmsCurrentProfile = cliDir + "/bms_data/current.dat";
        public static List<List<string>> profileList = new List<List<string>>();
        public static List<List<string>> nexIDList = new List<List<string>>();
        public static string CurrentProfile;

        static void Main(string[] args)
        {

            if (!Directory.Exists(bmsProfilesDir))
            {
                Directory.CreateDirectory(bmsProfilesDir);
            }

            if (!File.Exists(bmsProfilesList))
            {
                string line = "ID\tName";
                File.AppendAllText(bmsProfilesList, line);
            }

            if (!File.Exists(bmsNexIDList))
            {
                string line = "ID\tName";
                File.AppendAllText(bmsNexIDList, line);
            }

            if (!File.Exists(bmsCurrentProfile))
            {
                string line = "0";
                File.WriteAllText(bmsCurrentProfile, line);
            }

            CurrentProfile = File.ReadAllText(bmsCurrentProfile);

            profileList = loadProfileList();
            nexIDList = loadNexIdList();

            Console.Clear();
            switch (args[0])
            {
                case "-h":
                    Console.WriteLine("");
                    Console.WriteLine("Commands:");
                    Console.WriteLine("-v\t\t\t\tVersion info.");
                    Console.WriteLine("");
                    Console.WriteLine("-list\t\t\t\tList all profiles.");
                    Console.WriteLine("");
                    Console.WriteLine("-profile\t\t\tCurrent Profile.");
                    Console.WriteLine("");
                    Console.WriteLine("-switch <id>\t\t\tSwitch to mod profile by id.");
                    Console.WriteLine("-switch \"<name>\"\t\tSwitch to mod profile by name.");
                    Console.WriteLine("");
                    Console.WriteLine("-add \"<name>\" -e\t\tAdd new empty profile.");
                    Console.WriteLine("-add \"<name>\" -c\t\tAdd new profile contains current mods and configs.");
                    Console.WriteLine("");
                    Console.WriteLine("-remove <id>\t\t\tRemove profile by id.");
                    Console.WriteLine("-remove \"<name>\"\t\tRemove profile by name.");
                    break;
                case "-v":
                    Console.WriteLine("");
                    Console.WriteLine("You running BepInEx Mod Switch CLI version " + version);
                    break;
                case "-add":
                    string profileName = args[1];
                    switch (args[2])
                    {
                        case "-e":
                            addProfile(profileName,false);
                            break;
                        case "-c":
                            addProfile(profileName, true);
                            break;
                    }
                    break;
                case "-remove":
                    RemoveProfile(args[1]);
                    break;
                case "-list":

                    foreach (var dat in profileList)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Profile id:\t\t" + dat[0]);
                        Console.WriteLine("Profile name:\t\t" + dat[1]);
                        Console.WriteLine("Profile Configs:\t" + Directory.GetFiles(bmsProfilesDir + "/" + dat[0] + "/config/", "*.cfg", SearchOption.AllDirectories).Length);
                        Console.WriteLine("Profile Plugins:\t" + Directory.GetFiles(bmsProfilesDir + "/" + dat[0] + "/plugins/", "*.dll", SearchOption.AllDirectories).Length);
                        Console.WriteLine("Profile Patchers:\t" + Directory.GetFiles(bmsProfilesDir + "/" + dat[0] + "/patchers/", "*.dll", SearchOption.AllDirectories).Length);

                    }
                    break;
                case "-switch":
                    SwitchProfile(args[1]);
                    break;
                case "-profile":
                    bool ProfileExists = false;
                    string idToSearch = File.ReadAllText(bmsCurrentProfile);
                    foreach (var dat in profileList)
                    {
                        if (idToSearch.Equals(dat[0]))
                        {
                            ProfileExists = true;
                            Console.WriteLine("");
                            Console.WriteLine("Profile id:\t\t" + dat[0]);
                            Console.WriteLine("Profile name:\t\t" + dat[1]);
                            Console.WriteLine("Profile Configs:\t" + Directory.GetFiles(bmsProfilesDir + "/" + dat[0] + "/config/", "*.cfg", SearchOption.AllDirectories).Length);
                            Console.WriteLine("Profile Plugins:\t" + Directory.GetFiles(bmsProfilesDir + "/" + dat[0] + "/plugins/", "*.dll", SearchOption.AllDirectories).Length);
                            Console.WriteLine("Profile Patchers:\t" + Directory.GetFiles(bmsProfilesDir + "/" + dat[0] + "/patchers/", "*.dll", SearchOption.AllDirectories).Length);
                            break;
                        }
                    }
                    if (!ProfileExists)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Message:\t\tProfile not found.");
                    }
                    break;
            }
        }

        public static bool ProfileExists(string name)
        {
            bool ProfileExists = false;
            foreach (var dat in profileList)
            {
                if (name.Equals(dat[1]))
                {
                    ProfileExists = true;
                    break;
                }
            }
            return ProfileExists;
        }

        public static int GetNextId()
        {
            return nexIDList.Count()+1;
        }

        public static void SwitchProfile(string nameORid)
        {
            bool ProfileExists = false;
            string idToSwitch = "0";
            string nameToSwitch = "";
            foreach (var dat in profileList)
            {
                if (nameORid.Equals(dat[1]))
                {
                    idToSwitch = dat[0];
                    nameToSwitch = dat[1];
                    ProfileExists = true;
                    break;
                }
            }

            if (!ProfileExists)
            {
                foreach (var dat in profileList)
                {
                    if (nameORid.Equals(dat[0]))
                    {
                        idToSwitch = dat[0];
                        nameToSwitch = dat[1];
                        ProfileExists = true;
                        break;
                    }
                }
            }

            if (ProfileExists)
            {
                Console.WriteLine("");
                Console.WriteLine("Switch to Profile\t"+ idToSwitch);
                Console.WriteLine("Profile Name:\t"+ nameToSwitch);

                Directory.Delete(bepPatchers, true);
                Directory.Delete(bepPlugins, true);
                Directory.Delete(bepConfig, true);

                Directory.CreateDirectory(bepPatchers);
                Directory.CreateDirectory(bepPlugins);
                Directory.CreateDirectory(bepConfig);

                Copy(bmsProfilesDir + "/" + idToSwitch + "/config/", bepConfig);
                Copy(bmsProfilesDir + "/" + idToSwitch + "/plugins/", bepPlugins);
                Copy(bmsProfilesDir + "/" + idToSwitch + "/patchers/", bepPatchers);

                File.WriteAllText(bmsCurrentProfile, idToSwitch);
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("Message:\t\tProfile not found.");
            }
        }

        public static void RemoveProfile(string nameORid)
        {
            bool ProfileExists = false;
            string idToDelete = "0";
            foreach (var dat in profileList)
            {
                if (nameORid.Equals(dat[1]))
                {
                    idToDelete = dat[0];
                    ProfileExists = true;
                    break;
                }
            }

            if (!ProfileExists)
            {
                foreach (var dat in profileList)
                {
                    if (nameORid.Equals(dat[0]))
                    {
                        idToDelete = dat[0];
                        ProfileExists = true;
                        break;
                    }
                }
            }

            if (ProfileExists)
            {
                string profiletext = File.ReadAllText(bmsProfilesList);
                string text = "";
                foreach (var val in profiletext.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    bool idequals = false;
                    foreach (string id in val.Split('\t'))
                    {
                        if (id == idToDelete)
                        {
                            idequals = true;
                            break;
                        }
                    }
                    if (idequals) continue;
                    text += "\n" + val;
                }
                File.WriteAllText(bmsProfilesList, text.Trim());
                Console.WriteLine("Message:\t\tProfile has removed from the list.");
                Directory.Delete(bmsProfilesDir + "/" + idToDelete + "/",true);
                
            } 
            else
            {
                Console.WriteLine("");
                Console.WriteLine("Message:\t\tProfile not found.");
            }

        }

        public static void addProfile(string name, bool doCopy)
        {
            int newId = GetNextId();
            Console.WriteLine("Add profile...");
            Console.WriteLine("");
            Console.WriteLine("Profile name:\t\t" + name);
            Console.WriteLine("Profile id:\t\t" + newId);
            Console.WriteLine("Profile mode:\t\t" + (doCopy ? "Add new profile contains current mods and configs." : "Add new empty profile."));

            if (!ProfileExists(name))
            {
                string line = "\n";
                List<string> data = new List<string>();
                data.Add(newId.ToString());
                data.Add(name);
                foreach (var dat in data)
                {
                    line += dat + "\t";
                }
                File.AppendAllText(bmsProfilesList, line);
                File.AppendAllText(bmsNexIDList, line);

                if (!Directory.Exists(bmsProfilesDir + "/" + newId + "/")) Directory.CreateDirectory(bmsProfilesDir + "/" + newId + "/");
                if (!Directory.Exists(bmsProfilesDir + "/" + newId + "/config/")) Directory.CreateDirectory(bmsProfilesDir + "/" + newId + "/config/");
                if (!Directory.Exists(bmsProfilesDir + "/" + newId + "/plugins/")) Directory.CreateDirectory(bmsProfilesDir + "/" + newId + "/plugins/");
                if (!Directory.Exists(bmsProfilesDir + "/" + newId + "/patchers/")) Directory.CreateDirectory(bmsProfilesDir + "/" + newId + "/patchers/");

                if (doCopy)
                {
                    Copy(bepConfig, bmsProfilesDir + "/" + newId + "/config/");
                    Copy(bepPlugins, bmsProfilesDir + "/" + newId + "/plugins/");
                    Copy(bepPatchers, bmsProfilesDir + "/" + newId + "/patchers/");
                }
            } 
            else
            {
                Console.WriteLine("");
                Console.WriteLine("Cant add profile with name '" + name + "'");
                Console.WriteLine("Profile allready exists.");
            }
        }

        public static List<List<string>> loadProfileList()
        {
            string text = File.ReadAllText(bmsProfilesList);
            int count = 0;
            foreach (var val in text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                List<string> tmpDat = new List<string>();
                count++;
                if (count == 1) continue;
                var pairs = val.Split(new string[] { "\t" }, StringSplitOptions.None);
                foreach (var str in pairs)
                {
                    tmpDat.Add(str);
                }
                profileList.Add(tmpDat);
            }
            return profileList;
        }

        public static List<List<string>> loadNexIdList()
        {
            string text = File.ReadAllText(bmsNexIDList);
            int count = 0;
            foreach (var val in text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                List<string> tmpDat = new List<string>();
                count++;
                if (count == 1) continue;
                var pairs = val.Split(new string[] { "\t" }, StringSplitOptions.None);
                foreach (var str in pairs)
                {
                    tmpDat.Add(str);
                }
                nexIDList.Add(tmpDat);
            }
            return nexIDList;
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
