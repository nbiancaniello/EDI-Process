using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using Test.Inbound;
using Test.Inbound.Segments;

namespace Test
{
    class Program
    {
        public static string customerID;
        static void Main(string[] args)
        {
            // Get directory info
            string[] files = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings.Get("InboundFilesPath830"));
            string rawContent = null;
            char fieldDelimiter, segmentDelimiter;

            foreach (var file in files)
            {
                // Read each file
                StreamReader sr = new StreamReader(file);
                sr = File.OpenText(file);
                rawContent = sr.ReadToEnd();
                sr.Close();

                // Save all positions in file for ISA segment (Beginning of file)
                List<int> positions = new List<int>(checkNextISA(rawContent, out fieldDelimiter, out segmentDelimiter));
                for(int x=0; x< positions.Count; x++)
                {
                    int nextLine = (positions.Count > 1) ? (( x == (positions.Count-1)) ? rawContent.Length - 1 : positions[x + 1]) : rawContent.Length-1;
                    List<object> doc = parseLine(rawContent.Substring(positions[x], nextLine - positions[x]), fieldDelimiter, segmentDelimiter);
                    string fileType;

                    if (verifyData(doc, out fileType))
                    {
                        Save(doc, fileType);
                    } 
                }
            }

            Console.WriteLine(files);
        }

        // Evaluates each position of the ISA segment in the file
        protected static List<int> checkNextISA(string lines, out char fieldDelimiter, out char segmentDelimiter)
        {
            List<int> list = new List<int>();
            string pattern = null;

            if (lines.IndexOf("ISA~00~") != -1)
            {
                pattern = "ISA~00~";
                fieldDelimiter = "~".ToCharArray()[0];
                segmentDelimiter = lines.Substring(lines.IndexOf("GS") - 1, 1).ToCharArray()[0];
            }
            else
            {
                pattern = "ISA*00*";
                fieldDelimiter = "*".ToCharArray()[0];
                segmentDelimiter = lines.Substring(lines.IndexOf("GS") - 1, 1).ToCharArray()[0];
                //segmentDelimiter = "~".ToCharArray()[0];
            }

            for (int x = 0; x < lines.Length; x++)
            {
                int ix = lines.IndexOf(pattern, x);
                if (ix == -1)
                    break;
                list.Add(ix);
                x = ix;
            }

            return list;
        }

        // Divides in segments the line read.
        protected static List<object> parseLine(string line, char fieldDelimiter, char segmentDelimiter)
        {
            string[] segments = line.Split(segmentDelimiter);
            List<object> segmentList = new List<object>();
            for (int x=0; x<segments.Length; x++)
            {
                object segment = segments[x].Split(fieldDelimiter);
                segmentList.Add(segment);
                //Console.WriteLine(segments[x]);
            }
            return segmentList;
        }

        protected static bool verifyData(List<object> file, out string fileType)
        {
            string ISAControl = null, GSControl = null, STControl = null, IEAControl = null, GEControl = null, SEControl = null, 
                SECount = null, customerFileID = null, customerInternalID = null;
            fileType = null;
            int count = 0;
            foreach (object[] line in file)
            {
                switch (line[0].ToString())
                {
                    case "ISA":
                        ISAControl = line[13].ToString();
                        customerFileID = line[6].ToString().Trim();
                        break;
                    case "GS":
                        GSControl = line[6].ToString();
                        break;
                    case "ST":
                        fileType = line[1].ToString();
                        STControl = line[2].ToString();
                        count++;
                        break;
                    case "SE":
                        SECount = line[1].ToString();
                        SEControl = line[2].ToString();
                        count++;
                        break;
                    case "GE":
                        GEControl = line[2].ToString();
                        break;
                    case "IEA":
                        IEAControl = line[2].ToString();
                        break;
                    case "N1":
                        if (customerFileID == "BROSEKG")
                        {
                            if (line[1].ToString() == "MA")
                                customerInternalID = line[4].ToString();
                        }
                        count++;
                        break;
                    default:
                        count++;
                        break;
                }
            }

            getCustomerID(customerFileID, customerInternalID);

            return ISAControl != IEAControl | GSControl != GEControl | STControl != SEControl | int.Parse(SECount) != count | customerID == null ? false : true;
        }

