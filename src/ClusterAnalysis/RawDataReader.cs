using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterAnalysis
{
    public class RawDataReader
    {
        public static double[][] ReadFromCsv(string fileName)
        {
            List<double[]> r = new List<double[]>();

            using (var reader = new StreamReader(fileName))
            {
                string l = reader.ReadLine();
                while (reader.Peek() >= 0)
                {
                    l = reader.ReadLine().Trim();
                    if(!string.IsNullOrWhiteSpace(l))
                    {
                        var col = l.Split(';');
                        var val = new double[col.Length - 1];
                        r.Add(val);
                        for (int i = 1; i < col.Length; i++)
                        {
                            val[i - 1] = double.Parse(col[i]);
                        }
                    }
                }
            }
            return r.ToArray();
        }
    }
}
