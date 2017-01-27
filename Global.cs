using WHL.BLL;
using WHL.WhlControls;

namespace WHL
{
    public static class Global
    {
        public static string Version = "1.26";

        public static PilotsEntity Pilots = new PilotsEntity();

        public static Infrastructure Infrastructure = new Infrastructure();

        public static SpaceEntity Space = new SpaceEntity();

        public static whlBrowser Browser;
    }
}
