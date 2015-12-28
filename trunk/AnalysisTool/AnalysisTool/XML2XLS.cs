// ConvertXMLxls.cs
// v1 - ekhan 20070517
// v2 - ekhan 20070527 - commented out creation of second EVSview (second worksheet) to cut file creation time in half
//                          (depending on number of objects and number of timestamps, creation can take as little as 10
//                          seconds or as long as several minutes)
//                        - to reenable, set "doView2" to true
//
//  o how this program works:
//    -input is an xml file in EVS "display" format
//    -program parses the xml as a stream
//    -output is a .xls file (two worksheets) in EVS "human-readable" format 
//     (Worksheet1) trial/condition/object info and timestamps
//     (Worksheet2) timestamps only (in different layout)
//    -output is written to C:\same_directory\same_filename.xls
//
//  o notes
//    -whitespace in the xml is ignored (via the "WhitespaceHandling" property).  
//     However, comments or other unexpected xml_nodes are not handled.
//     Therefore, there cannot be any comments or unexpected xml_nodes in the display file.
//    -Excel has the ability to turn off/on pop-up alerts.  For this program, the DisplayAlerts property is set to false.
//     (The property includes warning the user that a file will be overwritten => this program will overwrite previous
//      .xls files without warning)
//    -most of the code is written for Worksheet1 since it has more info & formatting.
//     vars for Worksheet2 have names of the form "varView2"
//
//  o In order to use this program in an existing solution:
//    (1) a Reference to the Object Library has to be added to the Solution.  
//        -Otherwise, the following compile error:
//         "The type or namespace name 'Excel' could not be found (are you missing a using directive or an assembly reference?)"
//        -To add the reference:  Project > Add Reference > COM > Microsoft Excel 11.0 Object Library
//    (2) the Primary Interop Assemblies (PIA) for Excel have to be on to the development machine
//        -Otherwise, the following compile error:
//         "The type or namespace name 'Interop' does not exist in the namespace 'Microsoft.Office' (are you missing an assembly reference?)"
//        -To check if the PIA is already on the development machine:
//         Look for C:\Windows\assembly\Microsoft.Office.Interop.Excel.dll
//         If the file is not there, then the PIA for Microsoft Office must be installed.
//         (The PIAs for Office are bundled; you must download/install them all.)
//         As a side note, if the PIA is not there, then a full install of Office is not on the system.
//         (Probably the install was a "Typical" install.)
//        -To install the PIAs:
//         --download "Office 2003 Update: Redistributable Primary Interop Assemblies" from
//           www.microsoft.com/downloads/details.aspx?familyid=3c9a983a-ac14-4125-8ba0-d36d67e0f4ad&displaylang=en
//         --run the installer (O2003PIA.MSI)
//
//  o Refs
//    - an intro to programmatically creating excel files
//      www.codeproject.com/office/csharp_excel.asp?df=100&forumid=23997&select=1792818#xx1792818xx
//    - full program for programmatically creating excel file
//      cheeso.members.winisp.net/srcview.aspx?dir=MSOffice&file=AutomateExcel.cs
//    - Range properties
//      msdn2.microsoft.com/en-us/library/microsoft.office.interop.excel.range_properties(vs.80).aspx
//    - cell insertion
//      www.dotnet247.com/247reference/msgs/58/294566.aspx
//    - PIA info
//      msdn2.microsoft.com/en-us/library/aa159923(office.11).aspx


using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;    // for "missing"


namespace AnalysisTool
{
    public class XML2XLS
    {
        // excel vars
        private  object missing = System.Reflection.Missing.Value;    // needed for some Excel methods
        private  int firstColWidth = 20;
        private  string stringA = "A";        // for building cell_name
        private string stringB = "B";

        private  Excel.Range excelCell;       // the working/current cell for view1
        private  Excel.Range excelCellView2;  
        private  Excel.Range timestampCell;   // need to record cell location for line insertion
        private  int timeStampLineNum;        // need to record line number for line insertion
        private  string outputFile;
        private  int rowNumber;               // used to keep track of row numbers starting with "Condition" section (for view1)
        private  int rowNumberView2;          // row numbers for view2
        private  int numObjects = 0;          // calculated in a loop
        private String displayFileName = null;
        private static bool doView2 = false;


       
        public XML2XLS(String displayFile)
        {
            displayFileName =  displayFile;
        }