        protected static void getCustomerID(string customerFileID, string customerInternalID)
        {
            if (customerInternalID == null)
                customerInternalID = "";

            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("dbconnection")))
            using (SqlCommand cmd = new SqlCommand($@"SELECT fk_CUST_ID FROM EDI_IN_CUSTOMER_ID WHERE CUST_FILE_ID = @customerFileID AND CUST_INTERNAL_ID = @customerInternalID;"))

            {
                cmd.Parameters.AddWithValue("@customerFileID", customerFileID);
                cmd.Parameters.AddWithValue("@customerInternalID", customerInternalID);
                cmd.Connection = con;
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        customerID = dr["fk_CUST_ID"].ToString();
                    }
                }
                con.Close();
            }
        }

        protected static void Save(List<object> list, string fileType)
        {
            object edi = null;
            string insert = null;
            switch (fileType)
            {
                case "820":
                    //edi = new X12820File();
                    break;
                case "824":
                    //edi = new X12824File();
                    break;
                case "830":
                    edi = new X12830File(list);
                    insert = GenerateInserts(edi, typeof(X12830File),"");
                    break;
                case "850":
                    //edi = new X12850File();
                    break;
                case "860":
                    //edi = new X12860File();
                    break;
                case "861":
                    //edi = new X12861File(list);
                    break;
                case "862":
                    //edi = new X12862File();
                    break;
                case "997":
                    //edi = new X12997File();
                    break;
                default:
                    break;
            }
        }

        protected static string GenerateInserts(object callerObj, System.Type type, string segment, bool isDetail = false)
        {
            var propArray = type.GetProperties();
            PropertyInfo propertyInfo = null;
            MethodInfo methodInfo = null;
            object obj = null;
            StringBuilder outputLine = new StringBuilder();
            PropertyInfo[] info = null;
            try
            {

                for (int i = 0; i < propArray.Length; i++)
                {
                    propertyInfo = callerObj.GetType().GetProperty(propArray[i].Name.ToString());
                    methodInfo = propertyInfo.PropertyType.GetMethod(propertyInfo.GetGetMethod().ToString());
                    obj = propertyInfo.GetValue(callerObj, null);
                    obj = (obj == null) ? "" : obj;
                    if (obj.ToString() != "")
                    {
                        switch (propArray[i].PropertyType.Name)
                        {
                            case "String":
                                if (!isDetail)
                                {
                                    outputLine.Append($@"INSERT INTO EDI_IN_FILE_HEADER VALUES ('" + customerID + "','1','" + segment + "','" +
                                                propArray[i].Name.ToString() + "','" + obj.ToString() + "');");
                                } else
                                {
                                    outputLine.Append($@"INSERT INTO EDI_IN_FILE_DETAIL VALUES ('1','" + segment + "','1','" +
                                                propArray[i].Name.ToString() + "','" + obj.ToString() + "');");
                                }
                                break;
                            case "TransactionSetHeader":
                                outputLine.Append(GenerateInserts(obj, typeof(TransactionSetHeader), "ST"));
                                outputLine.AppendLine();
                                break;
                            case "BeginningSegmentForPlanningSchedule":
                                outputLine.Append(GenerateInserts(obj, typeof(BeginningSegmentForPlanningSchedule), "BFR"));
                                outputLine.AppendLine();
                                break;
                            case "SpecialInstruction":
                                outputLine.Append(GenerateInserts(obj, typeof(SpecialInstruction),""));
                                outputLine.AppendLine();
                                break;
                            case "UnitDetail":
                                outputLine.Append(GenerateInserts(obj, typeof(UnitDetail),"UIT", true));
                                outputLine.AppendLine();
                                break;
                            case "ReferenceNumbers":
                                outputLine.Append(GenerateInserts(obj, typeof(ReferenceNumbers),"REF", true));
                                outputLine.AppendLine();
                                break;
                            case "AdministrativeCommunicationContact":
                                outputLine.Append(GenerateInserts(obj, typeof(AdministrativeCommunicationContact),"PER", true));
                                outputLine.AppendLine();
                                break;
                            case "ForecastSchedule":
                                outputLine.Append(GenerateInserts(obj, typeof(ForecastSchedule),"FST", true));
                                outputLine.AppendLine();
                                break;
                            case "ResourceAuthorization":
                                outputLine.Append(GenerateInserts(obj, typeof(ResourceAuthorization),"ATH",true));
                                outputLine.AppendLine();
                                break;
                            case "ShippedReceivedInformation":
                                outputLine.Append(GenerateInserts(obj, typeof(ShippedReceivedInformation),"SHP", true));
                                outputLine.AppendLine();
                                break;
                            case "List`1":
                                switch (propArray[i].Name)
                                {
                                    case "Names":
                                        foreach (object o in (List<Name>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(Name),"N1"));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    case "ItemIdentification":
                                        foreach (object o in (List<ItemIdentification>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(ItemIdentification),"LIN",true));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    case "ShipDeliveryPattern":
                                        foreach (object o in (List<ShipDeliveryPattern>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(ShipDeliveryPattern),""));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    case "ShippedReceivedInformation":
                                        foreach (object o in (List<ShippedReceivedInformation>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(ShippedReceivedInformation),"SHP", true));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    case "ForecastSchedule":
                                        foreach (object o in (List<ForecastSchedule>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(ForecastSchedule),"FST", true));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    case "ResourceAuthorization":
                                        foreach (object o in (List<ResourceAuthorization>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(ResourceAuthorization),"ATH", true));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    case "ReferenceNumbers":
                                        foreach (object o in (List<ReferenceNumbers>)obj)
                                        {
                                            outputLine.Append(GenerateInserts(o, typeof(ReferenceNumbers),"REF", true));
                                            outputLine.AppendLine();
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return outputLine.ToString();
        }

        //protected static void Insert(IX12File edi)
        //{


        //    //using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("dbconnection")))
        //    //using (SqlCommand cmd = new SqlCommand($@"SELECT fk_CUST_ID FROM EDI_IN_CUSTOMER_ID WHERE CUST_FILE_ID = @customerFileID AND CUST_INTERNAL_ID = @customerInternalID;"))

        //    //{
        //    //    cmd.Parameters.AddWithValue("@customerFileID", customerFileID);
        //    //    cmd.Parameters.AddWithValue("@customerInternalID", customerInternalID);
        //    //    cmd.Connection = con;
        //    //    con.Open();
        //    //    using (SqlDataReader dr = cmd.ExecuteReader())
        //    //    {
        //    //        while (dr.Read())
        //    //        {
        //    //            customerID = dr["fk_CUST_ID"].ToString();
        //    //        }
        //    //    }
        //    //    con.Close();
        //    //}
        //}
    }
}
