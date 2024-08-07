using BepInEx;

namespace SummonerCreator
{
    // This mod is a fork of the original Summoner Creator mod by Team Grad.
    [BepInPlugin(GUID: "asb.summoner", Name: "ASB Summoner Creator", Version: "1.0.0")]
	public class SCLauncher : BaseUnityPlugin
	{
		public SCLauncher()
		{
			SCBinder.UnitGlad();
		}
	}
}
