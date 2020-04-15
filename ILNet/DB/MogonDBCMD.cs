using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.DB
{
    public class MogonDBCMD
    {

        /// <summary>
        /// This class is used to sort which by using merge sort
        /// </summary>
        public class MergeSort
        {
            public static void MergeSortFunc(ref List<BsonDocument> data, int start, int end, string condition)
            {
                if (start < end)
                {
                    int middle = (start + end) / 2;
                    MergeSortFunc(ref data, start, middle, condition);
                    MergeSortFunc(ref data, middle + 1, end, condition);

                    Merge(ref data, start, middle, middle + 1, end, condition);
                }
            }

            public static void Merge(ref List<BsonDocument> data, int startOne, int endOne, int startTwo, int endTwo, string condition)
            {
                int i, j;
                i = startOne;
                j = startTwo;
                //int[] temp = new int[endTwo - startOne + 1];
                List<BsonDocument> temp = new List<BsonDocument>();

                int m = 0;
                while (i < endOne && j < endTwo)
                {
                    if (Convert.ToInt32(data[i].GetValue(condition).ToString()) > Convert.ToInt32(data[j].GetValue(condition).ToString()))
                    {
                        temp[m] = data[j];
                        m++;
                        j++;
                    }
                    else
                    {
                        temp[m] = data[i];
                        m++;
                        i++;
                    }
                }

                while (i < endOne)
                {
                    temp[m] = data[i];
                    m++;
                    i++;
                }

                while (j < endTwo)
                {
                    temp[m] = data[j];
                    m++;
                    i++;
                }

                foreach (BsonDocument item in temp)
                {
                    data[startOne] = item;
                    startOne++;
                }
            }

            public static void Spy(List<BsonDocument> data)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    Console.WriteLine("Data is:" + data[i]);
                }
                Console.WriteLine("Spy...");
            }

        }

    }
}
