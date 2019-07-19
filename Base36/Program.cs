using System;
using System.Collections;

namespace Base36Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // This works with Base36 Nuget Package
            //string dateString = "000";
            //DateTime tempDate = DateTime.Now;
            //DateTime baseDate = new DateTime (2017,1,1);
            //int tempInt = (tempDate - baseDate).Days; //minus 1 because 2017-01-01 = 000
            //var dateIndex = Base36Extensions.ToBase36(tempInt);
            //dateString = dateString.Substring(0, (dateString.Length) - dateIndex.Length) + dateIndex;
            //Console.Write(dateString);


            string dateString = "000";
            DateTime tempDate = DateTime.Now;
            DateTime baseDate = new DateTime(2017, 1, 1);
            int tempInt = (tempDate - baseDate).Days; //minus 1 because 2017-01-01 = 000
            string dateIndex = Convert10To36(tempInt);
            dateString = dateString.Substring(0, (dateString.Length) - dateIndex.Length) + dateIndex;
            Console.Write(dateString);
        }

        public static string Convert10To36(int num)
        {
            ArrayList myArry = new ArrayList();
            bool flag = true;
            while (flag)
            {
                myArry.Add(toChar(num % 36));
                if ((Math.Floor((decimal)num / 36)) >= 1)
                {
                    num = (int) Math.Floor((decimal)(num / 36));
                }
                else
                {
                    flag = false;
                }
            }

            string str = "";
            myArry.Reverse();
            for (int i = 0; i < myArry.Count; i++)
            {
                str += myArry[i];
            }
            return str;
        }

        public static string toChar(int num)
        {
            string tempStr = "";
            if (num < 10)
            {
                tempStr = num.ToString();
            }
            else
            {
                tempStr = (Convert.ToChar(65 + (num - 10)).ToString());
            }
            return tempStr;
        }
        
    }
}
