using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

/**
    * Provides functionality to create Performance File for third-party statistics tools.
    * Accepts  Session Directory path :
    •	retrieves Performance file name to be created 
    •	Iterates over all Trials in the given Session extracting system targets and  Subject selected objects and addign them to the Performance file.
    Performance File consists of
    •	 data from all Trials in a Session if a Session directory was chosen by the user
    •	data from all trials in all sessions if an experiment directory was chosen by the user

    * @author Ayse Bahar Akbal
    * @date 05-03-2007
    */

namespace AnalysisTool
{
    class PerformanceFileCreation
    {
        private static bool newPerfFileOpen = false;

        public static String performanceFileName = null;

        // property accessor and Mutator for Performance File name
        public static String PerformanceFilename{
            get
            {

                return performanceFileName;
            }
            set
            {
                performanceFileName = value;
            }

        }

        public static String getPerformanceFileName(String sessionName, String sessionPath) 
        {
            String performanceFileName = null;
            String performanceFilePath = null;
            try
            {
                performanceFileName = Regex.Replace(sessionName, ".session$", ".perf");

                performanceFilePath = Regex.Replace(sessionPath, "Sess([0-9])+", "perf");

                performanceFilePath += "\\" + performanceFileName;

                return performanceFilePath;
            }
            catch (Exception e)
            {
                throw new ATLogicException(" Error retrieving Performance File name. ");
            }
        }

        public static String create(String performanceFilePath, String sessionName) 
        {
            String performanceFileName = null;
            String perfDirectory = null;
            try
            {
                performanceFileName = Regex.Replace(sessionName, ".session$", ".perf");
                DirectoryInfo perfDir = Directory.GetParent(performanceFilePath);
                perfDirectory = perfDir.FullName;
                // If C:\EVS\experiments\CurrentExperimentName\perf directory does not exit yet, create it.
                if (!Directory.Exists(perfDirectory))
                    Directory.CreateDirectory(perfDirectory);

                StreamWriter header = new StreamWriter(performanceFilePath);
                setPerfFileStatus(true);
                header.WriteLine("# " + performanceFileName);
                header.WriteLine("#");
                header.WriteLine("# trialNum = trial number");
                header.WriteLine("# condNum = condition number of the trial");
                header.WriteLine("# totNumObjs = total number of objects");
                header.WriteLine("# totNumTargs = total number of targets");
                header.WriteLine("# objTargs = which objects are targets (comma-separated list)");
                header.WriteLine("# subjTargs = which objects the subject chose as targets (comma-separated list)");
                header.WriteLine("#");
                header.WriteLine("#");
                header.WriteLine("#");
                header.WriteLine("# trialNum condNum totNumObjs totNumTargs objTargs subjTargs");
                header.WriteLine("# -------- ------- ---------- ----------- -------- ---------");
                header.WriteLine("# 1 1 2 1 1 2");
                header.WriteLine("# 2 3 4 2 1,2 3,4");
                header.WriteLine("# ...");
                header.WriteLine();
                //header.Flush();
                header.Close();

                return performanceFilePath;
            }
            catch (Exception e)
            {
                throw new ATLogicException(" Error creating Performance File ");
            }
        }

