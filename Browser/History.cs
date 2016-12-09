using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using log4net;

namespace WHL.Browser
{
    public class History
    {
        public readonly Dictionary<int, Address>  List = new Dictionary<int, Address>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(History));

        private int currentIndex = 0;

        public History()
        {
            try
            {
                if (!File.Exists("Data/browserhistory.csv")) return;

                using (var sr = new StreamReader(@"Data/browserhistory.csv"))
                {
                    var records = new CsvReader(sr).GetRecords<Address>();

                    foreach (var record in records)
                    {
                        List.Add(record.Id, record);

                        currentIndex = record.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Browser.History.History] Critical error in load history. Exception = " + ex);
            }
        }

        public void Add(string url)
        {
            if (url == "http://") return;

            if (List.ContainsKey(currentIndex))
            {
                if (List[currentIndex].Url == url) return;
            }

            var index = GetIndex();

            if (index > currentIndex)
            {
                Remove(currentIndex, index);

                index = currentIndex;
            }

            List.Add(index, new Address{Id = index, Url = url} );

            WriteToFile();

            currentIndex = index;
        }

        private void Remove(int fromIndex, int index)
        {
            for (var i = fromIndex; i <= index; i++)
            {
                if(List.ContainsKey(i))
                {
                    List.Remove(List[i].Id);
                }
            }
        }

        public string Previous()
        {
            if (List.ContainsKey(currentIndex - 1) == false) return string.Empty;

            currentIndex = currentIndex - 1;

            return List[currentIndex].Url;
        }

        public string Next()
        {
            if (List.ContainsKey(currentIndex + 1) == false) return string.Empty;

            currentIndex = currentIndex + 1;

            return List[currentIndex].Url;
        }

        private int GetIndex()
        {
            int index = List.Keys.Concat(new[] { 0 }).Max();

            return index + 1;
        }

        private void WriteToFile()
        {
            using (var sw = new StreamWriter(@"Data/browserhistory.csv"))
            {
                var writer = new CsvWriter(sw);

                IEnumerable records = List.ToList();

                writer.WriteRecords(records);
            }
        }
    }
}
