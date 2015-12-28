using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Data;
using System.Text.RegularExpressions; // for streamreader
using System.Diagnostics; // for Process
using System.Threading;
using AnalysisTool;

namespace AnalysisTool
{
    // Utility Class to Handle EDF to ASCII file Conversion 
    // and provides other common helper ro0utines needed by the application
    // Author : Smitha Madhavamurthy
    // Date : 04-25-2007
    class ATUtil
    {
        private static bool createXLSFile = false;


        /*
        * Given a Display FIle or a Gelog File name name, obtain and  return the name for the 
        */
        public static String retrieveXLSFileName(String inputFileName, String fileType)
        {
            String XLSFileName = null;
            String tmpGeLogFilename = null;
            String tmpDisplayFileName = null;

            if (inputFileName == null || fileType == null)
            {
                return null;
            }
            if (fileType.Equals("GELOG"))
                tmpGeLogFilename = inputFileName;
            else
                tmpDisplayFileName = inputFileName;
            if (tmpGeLogFilename != null)
            {
                try
                {
                    XLSFileName = Regex.Replace(tmpGeLogFilename, ".gelog$", ".xls");
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    XLSFileName = Regex.Replace(tmpDisplayFileName, ".display$", ".xls");
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return XLSFileName;


        }


        /*
         * Given a Display FIle name, obtain and  return the name for the 
         */
        public static String retrieveXLSFileName(String displayFileName)
        {
            String XLSFileName = null;

            if (displayFileName == null)
            {
                return null;
            }
            String tmpDisplayFileName = displayFileName;
            try
            {
                XLSFileName = Regex.Replace(tmpDisplayFileName, ".display$", ".xls");
            }
            catch (Exception)
            {
                return null;
            }

            return XLSFileName;


        }

        /**
         * check if a  given  Display File in Excel format exists
         */
        public static bool checkIfXLSFileExists(String XLSFileName)
        {
            if (XLSFileName == null)
                return false;
            try
            {
                if (File.Exists(XLSFileName))
                {
                    createXLSFile = false;
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        //routien to set flag to check if XLS file needs to be created
        public static void setCreateXLSFile(bool option)
        {
            createXLSFile = option;
        }

        /**
         * Checks to see  if XLS file has to be created
         */
        public static bool isCreateXLSFile()
        {
            return createXLSFile;
        }

        // Checks if input Directory is a  Session or Experiment Directory.
        // Returns true if iterator  is else  false
        public static bool IsSessionDirectory(DirectoryInfo directoryName)
        {

            //create an array of files using FileInfo object
            FileInfo[] files;
            //get all files for the current directory
            files = directoryName.GetFiles("*.*");
           //iterate through the directory and print the files
            foreach (FileInfo file in files)
            {
                if ((file.Extension == ".session") || (file.Extension == ".Session"))
                {
                    return true;
                }
               
            }
            return true;
        }
    }
}

