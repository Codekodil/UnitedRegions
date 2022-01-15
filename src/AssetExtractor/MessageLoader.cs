using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace AssetExtractor
{
    public static class MessageLoader
    {
        public static List<string> LoadPlatinumMessage(int id)
        {
            RomLoader.EnsureAssetsExist();
            return LoadMessage(Path.Combine(RomLoader.BasePathOverride ?? "", RomLoader.PlatinumPath, "msgdata", "pl_msg.narc", $"pl_msg_{id}.bin"));
        }

        public static List<string> LoadMessage(string path)
        {
            ResourceManager getChar = new ResourceManager($"{typeof(MessageLoader).Assembly.GetName().Name}.DecryptCharLookup", typeof(MessageLoader).Assembly);

            var results = new List<string>();
            //Original source: Thenewpoketext
            using (System.IO.BinaryReader readText = new System.IO.BinaryReader(File.OpenRead(path)))
            {
                var stringCount = readText.ReadUInt16();
                var initialKey = readText.ReadUInt16();
                int key1 = (initialKey * 0x2FD) & 0xFFFF;
                int key2 = 0;
                int realKey = 0;
                bool specialCharON = false;
                int[] currentOffset = new int[stringCount];
                int[] currentSize = new int[stringCount];
                int car = 0;
                bool compressed = false;
                for (int i = 0; i < stringCount; i++) // Reads and stores string offsets and sizes
                {
                    key2 = (key1 * (i + 1) & 0xFFFF);
                    realKey = key2 | (key2 << 16);
                    currentOffset[i] = (int)readText.ReadUInt32() ^ realKey;
                    currentSize[i] = (int)readText.ReadUInt32() ^ realKey;
                }
                for (int i = 0; i < stringCount; i++) // Adds new string
                {
                    key1 = (0x91BD3 * (i + 1)) & 0xFFFF;
                    readText.BaseStream.Position = currentOffset[i];
                    string pokemonText = "";
                    for (int j = 0; j < currentSize[i]; j++) // Adds new characters to string
                    {
                        car = readText.ReadUInt16() ^ key1;
                        #region Special Characters
                        if (car == 0xE000 || car == 0x25BC || car == 0x25BD || car == 0xF100 || car == 0xFFFE || car == 0xFFFF)
                        {
                            if (car == 0xE000)
                            {
                                pokemonText += @"\n";
                            }
                            if (car == 0x25BC)
                            {
                                pokemonText += @"\r";
                            }
                            if (car == 0x25BD)
                            {
                                pokemonText += @"\f";
                            }
                            if (car == 0xF100)
                            {
                                compressed = true;
                            }
                            if (car == 0xFFFE)
                            {
                                pokemonText += @"\v";
                                specialCharON = true;
                            }
                        }
                        #endregion
                        else
                        {
                            if (specialCharON == true)
                            {
                                pokemonText += car.ToString("X4");
                                specialCharON = false;
                            }
                            else if (compressed)
                            {
                                #region Compressed String
                                int shift = 0;
                                int trans = 0;
                                string uncomp = "";
                                while (true)
                                {
                                    int tmp = car >> shift;
                                    int tmp1 = tmp;
                                    if (shift >= 0xF)
                                    {
                                        shift -= 0xF;
                                        if (shift > 0)
                                        {
                                            tmp1 = (trans | ((car << (9 - shift)) & 0x1FF));
                                            if ((tmp1 & 0xFF) == 0xFF)
                                            {
                                                break;
                                            }
                                            if (tmp1 != 0x0 && tmp1 != 0x1)
                                            {
                                                string character = getChar.GetString(tmp1.ToString("X4"));
                                                pokemonText += character;
                                                if (character == null)
                                                {
                                                    pokemonText += @"\x" + tmp1.ToString("X4");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        tmp1 = ((car >> shift) & 0x1FF);
                                        if ((tmp1 & 0xFF) == 0xFF)
                                        {
                                            break;
                                        }
                                        if (tmp1 != 0x0 && tmp1 != 0x1)
                                        {
                                            string character = getChar.GetString(tmp1.ToString("X4"));
                                            pokemonText += character;
                                            if (character == null)
                                            {
                                                pokemonText += @"\x" + tmp1.ToString("X4");
                                            }
                                        }
                                        shift += 9;
                                        if (shift < 0xF)
                                        {
                                            trans = ((car >> shift) & 0x1FF);
                                            shift += 9;
                                        }
                                        key1 += 0x493D;
                                        key1 &= 0xFFFF;
                                        car = Convert.ToUInt16(readText.ReadUInt16() ^ key1);
                                        j++;
                                    }
                                }
                                #endregion
                                pokemonText += uncomp;
                            }
                            else
                            {
                                string character = getChar.GetString(car.ToString("X4"));
                                pokemonText += character;
                                if (character == null)
                                {
                                    pokemonText += @"\x" + car.ToString("X4");
                                }
                            }
                        }
                        key1 += 0x493D;
                        key1 &= 0xFFFF;
                    }
                    results.Add(pokemonText);
                    //dataGridView6.Rows.Add("", pokemonText);
                    //dataGridView6.Rows[i].HeaderCell.Value = i.ToString();
                    //progressBar1.Value = i;
                    compressed = false;
                }
            }
            return results;
        }
    }
}
