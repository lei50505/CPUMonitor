using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CPUMonitor.source.util
{
    public static class UConfig
    {
        private static string path = Directory.GetCurrentDirectory() + "/config.txt";

        public static void add(string key, string value)
        {
            string[] lines = new string[0];
            if (!File.Exists(path))
            {
                StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
                sw.Close();
            }
            else
            {
                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Split(':')[0].Equals(key))
                {
                    lines[i] = lines[i].Split(':')[0] + ':' + value;
                    File.WriteAllLines(path, lines);
                    return;
                }
            }
            string[] newLines = new string[lines.Length + 1];
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];

            }
            newLines[lines.Length] = key + ':' + value;
            File.WriteAllLines(path, newLines);
        }

        public static string get(string key)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Split(':')[0].Equals(key))
                {
                    return lines[i].Split(':')[1];
                }
            }
            return null;
        }
    }
    }
