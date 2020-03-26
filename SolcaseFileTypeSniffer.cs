// Solcase File Type Sniffer
// This is a console application that uses TrIDEngine to analyse the file type of *.HST files in the solhist directory
// Created 20 Feb 2020: Kieran Caulfield
// Acknowledgements: (C) 2004-2005 By Marco Pontello - http://mark0.net

using System;
using System.IO;

// Define an alias for TrIDEngine class
using TrIDEngine = TrID.TrIDEngine;

static class Constants
{
    public const string JPG = "JPG";
    public const string HTML = "HTML";
    public const string XLS = "XLS";
    public const string TIF = "TIF/TIFF";
    public const string TMDX = "TMDX/TMVX";
    public const string MPO = "MPO";

}

namespace SolcaseFileTypeSniffer
{
    class FileTypeSniffer
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter a directory path to search.");
                Environment.Exit(1);
            }

            string TargetDir = Path.GetFullPath(args[0]);

            if (!(System.IO.Directory.Exists(TargetDir)))
            {
                System.Console.WriteLine("Directory Not found: ." + TargetDir);
                Environment.Exit(1);
            }

            // Create the 'core' object
            TrIDEngine.PatternEngine PE = new TrIDEngine.PatternEngine();

            // Check for all the defs in current directory
            //string sCurDir = Directory.GetCurrentDirectory();
            string sCurDir = "C:\\TrID\\triddefs_xml\\defs\\";
            //Console.WriteLine(sCurDir + " Checking definitions files...");
            string[] sDefsList = Directory.GetFiles(sCurDir, "*.trid.xml", SearchOption.AllDirectories);
            if (sDefsList.Length == 0)
            {
                Console.WriteLine("No definitions available!");
                Console.WriteLine("Download an up to date defs library from http://mark0.ngi.it");
                Environment.Exit(1);
            }

            // Create Output Header
            Console.WriteLine("FileName,FullFileName,SPO-FILE-NAME,TrIDNumber,FileExt,FileTypeDescription");

            // Submit all defs to the engine
            for (int i = 0; i < sDefsList.Length; i++)
            {
                string sDefFile = sDefsList[i];
                //Console.WriteLine(Path.GetFileName(sDefFile));
                PE.LoadDefinitionByFilePath(sDefFile);
            }

            // This is just for showing how to save & reuse the loaded defs
            System.Collections.ArrayList MyDefPack;
            MyDefPack = (System.Collections.ArrayList)PE.GetDefinitions();
            // Clear all the definitions in memory...
            PE.ClearDefinitions();
            // ... Reload them!
            PE.SetDefs(ref MyDefPack);

            //Console.WriteLine("");
            //Console.WriteLine("Analyzing...");

            //string TargetDir = "C:\\Users\\kieran.caulfield\\OneDrive - Birkett Long LLP\\Documents\\Testing\\Migration\\Undertakings\\";
            string[] sTargetFileList = Directory.GetFiles(TargetDir, "*.HST", SearchOption.AllDirectories);

            if (sTargetFileList.Length == 0)
            {
                Console.WriteLine("No target *.HST files available!");
                Environment.Exit(1);
            }

            try
            {
                foreach (string sTargetFilePath in sTargetFileList)
                {
                    // Submit the file to analyze
                    PE.SubmitFile(sTargetFilePath);

                    // Check if the file is a binary one; if not, display a warning
                    if (!PE.IsBinary())
                    {
                        // TODO: This about this KC, read guide, Console.WriteLine(sTargetFileList[i] + ",,,");
                    }

                    // Start the identification process
                    // (this will show a demo-reminder with the demo ver. of TrIDEngine.DLL)
                    PE.Analyze();

                    // Create a container of the proper type to get the firsts 2 results
                    // in the extended form (instead of simple strings)
                    TrIDEngine.Result[] ExtResults;
                    ExtResults = PE.GetResultsData(1);

                    string sFileName = Path.GetFileName(sTargetFilePath);
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(sTargetFilePath);

                    // JPG does not preview correctly in Edge Internet Explorer but JPEG does so flip the Extn from JPG to JPEG
                    // HTML does not preview correctly in Edge Internet Explorer but HTM does so flip the Extn from HTML to HTM
                    // XLS 32500 (Points) are actually .DOC templates , not XLS Spreadsheets. 
                    // TIF/TIFF is not a valid extn, only the singular can be used (either is fine)
                    // OLE2 objects are emails with attachments, they are in the range 8000 (doc), 93000(msg), 191500(?)
                    // File Extensions must be lower case
                    // MPO files will not preview in Edge Inter Explorer buyt JPEG will show them

                    if (ExtResults[0].Points == 0)
                    {
                        Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".txt" + "," + ExtResults[0].Points + ",TXT,Default File Type to TXT");
                    }
                    else if (ExtResults[0].Points == 8000 || ExtResults[0].Points == 93000 || ExtResults[0].Points == 191500)
                    {
                        if (ExtResults[0].Points == 8000)
                        {
                            Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".doc" + "," + ExtResults[0].Points + ",DOC," + ExtResults[0].ExtraInfo.FileType);
                        }
                        if (ExtResults[0].Points == 93000)
                        {
                            Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".msg" + "," + ExtResults[0].Points + ",MSG," + ExtResults[0].ExtraInfo.FileType);
                        }
                        if (ExtResults[0].Points == 191500)
                        {
                            Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".doc" + "," + ExtResults[0].Points + ",DOC," + ExtResults[0].ExtraInfo.FileType);
                        }

                    }
                    else
                    {
                        switch (ExtResults[0].ExtraInfo.FileExt)
                        {
                            case Constants.JPG:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".jpeg" + "," + ExtResults[0].Points + ",JPEG," + ExtResults[0].ExtraInfo.FileType);
                                break;
                            case Constants.HTML:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".htm" + "," + ExtResults[0].Points + ",HTM," + ExtResults[0].ExtraInfo.FileType);
                                break;
                            case Constants.XLS:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".doc" + "," + ExtResults[0].Points + ",DOC," + ExtResults[0].ExtraInfo.FileType);
                                break;
                            case Constants.TIF:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".tif" + "," + ExtResults[0].Points + ",TIF," + ExtResults[0].ExtraInfo.FileType);
                                break;
                            case Constants.TMDX:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".tmdx" + "," + ExtResults[0].Points + ",TMDX," + ExtResults[0].ExtraInfo.FileType);
                                break;
                            case Constants.MPO:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + ".jpeg" + "," + ExtResults[0].Points + ",MPO," + ExtResults[0].ExtraInfo.FileType);
                                break;
                            default:
                                Console.WriteLine(sFileName + "," + sTargetFilePath + "," + sFileNameNoExt + "." + ExtResults[0].ExtraInfo.FileExt.ToLower() + "," + ExtResults[0].Points + "," + ExtResults[0].ExtraInfo.FileExt + "," + ExtResults[0].ExtraInfo.FileType);
                                break;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("* Error: Unable to open file.");
                Console.WriteLine("  (maybe it's locked by another process?!)");
                //Environment.Exit(1);
            }

        }
    }
}
