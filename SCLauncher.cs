﻿using BepInEx;

namespace SummonerCreator
{
	[BepInPlugin("teamgrad.summoner", "Summoner Creator", "1.0.7")]
	public class SCLauncher : BaseUnityPlugin
	{
		public SCLauncher()
		{
			SCBinder.UnitGlad();
		}
	}
}
