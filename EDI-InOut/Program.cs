using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace EDI_InOut
{
    class Program
    {
        public static string CustomerID { get; set; }
        public static string HeaderID { get; set; }

        static void Main(string[] args)
        {
            // Get directory info
            string[] files = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings.Get("InboundFilesPath830"));
            string rawContent = null, queryStr = null, fileType=null;
            char fieldDelimiter, segmentDelimiter;
            int i = 0, lastImportNum = getLastImportNum();

            try
            {
                foreach (var file in files)
                {
                    // Read each file, close the stream so it can be moved later
                    using (StreamReader sr = File.OpenText(file))
                    {
                        rawContent = sr.ReadToEnd();

                        List<int> positions = new List<int>(CheckNextISA(rawContent, out fieldDelimiter, out segmentDelimiter));
                        for (int x = 0; x < positions.Count; x++)
                        {
                            int nextLine = (positions.Count > 1) ? ((x == (positions.Count - 1)) ? rawContent.Length - 1 : positions[x + 1]) : rawContent.Length - 1;
                            List<object> doc = ParseLine(rawContent.Substring(positions[x], nextLine - positions[x]), fieldDelimiter, segmentDelimiter);

                            if (VerifyData(doc, out fileType))
                            {
                                Save(doc, fileType);
                            }
                        }
                    }

                    lastImportNum++;
                    string newFile = "RECV-EDI-01-" + lastImportNum.ToString();
                    string moveFile = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings.Get("InboundFilesPathBackup"), newFile + ".edi");
                    System.IO.File.Move(file, moveFile);

                    // Insert in the table the information of the processed file
                    queryStr = $@"INSERT INTO EDI_INBOUND_FILES VALUES('" + CustomerID + "','" + HeaderID + "','" + fileType + "','" + newFile + "',GETDATE())";
                    ExecuteQuery(queryStr);
                }

                queryStr = $@"UPDATE EDI_DCX_LastImportNum set LastImportNum = " + lastImportNum.ToString();
                ExecuteQuery(queryStr);
            } catch (Exception ex)
            {
                throw ex;
            }
        }

        protected static int getLastImportNum()
        {
            int value = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("dbconnection")))
                using (SqlCommand cmd = new SqlCommand($@"select LastImportNum from EDI_DCX_LastImportNum;"))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            value = Int32.Parse(dr["LastImportNum"].ToString());
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return value;
        }

        // Evaluates each position of the ISA segment in the file
        protected static List<int> CheckNextISA(string lines, out char fieldDelimiter, out char segmentDelimiter)
        {
            List<int> list = new List<int>();
            string pattern = null;

            try
            {
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
                }

                for (int x = 0; x < lines.Length; x++)
                {
                    int ix = lines.IndexOf(pattern, x);
                    if (ix == -1)
                        break;
                    list.Add(ix);
                    x = ix;
                }
            } catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }

        // Divides in segments the line read.
        protected static List<object> ParseLine(string line, char fieldDelimiter, char segmentDelimiter)
        {
            string[] segments = line.Split(segmentDelimiter);
            List<object> segmentList = new List<object>();
            try
            {
                for (int x = 0; x < segments.Length; x++)
                {
                    object segment = segments[x].Split(fieldDelimiter);
                    segmentList.Add(segment);
                }
            } catch (Exception ex)
            {
                throw ex;
            }
            
            return segmentList;
        }

        // Verify ISA, GS and ST data if matches with respective summary data.
        protected static bool VerifyData(List<object> file, out string fileType)
        {
            string ISAControl = null, GSControl = null, STControl = null //IEAControl = null, GEControl = null, SEControl = null,
                , customerFileID = null, customerInternalID = null;
            fileType = null;
            int count = 0;
            foreach (object[] line in file)
            {
                switch (line[0].ToString())
                {
                    case "ISA":
                        ISAControl = line[13].ToString();
                        customerFileID = line[6].ToString().Trim();
                        HeaderID = line[9].ToString() + line[10].ToString();
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
                        //SECount = line[1].ToString();
                        //SEControl = *line[2].ToString();
                        //count++;
                        //Once SE is reached, make the validation and reset, as 824 from DCX has multiples ST-SE
                        if (STControl != line[2].ToString() | int.Parse(line[1].ToString()) != count + 1)
                        {
                            return false;
                        }
                        count = 0;
                        break;
                    case "GE":
                        //GEControl = line[2].ToString();
                        if (GSControl != line[2].ToString())
                        {
                            return false;
                        }
                        break;
                    case "IEA":
                        //IEAControl = line[2].ToString();
                        if (ISAControl != line[2].ToString())
                        {
                            return false;
                        }
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

            GetCustomerID(customerFileID, customerInternalID);

            //return ISAControl != IEAControl | GSControl != GEControl | STControl != SEControl | int.Parse(SECount) != count | customerID == null ? false : true;
            return CustomerID == null ? false : true;
        }

        // Gets the customer associated with the Partner ID. Brose needs to verify also which is the interal customer.
        protected static void GetCustomerID(string customerFileID, string customerInternalID)
        {
            try
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
                            CustomerID = dr["fk_CUST_ID"].ToString();
                        }
                    }
                    con.Close();
                }
            } catch (Exception ex)
            {
                throw ex;
            }
            
        }

        // According to the file type, it will bring the queries to insert.
        protected static void Save(List<object> list, string fileType)
        { 
            string insert = null;
            switch (fileType)
            {
                case "820":
                    insert = GenerateInserts820(list);
                    break;
                case "824":
                    insert = GenerateInserts824(list);
                    break;
                case "830":
                    insert = GenerateInserts830(list);
                    break;
                case "850":
                    insert = GenerateInserts850(list);
                    break;
                case "860":
                    insert = GenerateInserts860(list);
                    break;
                case "861":
                    insert = GenerateInserts861(list);
                    break;
                case "862":
                    insert = GenerateInserts862(list);
                    break;
                case "997":
                    insert = GenerateInserts997(list);
                    break;
                default:
                    break;
            }
            if (insert != null) ExecuteQuery(insert);
        }

        protected static string GenerateInserts820(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int nameLoopCount = 0, rmrLoopCount = 0, segmentLoopCount = 1, dateTimeLoopCount = 0;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BPR":
                        case "TRN":
                        case "CUR":
                        case "PER":
                        case "ENT":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "N1":
                            nameLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, nameLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "RMR":
                            rmrLoopCount++;
                            dateTimeLoopCount = 0;
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "IT1":
                        case "REF":
                        case "ITA":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            }
                            else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "RMR", rmrLoopCount) + ");");
                            output.AppendLine();
                            break;
                        case "DTM":
                            if (rmrLoopCount > 0)
                            {
                                dateTimeLoopCount++;
                                output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, dateTimeLoopCount, "RMR", rmrLoopCount) + ");");
                                output.AppendLine();
                            } else
                            {
                                output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                                output.AppendLine();
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts824(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int nameLoopCount = 0, otiLoopCount = 0, segmentLoopCount = 1;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BGN":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "N1":
                            nameLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, nameLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "OTI":
                            otiLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, otiLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "DTM":
                        case "REF":
                        case "TED":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            }
                            else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "OTI", otiLoopCount) + ");");
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts830(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int nameLoopCount = 0, detailLoopCount = 0, segmentLoopCount=1;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object [] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BFR":
                            output.Append($@"INSERT INTO "+ ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "N1":
                            nameLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, nameLoopCount, null, 1) + ");");
                            output.AppendLine();
                            break;
                        case "LIN":
                            detailLoopCount++;
                            output.Append($@"INSERT INTO "+ ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "UIT":
                        case "PER":
                        case "ATH":
                        case "FST":
                        case "SHP":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            } else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "LIN",detailLoopCount) + ");");
                            output.AppendLine();
                            break;
                        case "REF":
                            if (prevSegment == "SHP")
                            {
                                output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "SHP", detailLoopCount) + ");");
                            } else
                            {
                                output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, "LIN", detailLoopCount) + ");");
                            }
                            
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts850(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int refLoopCount = 0, pkgLoopCount = 0, po1LoopCount = 0, segmentLoopCount = 1;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BEG":
                        case "DTM":
                        case "ITD":
                        case "TD5":
                        case "N1":
                        case "N3":
                        case "N4":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "REF":
                            refLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, refLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "PKG":
                            pkgLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, pkgLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "PO1":
                            po1LoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "PID":
                        case "PO4":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            }
                            else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "PO1", po1LoopCount) + ");");
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts860(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int pocLoopCount = 0, segmentLoopCount = 1;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BCH":
                        case "DTM":
                        case "N1":
                        case "N3":
                        case "N4":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "POC":
                            pocLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "PID":
                        case "SCH":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            }
                            else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "PO1", pocLoopCount) + ");");
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts861(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int rcdLoopCount = 0, segmentLoopCount = 1;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BRA":
                        case "DTM":
                        case "PRF":
                        case "N1":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "RCD":
                            rcdLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "LIN":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            }
                            else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "PO1", rcdLoopCount) + ");");
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts862(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            int refLoopCount = 0, pkgLoopCount = 0, linLoopCount = 0, segmentLoopCount = 1;
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL", currSegment = null, prevSegment = null;
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                        case "BSS":
                        case "N1":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "REF":
                            refLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, refLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "PKG":
                            pkgLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, pkgLoopCount, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "LIN":
                            linLoopCount++;
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "UIT":
                        case "PO4":
                            currSegment = obj[0].ToString();
                            if (prevSegment != currSegment)
                            {
                                prevSegment = currSegment;
                                segmentLoopCount = 1;
                            }
                            else
                            {
                                segmentLoopCount++;
                            }
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, segmentLoopCount, "PO1", linLoopCount) + ");");
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        protected static string GenerateInserts997(List<object> list)
        {
            StringBuilder output = new StringBuilder("");
            string ediInboundDoc = "", ediInboundHeader = "EDI_INBOUND_HEADER", ediInboundDetail = "EDI_INBOUND_DETAIL";
            try
            {
                foreach (object[] obj in list)
                {
                    switch (obj[0])
                    {
                        case "ST":
                            output.Append($@"INSERT INTO " + ediInboundHeader + " VALUES (" + GenerateParameters(obj, true, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        case "AK1":
                        case "AK2":
                        case "AK5":
                        case "AK9":
                            output.Append($@"INSERT INTO " + ediInboundDetail + " VALUES (" + GenerateParameters(obj, false, 1, null, 0) + ");");
                            output.AppendLine();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        // Constructs the query's parameters to be inserted
        protected static string GenerateParameters(object [] obj, bool isHeader, int iteration = 1, string loopSegment = null, int loopcount = 0)
        {
            StringBuilder output = new StringBuilder("");
            int colCount = 0, colsQty = (isHeader) ? 15 : 31;
            try
            {
                output.Append($@"'" + CustomerID + "','" + HeaderID + "','" + obj[0].ToString() + "'," + iteration +",'" + loopSegment + "','" + loopcount + "'");
                for (int i = 1; i < obj.Length-1; i++)
                {
                    output.Append($@",'" + obj[i].ToString() + "'");
                    colCount++;
                }
                output.Append($@",'" + obj[obj.Length-1].ToString() + "'");
                colCount++;

                for(int i = 0; i < (colsQty -colCount); i++)
                {
                    output.Append($@",NULL");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return output.ToString();
        }

        // Updates tables in DB
        protected static void ExecuteQuery(string query)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings.Get("dbconnection")))
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            } catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
