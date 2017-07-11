/*
 * Copyright (C) 2017 Andrew Yeung
 * 
 * All rights reserved
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flatten_source_directory
{

    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("ERROR: Invalid arguments");
                Console.WriteLine("Usage: flatten_directory.exe <target directory> <output directory>");
                return;
            }

            string targetDir = args[0];
            string outputDir = args[1];

            Console.WriteLine("-----------------------------");
            Console.WriteLine("Flattening source directories...");
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Target directory: " + targetDir);
            Console.WriteLine("Output directory: " + outputDir);

            string includeDir = "";
            string srcDir = "";

            if (!Directory.Exists(targetDir))
            {
                Console.WriteLine("ERROR: Target directory " + targetDir + " doesn't exist");
                return;
            }
            else
            {
                includeDir = Path.Combine(targetDir, "include");
                srcDir = Path.Combine(targetDir, "src");
                if (!Directory.Exists(includeDir))
                {
                    Console.WriteLine("ERROR: Target directory must contain 'include' directory");
                    return;
                }
                if (!Directory.Exists(srcDir))
                {
                    Console.WriteLine("ERROR: Target directory must contain 'src' directory");
                    return;
                }
            }

            if (!Directory.Exists(outputDir))
            {
                Console.WriteLine("ERROR: Output directory " + outputDir + " doesn't exist");
                return;
            }


            Console.WriteLine("\n-----------------------------");
            Console.WriteLine("Include directory structure:");
            Console.WriteLine("-----------------------------");

            PrintDir(includeDir, 0);

            Console.WriteLine("\n-----------------------------");
            Console.WriteLine("Src directory structure:");
            Console.WriteLine("-----------------------------");

            PrintDir(srcDir, 0);

            Console.WriteLine("\n-----------------------------");
            Console.WriteLine("Replacement list");
            Console.WriteLine("-----------------------------");

            List<KeyValuePair<string, string>> replacementItems = new List<KeyValuePair<string, string>>();
            List<string> currentPath = new List<string>();
            replacementItems = GetReplacementData(includeDir, currentPath, replacementItems);

            foreach (KeyValuePair<string, string> replacementItem in replacementItems)
            {
                Console.WriteLine(String.Format("{0,-50} --->    {1,-30}", replacementItem.Key, replacementItem.Value));
            }

            Console.WriteLine("\n-----------------------------");
            Console.WriteLine("Press enter to proceed...");

            Console.ReadLine();

            currentPath.Clear();
            FlattenDir(includeDir, currentPath, replacementItems, outputDir);
            FlattenDir(srcDir, currentPath, replacementItems, outputDir);

            Console.WriteLine("\nDONE");
            Console.WriteLine("-----------------------------");
        }

        static void PrintDir(string dir, int depth)
        {
            try
            {
                String prefix = new String(' ', 2 * depth);
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    Console.WriteLine(prefix + subDir.Remove(0, dir.Length));

                    foreach (string file in Directory.GetFiles(subDir))
                    {
                        // TODO HACK: Hardcoded stuff
                        Console.WriteLine(prefix + "  |" + file.Remove(0, subDir.Length + 1));
                    }
                    PrintDir(subDir, depth + 1);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        static List<KeyValuePair<string, string>> GetReplacementData(string includeDir, List<string> currentPath, List<KeyValuePair<string, string>> replacementItems)
        {
            try
            {
                foreach (string subDir in Directory.GetDirectories(includeDir))
                {
                    string subDirLast = subDir.Remove(0, includeDir.Length + 1);

                    currentPath.Add(subDirLast);
                    string currentPathString = string.Join("/", currentPath.ToArray());

                    foreach (string file in Directory.GetFiles(subDir))
                    {
                        // TODO HACK: Hardcoded stuff
                        string filename = file.Remove(0, subDir.Length + 1);
                        string name = Path.GetFileNameWithoutExtension(filename);
                        string extension = Path.GetExtension(filename);

                        string oldInclude = "<" + currentPathString + "/" + filename + ">";
                        string newInclude = "\"" + string.Join("_", string.Join("_", currentPath), name) + extension + "\"";

                        KeyValuePair<string, string> replacementItem = new KeyValuePair<string, string>(oldInclude, newInclude);
                        replacementItems.Add(replacementItem);
                    }

                    replacementItems = GetReplacementData(subDir, currentPath, replacementItems);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return replacementItems;
        }

        static void FlattenDir(string dir, List<string> currentPath, List<KeyValuePair<string, string>> replacementItems, string outputDir)
        {
            try
            {
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    string subDirLast = subDir.Remove(0, dir.Length + 1);

                    currentPath.Add(subDirLast);
                    string currentPathString = string.Join("/", currentPath.ToArray());

                    foreach (string file in Directory.GetFiles(subDir))
                    {
                        // TODO HACK: Hardcoded stuff and duplicated work from the replacement identification method
                        string filename = file.Remove(0, subDir.Length + 1);
                        string name = Path.GetFileNameWithoutExtension(filename);
                        string extension = Path.GetExtension(filename);

                        string newFilename = string.Join("_", string.Join("_", currentPath), name) + extension;

                        string text = File.ReadAllText(file);

                        foreach (KeyValuePair<string, string> replacementItem in replacementItems)
                        {
                            text = text.Replace(replacementItem.Key, replacementItem.Value);
                        }

                        File.WriteAllText(Path.Combine(outputDir, newFilename), text);
                    }

                    FlattenDir(subDir, currentPath, replacementItems, outputDir);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

    }
}