        public void convert()
        {
            ////////////////////////////////////////////////////////////////////
            // get input file                                                 //
            ////////////////////////////////////////////////////////////////////
            string inputFile;
            string suffPattern = "\\.(DISPLAY|display|XML|xml)$";   // only works with these types of files


            if (displayFileName == null)
            {
               // Console.Error.WriteLine("Error: requires Display FileName");
                return ;
            }
            if (!System.IO.File.Exists(displayFileName))
            {                                  // check that file exists
                Console.Error.WriteLine("Error: file not found");
                return;
            }
            if (!Regex.IsMatch(displayFileName, suffPattern))
            {
                Console.Error.WriteLine("Error: no suffix match");
                return;
            }

            inputFile = String.Copy(displayFileName);
            outputFile = Regex.Replace(inputFile, suffPattern, ".xls");

            ////////////////////////////////////////////////////////////////////
            // set up xml                                                     //
            ////////////////////////////////////////////////////////////////////
            System.Xml.XmlTextReader xmlreader;
            // o reads a stream (can't go backwards in the stream)
            // o doesn't load the whole document/tree, so is relatively fast
            // o does not validate the xml

            xmlreader = new XmlTextReader(inputFile);

            xmlreader.WhitespaceHandling = WhitespaceHandling.None;         // do not consider whitespace as xml nodes

            ////////////////////////////////////////////////////////////////////
            // set up excel                                                   //
            ////////////////////////////////////////////////////////////////////

            // open the Excel application
            Excel.Application excelApp = new Excel.ApplicationClass();

            // turn off alerts
            excelApp.DisplayAlerts = false;
            // msdn2.microsoft.com/en-us/library/microsoft.office.interop.excel._application.displayalerts(VS.80).aspx

            // set number of sheets
            if (doView2) excelApp.SheetsInNewWorkbook = 2;
            else excelApp.SheetsInNewWorkbook = 1;

            // open a new blank workbook 
            Excel.Workbook excelWorkbook = excelApp.Workbooks.Add(missing);

            // create a Sheets object that holds the Worksheets within the workbook
            Excel.Sheets excelSheets = excelWorkbook.Worksheets;

            // get an individual sheet
            Excel.Worksheet view1Worksheet = (Excel.Worksheet)excelSheets.get_Item("Sheet1");
            view1Worksheet.Name = "EVSview1";
            // create view2Worksheet even though we may not use it            
            Excel.Worksheet view2Worksheet = null;
            if (doView2)
            {
                view2Worksheet = (Excel.Worksheet)excelSheets.get_Item("Sheet2");
                view2Worksheet.Name = "EVSview2";
            }

            ////////////////////////////////////////////////////////////////////
            // write "Trial Name" section                                     //
            ////////////////////////////////////////////////////////////////////

            // we can use hard-coded cell locations for this section because we know the format
            //   (i.e., there are only two columns and three rows of output)

            // "Trial" node
            excelCell = (Excel.Range)view1Worksheet.get_Range("A1", "A1");  // access individual cell
            excelCell.Value2 = "Trial Name";
            excelCell.Font.Bold = true;

            excelCell = (Excel.Range)view1Worksheet.get_Range("B1", "B1");
            xmlreader.Read();       // get first node
            while (xmlreader.Name != "Trial") xmlreader.Read();    // scroll thru until "Trial" node
            if (xmlreader.Name != "Trial")  // we should be at "Trial" node now
            {
                Console.Error.WriteLine("xmlParseError: Looking for Trial node.  Found" + xmlreader.Name);
                return;
            }
            excelCell.Value2 = xmlreader.GetAttribute("name");      // write trial_name to B1

            // "Display" node
            xmlreader.Read();       // move to "Display" node
            if (xmlreader.Name != "Display")
            {
                Console.Error.WriteLine("xmlParseError: Looking for Display node.  Found" + xmlreader.Name);
                return;
            }
            excelCell = (Excel.Range)view1Worksheet.get_Range("A2", "A2");
            excelCell.Value2 = "DisplayX";
            excelCell = (Excel.Range)view1Worksheet.get_Range("B2", "B2");
            excelCell.Value2 = xmlreader.GetAttribute("X");
            excelCell = (Excel.Range)view1Worksheet.get_Range("A3", "A3");
            excelCell.Value2 = "DisplayY";
            excelCell = (Excel.Range)view1Worksheet.get_Range("B3", "B3");
            excelCell.Value2 = xmlreader.GetAttribute("Y");


            ////////////////////////////////////////////////////////////////////
            // write "Condition" section                                      //
            ////////////////////////////////////////////////////////////////////

            excelCell = (Excel.Range)view1Worksheet.get_Range("A5", "A5");
            excelCell.Value2 = "Condition Info";
            excelCell.Font.Bold = true;

            rowNumber = 5;   // from this point, rows and columns are not hard-coded

            // the next node is Condition
            xmlreader.Read();
            if (xmlreader.Name != "Condition")
            {
                Console.Error.WriteLine("xmlParseError: Looking for Condition node.  Found" + xmlreader.Name);
                return;
            }

            // we are now at "Condition" node
            //  o we're going to loop through attributes (instead of accessing them via hard-coded names)
            //  o for future, this means that attributes can be added without changing this section of code
            int numAttr = xmlreader.AttributeCount;     // for v1, the count is 12

            xmlreader.MoveToFirstAttribute();   // go to first attribute
            for (int i = 0; i < numAttr; i++)
            {
                rowNumber++;                     // increment row number
                excelCell = getCell(view1Worksheet, stringA, rowNumber);
                excelCell.Value2 = xmlreader.Name;      // write attribute name
                excelCell = excelCell.Next;             // move to the right
                excelCell.Value2 = xmlreader.Value;     // write attribute value
                xmlreader.MoveToNextAttribute();
            }

            // at this point, we're still in the condition node, and are going to read the object nodes

            // skip two lines for spacing
            rowNumber += 2;

            ////////////////////////////////////////////////////////////////////
            // write "Object Info" section                                    //
            ////////////////////////////////////////////////////////////////////
            excelCell = getCell(view1Worksheet, stringA, rowNumber);
            excelCell.Value2 = "Object Info";
            excelCell.Font.Bold = true;

            xmlreader.Read();               // move to first object node
            if (xmlreader.Name != "Obj")
            {
                Console.Error.WriteLine("xmlParseError: Looking for Obj node (sub-node of Condition).  Found" + xmlreader.Name);
                return;
            }

            // iterate over the Object nodes (this is also where we calculate the numObjects value)
            while (xmlreader.Name == "Obj")
            {
                numObjects++;
                rowNumber++;
                excelCell = getCell(view1Worksheet, stringA, rowNumber);
                excelCell.Value2 = xmlreader.GetAttribute("name");
                excelCell = excelCell.Next;     // move to the right
                if (xmlreader.GetAttribute("IsTargetObject") == "1")  //0=no, 1=yes
                {
                    excelCell.Value2 = "TARGET";
                }
                else
                {
                    excelCell.Value2 = "not target";
                }
                excelCell = excelCell.Next;
                excelCell.Value2 = xmlreader.GetAttribute("ObjectFilePath");
                xmlreader.Read();
            }

            // at this point, we are at the </Condition> node

            // skip two more lines for spacing
            rowNumber += 2;

            ////////////////////////////////////////////////////////////////////
            // write the timestamp header rows in view1                       //
            ////////////////////////////////////////////////////////////////////

            // calculate how many columns we will need for the TimeStamp header row:
            //   [first column]  +  [object/gaze labels]  + [X and Y label for each object/gaze] + [n spacers]
            //     TimeStamp row + (n objects + 1 gaze)(X + Y columns) + (n blank columns)
            //      =>         1 + (n         + 1     )(2)             + n
            //      =>         1 + (n+1)(2) + n
            //      =>         1 + (2n + 2) + n
            //      =>         1 + 2n + 2 + n
            //      =>         3 + 3n 
            //      =>         (n+1)3 columns
            //
            // format of columns:
            //   timestamp gazeX gazeY blank obj1X obj1Y blank ... objnX objnY

            int numColumns = (numObjects + 1) * 3;  // 3(n+1)

            // vars for formatting the first row
            Excel.Range mergeRange;
            Excel.Range mergeFrom;
            Excel.Range mergeTo;
            Excel.Range highlightRange;
            Excel.Range highlightFrom;
            Excel.Range highlightTo;

            // write the timestamp-label cell
            excelCell = getCell(view1Worksheet, stringA, rowNumber);
            timeStampLineNum = rowNumber;       // need this number for insertion later
            excelCell.Value2 = "TimeStamp";
            excelCell.Font.Bold = true;
            timestampCell = excelCell;  // save location of TimeStamp for insertion later 
            highlightFrom = excelCell; // save location of TimeStamp for highlighting now

            // write and merge the gaze-label cells
            excelCell = excelCell.Next;
            excelCell.Value2 = "gaze";
            mergeFrom = excelCell;      // the "X" col
            excelCell = excelCell.Next; // the "Y" col
            mergeTo = excelCell;
            mergeRange = (Excel.Range)view1Worksheet.get_Range(mergeFrom, mergeTo);
            mergeRange.MergeCells = true;
            mergeRange.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

            excelCell = excelCell.Next.Next; // first Next for spacer; second for moving to initial obj-label cell

            // write and merge the object-label cells
            for (int n = 1; n < numObjects + 1; n++)
            {
                excelCell.Value2 = "obj" + n;
                mergeFrom = excelCell;      // the "X" col
                excelCell = excelCell.Next; // the "Y" col
                mergeTo = excelCell;
                excelCell = excelCell.Next.Next; // spacer

                mergeRange = (Excel.Range)view1Worksheet.get_Range(mergeFrom, mergeTo);
                mergeRange.MergeCells = true;
                mergeRange.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;
            }

            // kluge!  we're now two cells too far for highlighting
            excelCell = excelCell.Previous.Previous;

            highlightTo = excelCell;
            highlightRange = (Excel.Range)view1Worksheet.get_Range(highlightFrom, highlightTo);
            highlightRange.Interior.Color = (222 << 16) | (222 << 8) | 222;        // 222 = shade of grey

            //next line (labelled with X/Y)
            rowNumber++;

            excelCell = getCell(view1Worksheet, stringB, rowNumber);    // start in column B
            for (int j = 0; j < numObjects + 1; j++)
            {
                excelCell.Value2 = "X";
                excelCell = excelCell.Next;
                excelCell.Value2 = "Y";
                excelCell = excelCell.Next.Next; // spacer
            }

            // now we center the X/Y labels
            excelCell.EntireRow.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

            ////////////////////////////////////////////////////////////////////
            // write the timestamp header rows in view2                       //
            ////////////////////////////////////////////////////////////////////

            if (doView2)
            {
                // headers cells on this worksheet can be hard-coded because they have standard locations
                excelCellView2 = getCell(view2Worksheet, "A", 1);
                excelCellView2.Value2 = "TimeStamp";
                excelCellView2.Font.Bold = true;
                excelCellView2.Font.Underline = true;
                excelCellView2.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

                excelCellView2 = getCell(view2Worksheet, "B", 1);
                excelCellView2.Value2 = "Object";
                excelCellView2.Font.Bold = true;
                excelCellView2.Font.Underline = true;
                excelCellView2.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

                excelCellView2 = getCell(view2Worksheet, "C", 1);
                excelCellView2.Value2 = "X";
                excelCellView2.Font.Bold = true;
                excelCellView2.Font.Underline = true;
                excelCellView2.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

                excelCellView2 = getCell(view2Worksheet, "D", 1);
                excelCellView2.Value2 = "Y";
                excelCellView2.Font.Bold = true;
                excelCellView2.Font.Underline = true;
                excelCellView2.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

                rowNumberView2 = 2;     // object locations start on next line
            }

            ////////////////////////////////////////////////////////////////////
            // write the timestamps (onto both sheets)                        //
            ////////////////////////////////////////////////////////////////////

            // recall that we are at the </Condition> node - we want to move to <TrialStart>
            xmlreader.Read();
            if (xmlreader.Name != "TrialStart")
            {
                Console.Error.WriteLine("xmlParseError: Looking for TrialStart.  Found" + xmlreader.Name);
                return;
            }

            while (xmlreader.Name != "SubjectTargetStart")  // go thru nodes until SubjectTargetStart is reached
            {
                rowNumber++;
                xmlreader.Read();   // move to Time node

                if (xmlreader.Name == "Time")
                {
                    // o loop over "Time" nodes
                    // o the "if Time" statement is present to skip over the </TrialStart> and <SubjectTargetStart> nodes.
                    //   If those nodes were not present, we wouldn't need the "if Time" statement 
                    //   (or we would error-check with an "if !Time" statement)

                    excelCell = getCell(view1Worksheet, stringA, rowNumber);
                    excelCell.Value2 = xmlreader.GetAttribute("stamp");         // write time to the cell

                    // for view2
                    if (doView2)
                    {
                        excelCellView2 = getCell(view2Worksheet, stringA, rowNumberView2);
                        excelCellView2.Value2 = xmlreader.GetAttribute("stamp");
                        excelCellView2 = excelCellView2.Next;
                    }

                    xmlreader.Read();   // move to first obj node 
                    // (this just allows us to put the Read() at the bottom of the while loop)
                    if (xmlreader.Name != "obj")
                    {
                        Console.Error.WriteLine("xmlParseError: Looking for obj (sub-node of Time).  Found" + xmlreader.Name);
                        return;
                    }

                    // loop over the obj nodes
                    while (xmlreader.NodeType != XmlNodeType.EndElement)
                    {
                        // o loop over the obj nodes
                        // o we can end the loop with EndElement because obj nodes do not have an ending element
                        //   but the Time node does (</Time>)

                        // View2 needs obj name for each iteration, but View1 doesn't
                        if (doView2) excelCellView2.Value2 = xmlreader.GetAttribute("name");

                        excelCell = excelCell.Next; // move to the right
                        if (doView2) excelCellView2 = excelCellView2.Next;
                        excelCell.Value2 = xmlreader.GetAttribute("x");
                        if (doView2) excelCellView2.Value2 = xmlreader.GetAttribute("x");

                        excelCell = excelCell.Next; // move to the right
                        if (doView2) excelCellView2 = excelCellView2.Next;
                        excelCell.Value2 = xmlreader.GetAttribute("y");
                        if (doView2) excelCellView2.Value2 = xmlreader.GetAttribute("y");

                        excelCell = excelCell.Next; // move to the right (the "spacer" column)

                        if (doView2) rowNumberView2++;   // view2 has objects on different rows
                        if (doView2) excelCellView2 = getCell(view2Worksheet, stringB, rowNumberView2);

                        xmlreader.Read();   // move to next node (which will be either <obj> or </Time>
                    }

                    // after the while loop, we are at a </Time> node
                }
                if (doView2) rowNumberView2 += 2;    // extra spacing on view2
            }

            // at this point, we are at <SubjectTargetStart>

            ////////////////////////////////////////////////////////////////////
            // insert user selection above timestamps                         //
            ////////////////////////////////////////////////////////////////////
            int linenumForWriting = timeStampLineNum;

            if (xmlreader.Name != "SubjectTargetStart")
            {
                Console.Error.WriteLine("xmlParseError: Looking for SubjectTargetStart.  Found" + xmlreader.Name);
                return;
            }

            // insert "User Selected" Label/row
            Excel.Range timestampRow = timestampCell.EntireRow;  // select TimeStamp-label row
            timestampRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown, missing);  // move the row down (insert line above)
            timeStampLineNum++;     // the TimeStamp line has moved down one row

