
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalysisTool
{

    // createGazeFiles.cs - v2 - given an .EDF file, create individual trial Gaze files in separate trial dirs

    // requirements:
    //  o input must be an absolute path (i.e., C:\EVS\experiments\MyExperiment\Sess1\MyExperiment_Sess1.EDF)
    //  o EDF2ASC must  be  present under C:\\EVS\bin 

    // what program does:
    //  o sets up class vars that should "never" be changed
    //  o sets up some general vars (in main)
    //  o checks input (convince ourselves we've got a valid .edf)
    //  o redirect stdout to logfile (we want to see what happenned, but don't want console output)
    //  o get file/dir/path info
    //  o run edf2asc
    //  o parse the asc file
    //    -trial blocks begin with TRIALID line
    //    -creates trial dir if doesn't exist
    //    -checks if .gaze already exists (boolean "overwrite" determines behavior)
    //    -writes .gaze file

    // return codes:
    //  o 0=ok or  .asc file already exists
    //  o 1=error: there is not a single argument to the program (either there are no args or there are 2+ args)
    //  o 2=error: file not found
    //  o 3=error: file does not have .EDF/.edf suffix
    
    //  o 5=error: no .asc file after edf2asc runs
    //  o 6=error: couldn't create trial dir
    //  o 7=error: stdout redirect failed
    //  o 99=error: unknown error occurred

    // history
    //  o ekhan 4 Feb 07 (v1 is createTrialFiles.cs)
    //  o ekhan 4 Apr 07 (v2)
    //  o Smitha Madhavamurthy (v3) Refactored code to integrate with rest of Analysis Tool)
    //  o ekhan 20 May 07 - added fix for EDF2ASC.EXE (see convertEdfToAsc())

    public class CreateGazeFiles
    {
        private const bool debug = false;                        // outputs info about parsing-progress
        private const bool overwrite = true;                    // if .gaze already exists,
                                    //we  overwrite as it wouyld bechecked only after a new Gaze file is created.

        private const string EDF2ASCexe = "C:\\EVS\\bin\\EDF2ASC.EXE";
        // private const string EDF2ASCswitch = "";
        private const string EDF2ASCswitch = "";
        private const string gazeSuffix = ".gaze";
        private const string edfsufpattern = "\\.(EDF|edf)$";
        private const string ascsufpattern = "\\.(ASC|asc)$";
        private const string ascSuffix = ".ASC";

        private const string trialPrefix = "Tr";                                  // where .gaze files go
        private const string dirSeparator = "\\";
        private const string nameSeparator = "_";

        private const string strTrialID = "TRIALID";                               // for parsing
        private const string redirSuffix = ".stdout";                              // for logfile
        Regex regexTrialID = null;
        Regex regexForName = null;


        // Constructor
        public CreateGazeFiles(Regex regexID, Regex regexName)
        {
            if (regexID == null)
            {
                // for parsing 
                regexTrialID = new Regex("^MSG\\s[\\d]+\\sTRIALID");   // assuming that TRIALID holds the trial number 
            }
            else
            {
                this.regexTrialID = regexID;
            }
            if (regexName == null)
            {
                regexForName = new Regex("TRIALID.*$");                // assuming we use the entire TRIALID to name file       
            }
            else
            {
                regexForName = regexName;

            }
        }

        // Generates Gaze files from a Given EDF File 
       public int generateGazeFiles(String edfFileName) {
           
            string edfFileFullPath;                                 // full path to edf, including filename
            string sessionDirFullPath;                                  // the "working" directory
            string ascFileFullPath;

            // summary info
            int numTrials = 0;          
            int numFilesWritten = 0;    // may be different from numTrials if there are dup TRIALID's in the edf file

            // redirect stdout to file
            StreamWriter swStdout = null;
            string redirFullPath;
            try
            {
           
                //validateInput
                validateInput(edfFileName);
           
                // now we're convinced it's an edf file - copy filename
                edfFileFullPath = edfFileName;          


           
                // redirect stdout to file //
                redirFullPath = Regex.Replace(edfFileFullPath, edfsufpattern, redirSuffix);
           
                swStdout = new StreamWriter(redirFullPath);
                if (debug) Console.SetOut(swStdout);
                if (debug) Console.WriteLine("===== logfile =====");
                if (debug) Console.WriteLine(redirFullPath);
                if (debug) Console.WriteLine();
            }
            catch (IOException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
                errorWriter.Flush();
                errorWriter.Close();
                errorWriter = null;
                return 7;   // return optional - can continue using stdout as normal
            }
            finally
            {
                if (swStdout != null)
                {
                    swStdout.Close();
                    swStdout = null;
                }
            }
            

            // get filename info 
            if (debug) Console.WriteLine();
            if (debug) Console.WriteLine("===== edf file info =====");
            if (debug) Console.WriteLine("edf path=" + edfFileFullPath);
            FileInfo fInfo = new FileInfo(edfFileFullPath);
            
           sessionDirFullPath = String.Copy(fInfo.DirectoryName);
            if (debug) Console.WriteLine("edf dir=" + sessionDirFullPath);
            edfFileName = fInfo.Name;
            if (debug) Console.WriteLine("edf filename=" + edfFileName);


            // prepare for conversion 
            ascFileFullPath = Regex.Replace(edfFileFullPath, edfsufpattern, ascSuffix);   
                        // file will be named automatically by EDF2ASC

            convertEdfToAsc(ascFileFullPath, sessionDirFullPath, edfFileFullPath, swStdout);
           
           // check that .ASC was created 
            if (!System.IO.File.Exists(ascFileFullPath))
            {
                if (debug) Console.WriteLine("error 5: no .asc after edf2asc");
                return 5;
            }
           
           // parse the .asc file 
            return _creategazeFiles(ascFileFullPath, sessionDirFullPath , swStdout);


            // success 
            return 0;
        }


        /**
         * Helper  function : Validates Input parameters.
         */

        public int validateInput(String edfFileName)
        {
            if (edfFileName == null)
            {                                                 // check for one arg
                if (debug) System.Console.WriteLine("error 1: Invalid EDF File Name");
                return 1;
            }
            if (!System.IO.File.Exists(edfFileName))
            {                                  // check that file exists
                if (debug) System.Console.WriteLine("error 2: EDF file : " + edfFileName + " not found");
                return 2;
            }
            if (!Regex.IsMatch(edfFileName, edfsufpattern))
            {                           // check suffix
                if (debug) System.Console.WriteLine("error 3: Not a Valid EDF  Ascii File : " + edfFileName);
                return 3;
            }
            return 99; //default : unknown error 
        }

        //Convertes from EDF to ASC file  using EDF2ASC.exe found under C:\\EVS\bin directory
        public int convertEdfToAsc(String ascFileFullPath, String sessionDirFullPath, String edfFileName , StreamWriter swStdout)
        {
            int returnVal = 99; // defaults  ot unknown error

            if (!System.IO.File.Exists(ascFileFullPath))
            {
                /////////////////
                // run edf2asc //
                /////////////////
                if (debug) Console.WriteLine();
                if (debug) Console.WriteLine("===== calling edf2asc =====");
                try
                {
                    Process convertEDF = new Process();
                    convertEDF.StartInfo.FileName = EDF2ASCexe;
                    convertEDF.StartInfo.WorkingDirectory = sessionDirFullPath;
                    convertEDF.StartInfo.RedirectStandardOutput = true;    // want it to go to streamwriter/stdout
                    convertEDF.StartInfo.UseShellExecute = false;

                    // 20070520 - last-minute kluge
                    //   o EDF2ASC.EXE only accepts the 8.3 filename format
                    //
                    //      C:\EVS\EXPERI~1\homer\Sess1>C:\EVS\bin\EDF2ASC.EXE HOMER_SESS1.EDF
                    //      EDF2ASC: EyeLink EDF file -> ASCII (text) file translator
                    //      (c) 1996 by SR Research, last modified Jan 26 1997
                    //      No files found matching "HOMER_SE.EDF"

                    //   o we're going to copy the .EDF file (regardless of filename length) to a temporary name before conversion
                    //     and delete that copy afterwards (original .EDF will still exist)

                    // use arbitrary name
                    string tmpFileName = "tmp4conv.EDF";
                    string tmpPathName = sessionDirFullPath + "\\" + tmpFileName;
                    
                    // copy file (inconsistent variable naming - "edfFileName" var includes full path)
                    File.Copy(edfFileName, tmpPathName);

                    //string argString = EDF2ASCswitch + " " + edfFileName; // before edit
                    string argString = EDF2ASCswitch + " " + tmpFileName;   // EDF2ASC.EXE also doesn't like full pathname

                    if (debug) Console.WriteLine("args=" + argString);
                    convertEDF.StartInfo.Arguments = argString;

                    convertEDF.Start();
                    String convertEdfString = convertEDF.StandardOutput.ReadToEnd();
                    convertEDF.WaitForExit();
                    returnVal = 0;

                    // now rename the file - EDF2ASC.EXE automatically changes the suffix to .ASC
                    string tmpPathAfterConversion = sessionDirFullPath + "\\TMP4CONV.ASC";  // output name written as caps
                    string edfFileAfterConversion = Path.ChangeExtension(edfFileName, ascSuffix);
                    File.Move(tmpPathAfterConversion, edfFileAfterConversion);

                    // remove tmp4conv.EDF
                    File.Delete(tmpPathName); // delete tmp4conv.EDF

                    // done with kluge!

                }
                catch (Exception e)
                {
                    if (debug) Console.WriteLine("Error Message:");
                    if (debug) Console.WriteLine(e.Message);
                    if (debug) Console.WriteLine("Stack Trace:");
                    if (debug) Console.WriteLine(e.StackTrace);

                }
                returnVal = 0;
                // not checking return code of EDF2ASC.EXE because only saw 255 as return code 
                //  (for both successful runs and for failures)

            }
            else
            {
                returnVal = 0;
            }

            return returnVal;
        }


        /**
         * Actual Routine to convert .ASC file to .gaze file for every Trial in the Session
         */
        public int _creategazeFiles(String ascFileFullPath, String sessionDirFullPath, StreamWriter swStdout)
        {
            int returnVal = 99; // default  return value is unknown error
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader. (from api)
                using (StreamReader sr = new StreamReader(ascFileFullPath))
                {
                    String line;
                    string tmpname;
                    string gazefilename;
                    string trialnum;   // trial number
                    string trialname;
                    string trialdirfullpath;
                    string gazefilefullpath;
                    int numTrials = 0;
                    int numFilesWritten = 0;


                    // get first TRIALID line 
                    do
                    {
                        line = sr.ReadLine();
                        //Console.WriteLine(line); // for debug
                    } while (line != null && !regexTrialID.IsMatch(line));

                    // at this point, "line" holds TRIALID line


                    // loop until EOF
                    do
                    {
                        if (debug) Console.WriteLine("found:  " + line);
                        numTrials++;    // increment number of trials (there is a TRIALID line when this loop starts)
                        if (debug) Console.WriteLine("  num trial so far:  " + numTrials);


                        // get trial number from line
                        tmpname = regexForName.Match(line).ToString();    // tmpname = "TRIALID 0 3 7 0 0EVStime=123456"
                        String[] tmpnameStr = Regex.Split(tmpname, "EVStime=");
                        if (tmpnameStr.Length > 0)
                        {
                            tmpname = tmpnameStr[0];// tmpname = TRIALID 0 3 7 0 
                        }

                        tmpname = Regex.Replace(tmpname, "\\s+", "");     // tmpname = "TRIALID03700" (remove whitespace)
                        trialnum = Regex.Replace(tmpname, strTrialID, "");     // trialnum = "03700"
                        if (debug) Console.WriteLine("  trial num:  " + trialnum);


                        // set trial name
                        trialname = trialPrefix + trialnum;                   // trialname = "Tr03700"
                        if (debug) Console.WriteLine("  trialname:  " + trialname);


                        // set gaze file name
                        Regex regexsplitslash = new Regex("(\\\\)");        // requires parentheses
                        string[] splitDir = regexsplitslash.Split(sessionDirFullPath);
                        string expname = splitDir[6];                       // gets expname
                        string sessname = splitDir[8];                      // gets sessname
                        gazefilename = expname + nameSeparator + sessname + nameSeparator + trialname + gazeSuffix;
                        if (debug) Console.WriteLine("  gazefile:  " + gazefilename);


                        // set fullpath trialdir name
                        trialdirfullpath = sessionDirFullPath + dirSeparator + trialname;
                        if (debug) Console.WriteLine("  trialdir fullpath:  " + trialdirfullpath);


                        // set fullpath gaze file
                        gazefilefullpath = trialdirfullpath + dirSeparator + gazefilename;
                        if (debug) Console.WriteLine("  gazefile fullpath:  " + gazefilefullpath);


                        // check if trialdir exists (if not, create it)
                        if (debug) Console.Write("  checking if trialdir exists ...");
                        if (Directory.Exists(trialdirfullpath))
                        {
                            if (debug) Console.WriteLine(" yes");
                        }
                        else
                        {
                            if (debug) Console.WriteLine(" no");
                            try
                            {
                                if (debug)   // create trial dir
                                    Console.Write("  creating directory ...");
                                Directory.CreateDirectory(trialdirfullpath);
                                if (debug) Console.WriteLine(" done");
                            }
                            catch (Exception e)
                            {
                                if (debug) Console.WriteLine("error 6: failed to create trial dir");
                                if (debug) Console.WriteLine(e.Message);
                                if (debug) Console.WriteLine(e.StackTrace);
                                return 6;
                            }
                        }


                        // check if .gaze already exists
                        if (debug) Console.Write("  checking if gazefile exists ...");
                        if (File.Exists(gazefilefullpath))
                        { // from api 
                            if (debug) Console.WriteLine(" yes");
                            if (debug) Console.Write("  checking if we overwrite ...");
                            if (overwrite)
                            {
                                if (debug) Console.WriteLine(" yes");
                                // fall thru
                            }
                            else
                            {
                                if (debug) Console.WriteLine(" no");
                                if (debug) Console.Write("  skipping trial ...");
                                do
                                {
                                    line = sr.ReadLine();
                                } while (line != null && !regexTrialID.IsMatch(line));
                                if (debug) Console.WriteLine(" done");
                                continue;   // return to beginning of loop
                            }
                        }
                        else
                        {
                            // .gaze does not exist
                            if (debug) Console.WriteLine(" no");

                        }

                            // now write trial info to file
                            using (StreamWriter sw = new StreamWriter(gazefilefullpath))    // overwrites existing file
                            {
                                do
                                {
                                    // write line to file (first line is TRIALID line matched earlier)
                                    sw.WriteLine(line);
                                    // get next line
                                    line = sr.ReadLine();
                                } while (line != null && !regexTrialID.IsMatch(line));
                                // o check for null before checking for regex; 
                                //   otherwise exception
                                // o loop stops at next TRIALID line or at EOF
                                //   (if this is last trial, there may be extra msgs
                                //    after "TRIAL OK" marker and before EOF; 
                                //    the extra lines get written to the last file)
                            }
                            // end writing to file

                            numFilesWritten++;
                            if (debug) Console.WriteLine("  wrote file " + gazefilefullpath);
                        

                    } while (line != null); // end loop that reads .asc file


                    if (debug) Console.WriteLine();
                    if (debug) Console.WriteLine("===== summary =====");
                    if (debug) Console.WriteLine("number of trials = " + numTrials);
                    if (debug) Console.WriteLine("number of files written = " + numFilesWritten);
                    returnVal = 0;

                    try
                    {
                        // if no call to Flush, then some info doesn't make it to .stdout
                        swStdout.Flush();
                    }
                    catch (Exception e)
                    {
                        // if it fails at this point, ignore
                    }
                }
                returnVal = 0;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                if (debug) Console.WriteLine("error 99: unknown error");
                if (debug) Console.WriteLine(e.Message);
                if (debug) Console.WriteLine(e.StackTrace);
                return 99;
            }
            
            return returnVal;

        }

    }
}