        public static void parseData(String geLogFileName, String performanceFilePath) 
        {
            try
            {
                StreamReader reader = File.OpenText(geLogFileName);

                StreamWriter trialInfo = File.AppendText(performanceFilePath);

                //get all the text in the file
                String fileStr = reader.ReadToEnd();

                // Write each trial's info on a new line
                trialInfo.WriteLine();

                // Start parsing the text, extract info and append to the performance file.
                // trial number:
                String trialName = Regex.Match(fileStr, @"TrialName\s(.*)\r\n").ToString();
                String trialNumber = Regex.Match(Regex.Match(trialName, "Tr([0-9])+").ToString(),
                                                     "([0-9])+").ToString();
                
                trialInfo.Write(trialNumber + " ");

                // condition number:
                String conditionNo = Regex.Match(Regex.Match(fileStr, @"Condition\s(\d*)").ToString(),
                                                    "([0-9])+").ToString();
                trialInfo.Write(conditionNo + " ");

                // total number of objects:
                String totalNumObj = Regex.Match(Regex.Match(fileStr, @"NumObjects\s(\d*)").ToString(),
                                                    "([0-9])+").ToString();
                trialInfo.Write(totalNumObj + " ");

                // total number of targets:
                String totalNumTargets = Regex.Match(Regex.Match(fileStr, @"NumTargets\s(\d*)").ToString(),
                                                    "([0-9])+").ToString();
                trialInfo.Write(totalNumTargets + " ");

                // target objects:
                String targetObjRegex = @"ObjStart.*\r\nObjName\s(.*)\r\nObjectFilePath\s(.*)\r\nIsTargetObject\s(\d)";
                MatchCollection targetMatches = Regex.Matches(fileStr, targetObjRegex, RegexOptions.Multiline);
                int targetCount = 0;
                //Iterate through the matches, appending each to the performance file.
                foreach (Match SingleMatch in targetMatches)
                {
                    GroupCollection gc = SingleMatch.Groups;
                    String isTargetObject = gc[3].Captures[0].Value.Trim();
                    if (isTargetObject.Equals("1"))
                    {
                        String targetObj = gc[1].Captures[0].Value.Trim();
                        targetObj = Regex.Replace(targetObj, "Obj", "");

                        if(targetCount > 0)
                            trialInfo.Write("," + targetObj);
                        else
                            trialInfo.Write(targetObj);
                        targetCount++;
                    }
                }

                // Subject's selection of targets:
                String subjTargetRegex = @"SubjectTargetStart((.*)\r\n)*SubjectTargetEnd";
                MatchCollection subjSelectionMatches = Regex.Matches(fileStr, subjTargetRegex, RegexOptions.Multiline);
                int selectionCount = 0;
                foreach (Match SingleMatch in subjSelectionMatches)
                {
                    //Iterate through the matches, appending each to the performance file.
                    GroupCollection gc = SingleMatch.Groups;
                    CaptureCollection cc = gc[1].Captures;
                    for (int k = 1; k < cc.Count; k++)
                    {
                        String subjSelection = cc[k].Value.Trim();
                        subjSelection = Regex.Replace(subjSelection, "Obj", "");

                        if(selectionCount > 0)
                            trialInfo.Write("," + subjSelection);
                        else
                            trialInfo.Write(" " + subjSelection);
                        selectionCount++;
                    }
                }
              
                //dismiss the file handle
                reader.Close();
                trialInfo.Close();
            }
            catch (Exception e)
            {
                throw new ATLogicException(" Error populating the Performance File. ");
            }
        }

        public static bool isNewPerfFileOpen()
        {
            return newPerfFileOpen;
        }

        public static void setPerfFileStatus(bool isOpen) 
        {
            newPerfFileOpen = isOpen;
        }


        /**
        * Walks through each subfolder and file under the given directory recursively.
        * Creates a Performance (.perf) File, if not exists, for each session in
        * the given set by parsing the .gelog files on its way. 
        */
        public static void iterateOverTrials(DirectoryInfo d, String pf)
        {
            //create an array of files using FileInfo object
            FileInfo[] files;
            //get all files for the current directory
            files = d.GetFiles("*.*");
            // .perf file to be written on
            String perfFile = pf;

            //iterate through the directory and print the files
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".session")
                {
                    String perfFilePath = getPerformanceFileName(file.Name, file.DirectoryName);

                    if (File.Exists(perfFilePath))
                    {
                        return;
                    }
                    else
                    {
                        perfFile = create(perfFilePath, file.Name);
                    }
                }

                if (file.Extension == ".gelog" && perfFile != null)
                {
                    parseData(file.FullName, perfFile);
                }
            }

            //get sub-folders for the current directory
            DirectoryInfo[] dirs = d.GetDirectories("*.*");

            //This is the code that calls the getDirsFiles
            //This is the stopping point for this recursion function.
            //It loops through until reaches the child folder.
            foreach (DirectoryInfo dir in dirs)
            {
                iterateOverTrials(dir, perfFile);
            }
        }

    }
}
