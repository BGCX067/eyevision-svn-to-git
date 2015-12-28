using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace AnalysisTool
{
    /**
    * Parses Display File and provides utility routines to get specific data from Display File.
    * The data returned from this Parser is fed to a Windows Form component 
    * which handles the Graphical Display of the data
    * @author Smitha Madhavamurthy
    * @date 04-15-2007
    */
    class DisplayFileParser
    {
        // private fields
        private String displayFileName;
        
        private XmlReader reader ;

        private string displayFileString;

        private Hashtable data;
        private String SchemaFilePath = "DisplayFileSchema.xsd";
        

        // constructor
        public DisplayFileParser(String displayFileName)
        {
            this.displayFileName = displayFileName;

           
        }

        // Validate if display file is valid
        public bool validate()
        {
            try
            {
                if (File.Exists(this.displayFileName))
                {
                    FileStream fileStream = new FileStream(this.displayFileName, FileMode.Open, FileAccess.Read);

                   XmlReaderSettings settings = new XmlReaderSettings();
                   settings.Schemas.Add(null, SchemaFilePath);
                    
                   settings.ValidationType = ValidationType.Schema; 
                   settings.IgnoreWhitespace = true;
                   reader = XmlReader.Create(fileStream, settings);
                  reader = new XmlTextReader(this.displayFileName);
                    data = new Hashtable();
                   return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
               throw;
            }
            // Any other  validation checks can be performed in this block
            
        }

        // helper  routine to read  attribute data from a  node
        public static void readAttributes(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                    // process all of the element's attributes
                    for (int i = 0; i < reader.AttributeCount; i++)
                        Console.WriteLine(reader.GetAttribute(i));
            }
        }

        

        //
         public static void InspectCurrentNode1(XmlReader reader)
        {
          String sQName = reader.Name;
          String sLocalName = reader.LocalName;
          String sNSURI = reader.NamespaceURI;
          XmlNodeType type = reader.NodeType;
          String sValue = reader.Value;
          bool bAtts = reader.HasAttributes;
          int nAtts = reader.AttributeCount;
          
        }
        public  bool isEOF()
        {
            return this.reader.EOF;
        }

        // routine to retrieve global display data from Display File
        public Hashtable retrieveGlobalDisplayData()
        {
            List<ObjectData> objList = new List<ObjectData>();
            Hashtable currentData  = new Hashtable();
           
            if(! reader.EOF){
                reader.ReadToFollowing("Display");
                if (reader.NodeType == XmlNodeType.Element){

                    if (reader.Name == "Display")
                    {
                        processDisplay(currentData);
                    }
                }
            }
            if (!reader.EOF)
            {
                reader.ReadToFollowing("Condition");
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if(reader.Name == "Condition")
                    {
                     processCondition(currentData);
                    }
                }
            }

            while (reader.Read() && !reader.EOF  )
            {
                reader.MoveToContent() ;
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Condition")
                {
                    break;
                }

                if (!(reader.Name == "Obj"))
                {
                    reader.ReadToFollowing("Obj");
                }
                ObjectData  data  = processObjectInfo(currentData);
                if (data != null)
                 {
                      objList.Add(data);
                }
                    
                
            }
            currentData.Add("Objlist", objList);
            return currentData;
           
}


        /**
         * Extracts Display Size in both X and Y Axis.
         * This Data would have been logged as nodes  DisplayX  and  DisplayY respectively
         */
        private void processDisplay(Hashtable currentData){
            if(reader.HasAttributes && reader.AttributeCount == 2){
                  // process  X and Y attributes of Display Screen 
                        
                int  x =  XmlConvert.ToInt32(reader.GetAttribute("X"));
                int y = XmlConvert.ToInt32(reader.GetAttribute("Y"));
                currentData.Add("X", x);
                currentData.Add("Y", y);
            }

        }

        // Retrieve data related to the Trial Condition.
        private void processCondition(Hashtable currentData){
          if (reader.HasAttributes && reader.AttributeCount == 12)
          {
              // process all attributes of Condition Node
                 int num = XmlConvert.ToInt32(reader.GetAttribute(0)); // attribute  index is zero based
                    int gridDisplay = XmlConvert.ToInt32(reader.GetAttribute(1));
                    int bga = XmlConvert.ToInt32(reader.GetAttribute(2));
                    int bgr = XmlConvert.ToInt32(reader.GetAttribute(3));
                    int bgg = XmlConvert.ToInt32(reader.GetAttribute(4));
                    int bgb = XmlConvert.ToInt32(reader.GetAttribute(5));
                    int fga = XmlConvert.ToInt32(reader.GetAttribute(6));
                    int fgr = XmlConvert.ToInt32(reader.GetAttribute(7));
                    int  fgg = XmlConvert.ToInt32(reader.GetAttribute(8));
                    int fgb = XmlConvert.ToInt32(reader.GetAttribute(9));
                    int numObjects = XmlConvert.ToInt32(reader.GetAttribute(10));
                    int numTargets  = XmlConvert.ToInt32(reader.GetAttribute(11));
                   
                    currentData.Add("Num", num);
                    currentData.Add("GridDisplay", gridDisplay);
                    currentData.Add("BGA", bga);
                    currentData.Add("BGR", bgr);
                    currentData.Add("BGG", bgg);
                    currentData.Add("BGB", bgb);
                    currentData.Add("FGA", fga);
                    currentData.Add("FGR", fgr);
                    currentData.Add("FGG", fgg);
                    currentData.Add("FGB", fgb);
                    currentData.Add("NumObjects", numObjects);
                    currentData.Add("NumTargets", numTargets);
                }
         
        }
          
        // Retrieve  data about every Object used in the Trial.
        private ObjectData processObjectInfo(Hashtable currentData){
            ObjectData obj = null;
            if (reader.HasAttributes && reader.AttributeCount == 3)
            {
                   string name =reader.GetAttribute("name").ToString(); // attribute  index is zero based
                   string objectFilePath = reader.GetAttribute("ObjectFilePath").ToString();
                   int isTargetObject = XmlConvert.ToInt32(reader.GetAttribute("IsTargetObject"));
                  obj = new ObjectData();
                    obj.ObjName = name;
                    obj.ObjFilePath =  objectFilePath;
                    obj.IsTargetObject = isTargetObject;
                   
                            
             }
             return obj;
         }

       
        // Get  Display screen size data
        public Hashtable getDisplayData()
        {
            data.Clear(); // reuse the  object
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                    // process all of the element's attributes
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                       // Console.WriteLine(reader.GetAttribute(i));
                        data.Add(reader.Name, reader.GetAttribute(i));
                        reader.MoveToAttribute(0);
                        
                    }
            }
            return data;
           
               
        }

        // return Grid Display data
        public int getGridDisplay()
        {
            string gridDisplayRegex = @"GridDisplay\s.*(\d)+";
            String gridDisplay = Regex.Match(displayFileString, gridDisplayRegex).ToString();
            if (gridDisplay == null)
            {
                Console.WriteLine("Error Retrieving Grid Display Data");
            }
            return Convert.ToInt32(gridDisplay);
        }

        // retrun foreground color information
        public Hashtable getBgColor()
        {
            data.Clear(); // reuse the  object
           
          //  data.Add("DisplayX", display[0]);
          //  data.Add("DisplayY", display[1]);
            return data;
        }

        // retrun back ground color information
        public Hashtable getFgColor()
        {
            return data;
        }

        // return total number of objects in the trial
        public int getNumObjects()
        {
            int numObjects = 0;
            return numObjects;
        }

        // getobject info :  Object file name and Absolute file path where Object is stored
        public List<ObjectData> getObjectsInfo()
        {
            return null;

        }

        // return system target objects
        public List<string> getSubjectTargetObjects()
        {
            return null;
        }


        //return all objectsposition data for one time stamp
        public ObjPositionData getNextTimeStampData()
        {
           

            ObjPositionData objPosData = new ObjPositionData();

            if ( !reader.EOF)
            {
                reader.ReadToFollowing("Time");
                if(reader.HasAttributes &&  reader.AttributeCount > 0){
                    int timeStamp = Convert.ToInt32(reader.GetAttribute("stamp"));
                    objPosData.TimeStamp = timeStamp;

                }
                
                do
                {
                    reader.Read();
                    
                   // reader.ReadToFollowing("obj");
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "obj")
                        {
                            if (reader.HasAttributes && reader.AttributeCount >= 0)
                            {
                                string name = reader.GetAttribute("name").ToString();
                                float x = Convert.ToSingle(reader.GetAttribute("x"));
                                float y = Convert.ToSingle(reader.GetAttribute("y"));
                                ObjectData objdata = new ObjectData();
                                objdata.objName = name;
                                objdata.xPos = x;
                                objdata.YPos = y;
                                
                                objPosData.addObjData(objdata);
                            }
                        }
                        else if (reader.Name == "Time")
                        {
                            break;
                        }
                    }
                
                } while (!(reader.EOF) && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Time"));
               
            }
            return objPosData;
        }

        
        // Document  traversal routine.
        public void TraverseInDocumentOrder(XmlReader reader)
        {
            try
            {
                while (reader.Read())
                    Console.WriteLine("{0}", reader.NodeType);
            }
            catch (XmlException e)
            {
                Console.WriteLine("###error: " + e.Message);
            }
        } 


        //Check to find the Current  Node Type
        public static void InspectCurrentNode(XmlReader reader)
        {
          String sQName = reader.Name;
          String sLocalName = reader.LocalName;
          String sNSURI = reader.NamespaceURI;
          XmlNodeType type = reader.NodeType;
          String sValue = reader.Value;
          bool bAtts = reader.HasAttributes;
          int nAtts = reader.AttributeCount;
          
        }

        // Testing purposes  only
        public static void run()
        {
            if (File.Exists("C:\\EVS\\experiments\\Exp1\\Sess1\\Tr1\\displayParser.log"))
            {
                File.Delete("C:\\EVS\\experiments\\Exp1\\Sess1\\Tr1\\displayParser.log");
            }
            TextWriter log = null;
            try
            {
                log = new StreamWriter("C:\\EVS\\experiments\\Exp1\\Sess1\\Tr1\\displayParser.log");


                DisplayFileParser parser = new DisplayFileParser("C:\\EVS\\experiments\\Exp1\\Sess1\\Tr1\\Exp1_Sess1_Tr5_disp.xml");

                Console.WriteLine(" validating file");
                if (parser.validate())
                {
                    Hashtable data = parser.retrieveGlobalDisplayData();
                    if (data == null)
                    {
                        log.WriteLine(" display data null");
                        return;
                    }
                    System.Collections.IDictionaryEnumerator itr = data.GetEnumerator();
                    while (itr.MoveNext())
                    {
                        Object value = itr.Value;
                        if (value is List<ObjectData>)
                        {
                            List<ObjectData> list =(List<ObjectData>) value;
                            foreach (ObjectData thisData in list)
                                log.WriteLine(thisData.objName + " : " + thisData.objFilePath + " : " + thisData.isTargetObject);
                        }
                        else
                        {
                            log.WriteLine(itr.Key.ToString() + " : " + itr.Value.ToString());
                        }
                    }

                }
                else
                {
                    log.WriteLine("not valid!!");
                }

            }
            finally
            {

                log.Flush();
                log.Close();

            }
        }


        // Routine to close allServices  help by this Object
        public void close()
        {
            if(reader != null)
                reader.Close();
            this.data = null;
            this.displayFileName = null;
            this.displayFileString = null;

            reader = null;
        }


    }
}

