using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using log4net;

namespace WHL.BLL
{
    //public class WormholeEntity
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public string Life { get; set; }
    //    public string MaxStableMass { get; set; }
    //    public string MaxJumpMass { get; set; }
    //}

    public class WormholesEntity
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WormholesEntity));

        private readonly List<WormholeEntity> _list = new List<WormholeEntity>();

        public WormholeEntity GetWormhole(string id)
        {
            Log.DebugFormat("[Wormholes.GetWormhole] name is {0}", id);

            foreach (var wormhole in _list)
            {
                if (wormhole.Id.Trim() == id.Trim())
                {
                    return wormhole;
                }
            }

            return null;
            //return _list.FirstOrDefault(wormholeType => wormholeType.Id.Trim() == id.Trim());
        }

        public WormholesEntity()
        {
            using (var sr = new StreamReader(@"Data/WormholesList.csv"))
            {
                var reader = new CsvReader(sr);

                //CSVReader will now read the whole file into an enumerable
                var records = reader.GetRecords<WormholeEntity>();

                foreach (WormholeEntity record in records)
                {
                    Log.DebugFormat("[Wormholes.Wormholes] Read csv row. {0} {1}, {2}", record.Id, record.Name, record.Life);

                    record.MaxStableMass = record.MaxStableMass.Replace(":", "");

                    _list.Add(record);
                }
            }

            //GetAdditionalInfo();
        }

        private void GetAdditionalInfo()
        {
            var list = new List<WormholeEntity>();

            foreach (var wormholeType in _list)
            {
                var data = WHL.UiTools.Tools.ReadFile("http://www.ellatha.com/eve/wormholelistview.asp?key=Wormhole+" + wormholeType.Id, Log);

                var wormhole = new WormholeEntity
                {
                    Id = wormholeType.Id,
                    Name = wormholeType.Name,
                    Life = wormholeType.Life
                };


                var dataParts = data.Split(new[] { "Max Stable Mass</b>" }, StringSplitOptions.None)[1].Split(new[] { "&nbsp;kg" }, StringSplitOptions.None)[0];

                wormhole.MaxStableMass = dataParts;

                dataParts = data.Split(new[] { "<b>Max Jump Mass</b>:" }, StringSplitOptions.None)[1].Split(new[] { "&nbsp;kg" }, StringSplitOptions.None)[0];

                wormhole.MaxJumpMass = dataParts;

                list.Add(wormhole);
            }

            using (var sw = new StreamWriter(@"WormholesList.csv"))
            {
                var writer = new CsvWriter(sw);

                writer.WriteRecords(list);
            }
        }
    }
}
