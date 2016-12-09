using WHL.BLL;
using WHL.Browser;

namespace WHL
{
    public static class Global
    {
        public static PilotsEntity Pilots = new PilotsEntity();

        public static Infrastructure Infrastructure = new Infrastructure();

        public static SpaceEntity Space = new SpaceEntity();

        public static BrowserEntity Browser = new BrowserEntity();
    }
}