            Excel.Range currWritingCell = getCell(view1Worksheet, stringA, linenumForWriting);
            currWritingCell.Value2 = "User Selected";
            currWritingCell.Font.Bold = true;
            linenumForWriting++;

            // move to first Obj node
            xmlreader.Read();
            if (xmlreader.Name != "Obj")
            {
                Console.Error.WriteLine("xmlParseError: Looking for Obj (sub-node of SubjectTargetStart).  Found" + xmlreader.Name);
                return;
            }

            while (xmlreader.NodeType != XmlNodeType.EndElement)
            {
                timestampCell = getCell(view1Worksheet, stringA, timeStampLineNum);     // where TimeStamp-label is currently
                timestampRow = timestampCell.EntireRow;

                timestampRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown, missing);
                timeStampLineNum++; // TimeStamp-label row has moved down

                // write attribute where TimeStamp-label used to be
                currWritingCell = getCell(view1Worksheet, stringA, linenumForWriting);
                currWritingCell.Value2 = xmlreader.GetAttribute("name");
                currWritingCell.Font.Bold = false;
                linenumForWriting++;

                xmlreader.Read();   // move to next node
            }

            // at this point, we are at </SubjectTargetStart>

            // insert final row for spacing
            timestampCell = getCell(view1Worksheet, stringA, timeStampLineNum);
            timestampRow = timestampCell.EntireRow;
            timestampRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown, missing);
            timeStampLineNum++;


            ////////////////////////////////////////////////////////////////////
            // resize column A on both sheets                                 //
            ////////////////////////////////////////////////////////////////////

            excelCell = (Excel.Range)view1Worksheet.get_Range("A1", missing);
            excelCell.ColumnWidth = firstColWidth;

            if (doView2) excelCellView2 = (Excel.Range)view2Worksheet.get_Range("A1", missing);
            if (doView2) excelCellView2.ColumnWidth = firstColWidth;


            ////////////////////////////////////////////////////////////////////
            // save doc & close Excel                                         //
            ////////////////////////////////////////////////////////////////////                

            excelWorkbook.SaveAs(outputFile, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            // parameters description at msdn2.microsoft.com/en-us/library/microsoft.office.interop.excel._workbook.saveas(VS.80).aspx

            excelApp.Quit();
        }


        ///////////////////////////////////////////////////////////////////////////////////
        // helper function
        ///////////////////////////////////////////////////////////////////////////////////

        private  Excel.Range getCell(Excel.Worksheet sheet, string col, int row)
        {
            // o purpose: reduce code clutter - these lines are used frequently and make the code look more confusing than it is
            // o function: given cell coordinates, return an Excel.Range object for that single cell

            string cellLabel = col + row.ToString();
            return (Excel.Range)sheet.get_Range(cellLabel, cellLabel);
        }

       

       
    }
}
