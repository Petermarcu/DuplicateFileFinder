﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Security;

namespace DuplicateFileFinder
{

    class Program
    {

        // Fields

        public static MultiDictionary<long, FileInfo> fileSizeHashDictionary = new MultiDictionary<long, FileInfo>();

        public static List<List<FileInfo>> duplicateFileList = new List<List<FileInfo>>();

        static void Main(string[] args)
        {
            if (args.Count<String>() < 1 || args[0] == "/?")
            {
                ShowUsage();
                return;
            }

            try
            {
                // Get the list of all the files in the directory recursively
                IEnumerable<FileInfo> listOfFiles = new DirectoryInfo(args[0]).EnumerateFiles("*", SearchOption.AllDirectories);

                List<List<FileInfo>> listOfIdenticalFiles = new List<List<FileInfo>>();

                foreach (FileInfo fileInfo in listOfFiles)
                {
                    fileSizeHashDictionary.Add(fileInfo.Length, fileInfo);
                }

                FileInfo fileInfo1;
                FileInfo fileInfo2;
                foreach (long key in fileSizeHashDictionary.Keys)
                {
                    // TODO: Go backwards instead and remove from the list as we find duplicates.
                    // TODO: Prevent scanning the first in the list for each comparison. 
                    // TODO: Whats the best way to cast IEnumerable to List?
                    List<FileInfo> sameLengthList = (List<FileInfo>)fileSizeHashDictionary[key];

                    if (sameLengthList.Count > 1)
                    {
                        while (sameLengthList.Count > 1)
                        {
                            List<FileInfo> currentIdenticalFilesList = new List<FileInfo>();
                            for (int j = 1; j < sameLengthList.Count; j++)
                            {
                                fileInfo1 = sameLengthList[0];
                                fileInfo2 = sameLengthList[j];

                                if ((bool)CompareFiles(fileInfo1, fileInfo2))
                                {
                                    if (!currentIdenticalFilesList.Contains(fileInfo1))
                                    {
                                        currentIdenticalFilesList.Add(fileInfo1);
                                    }

                                    if (!currentIdenticalFilesList.Contains(fileInfo2))
                                    {
                                        currentIdenticalFilesList.Add(fileInfo2);
                                    }
                                }
                            }
                            // Remove the first because its either tracked in the identical list or has no dupes.

                            sameLengthList.RemoveAt(0);

                            foreach (FileInfo fileInfo in currentIdenticalFilesList)
                            {
                                sameLengthList.Remove(fileInfo);
                            }

                            listOfIdenticalFiles.Add(currentIdenticalFilesList);

                            if (currentIdenticalFilesList.Count > 1)
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (FileInfo fi in currentIdenticalFilesList)
                                {
                                    sb.Append(fi.FullName + ",");
                                }
                                //Console.WriteLine("Duplicate Count: " + currentIdenticalFilesList.Count.ToString());
                                Console.WriteLine(sb.ToString());
                            }

                        }
                    }
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("Path was null");
                Console.WriteLine(ane.Message + ane.Data);
                Console.WriteLine(ane.InnerException.Message + ane.Data);
            }
            catch (SecurityException se)
            {
                Console.WriteLine("Dont have access to the path");
                Console.WriteLine(se.Message + se.Data);
                Console.WriteLine(se.InnerException.Message + se.Data);
            }
            catch (PathTooLongException ptle)
            {
                Console.WriteLine("Path was too long");
                Console.WriteLine(ptle.Message + ptle.Data);
                Console.WriteLine(ptle.InnerException.Message + ptle.Data);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine("Invalid Path");
                Console.WriteLine(ae.Message + ae.Data);
                Console.WriteLine(ae.InnerException.Message + ae.Data);
            }

            // Console.ReadKey();
            return;
        }

        static void ShowUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage: LocateDuplicateFiles.exe <directory>");
            Console.WriteLine("");
            Console.WriteLine("Pass the directory to search recursively");
        }

        public static bool? CompareFiles(FileInfo targetFileInfo, FileInfo updatedFileInfo)
        {
            if (targetFileInfo.Length != updatedFileInfo.Length)
            {
                return false;
            }

            using (FileStream targetStream = File.OpenRead(targetFileInfo.FullName))
            {
                using (FileStream updatedStream = File.OpenRead(updatedFileInfo.FullName))
                {
                    if (targetStream.Length != updatedStream.Length)
                    {
                        return false;
                    }

                    byte[] targetBuffer = new byte[16 * 1024];
                    byte[] updatedBuffer = new byte[16 * 1024];
                    int targetReadLength;
                    int updatedReadLength;

                    do
                    {
                        targetReadLength = targetStream.Read(targetBuffer, 0, targetBuffer.Length);
                        updatedReadLength = updatedStream.Read(updatedBuffer, 0, updatedBuffer.Length);

                        if (targetReadLength != updatedReadLength)
                        {

                            return false;

                        }

                        for (int i = 0; i < targetReadLength; ++i)
                        {
                            if (targetBuffer[i] != updatedBuffer[i])
                            {
                                return false;
                            }
                        }

                    } while (0 < targetReadLength);
                }
            }
            return true;
        }
    }
}