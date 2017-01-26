using System.Collections.Generic;
using System.IO;
using CsvHelper;
using log4net;

namespace EveJimaCore.BLL
{
    public class SpaceEntity
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpaceEntity));

        public readonly Dictionary<string, WormholeEntity> Wormholes = new Dictionary<string, WormholeEntity>();

        public readonly Dictionary<string, StarSystemEntity> SolarSystems = new Dictionary<string, StarSystemEntity>();

        public readonly Dictionary<string, string> BasicSolarSystems = new Dictionary<string, string>();

        public SpaceEntity()
        {
            LoadWormholes();

            LoadStarSystems();

            LoadBasicSolarSystems();
        }

        private void LoadBasicSolarSystems()
        {
            using (var sr = new StreamReader(@"Data/WSpaceSystemInfo - Basic Solar Systems.csv"))
            {
                var records = new CsvReader(sr).GetRecords<BasicSolarSystem>();

                foreach (var record in records)
                {
                    Log.DebugFormat("[SpaceEntity.LoadBasicSolarSystems] Read csv row. {0} {1}", record.Name, record.Id);

                    BasicSolarSystems.Add(record.Name.Trim(), record.Id.Trim());
                }
            }
        }

        private void LoadWormholes()
        {
            using (var sr = new StreamReader(@"Data/WSpaceSystemInfo - Wormholes.csv"))
            {
                var records = new CsvReader(sr).GetRecords<WormholeEntity>();

                foreach (var record in records)
                {
                    Log.DebugFormat("[SpaceEntity.LoadWormholes] Read csv row. {0} {1}", record.Name, record.LeadsTo);

                    Wormholes.Add(record.Name.Trim(), record);
                }
            }
        }

        private void LoadStarSystems()
        {
            using (var sr = new StreamReader(@"Data/WSpaceSystemInfo - Systems.csv"))
            {
                var records = new CsvReader(sr).GetRecords<StarSystemEntity>();

                foreach (var record in records)
                {
                    Log.DebugFormat("[SpaceEntity.LoadStarSystems] Read csv row. {0} {1}, {2}", record.System, record.Class, record.Effect);

                    SolarSystems.Add(record.System.Trim(), record);
                }
            }
        }
    }
}
