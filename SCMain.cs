using UnityEngine;
using Landfall.TABS;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using DM;
using JetBrains.Annotations;
using Landfall.TABS.Workshop;
using Landfall.TABS.UnitEditor;

namespace SummonerCreator
{
    public class SCMain
    {
        public List<UnitBlueprint> newUnits = new List<UnitBlueprint>();
        public List<Faction> newFactions = new List<Faction>();
        public List<TABSCampaignAsset> newCampaigns = new List<TABSCampaignAsset>();
        public List<TABSCampaignLevelAsset> newCampaignLevels = new List<TABSCampaignLevelAsset>();
        public List<VoiceBundle> newVoiceBundles = new List<VoiceBundle>();
        public List<FactionIcon> newFactionIcons = new List<FactionIcon>();
        public List<GameObject> newBases = new List<GameObject>();
        public List<GameObject> newProps = new List<GameObject>();
        public List<GameObject> newAbilities = new List<GameObject>();
        public List<GameObject> newWeapons = new List<GameObject>();
        public List<GameObject> newProjectiles = new List<GameObject>();

        public AssetBundle summon = AssetBundle.LoadFromMemory(Properties.Resources.summon);

        public SCMain()
        {
            var db = ContentDatabase.Instance().LandfallContentDatabase;
            List<UnitBlueprint> realList = (from unit in db.GetUnitBlueprints()
                                            where !db.GetUnitBlueprint(unit.Entity.GUID).IsCustomUnit
                                            orderby db.GetUnitBlueprint(unit.Entity.GUID).Name
                                            select db.GetUnitBlueprint(unit.Entity.GUID)).ToList();
            foreach (var fac in summon.LoadAllAssets<Faction>())
            {
                newFactions.Add(fac);
            }
            foreach (var unit in summon.LoadAllAssets<UnitBlueprint>())
            {
                newUnits.Add(unit);
                foreach (var b in db.GetUnitBases().ToList()) { if (unit.UnitBase != null) { if (b.name == unit.UnitBase.name) { unit.UnitBase = b; } } }
                foreach (var b in db.GetWeapons().ToList()) { if (unit.RightWeapon != null && b.name == unit.RightWeapon.name) unit.RightWeapon = b; if (unit.LeftWeapon != null && b.name == unit.LeftWeapon.name) unit.LeftWeapon = b; }
            }
            foreach (var objecting in summon.LoadAllAssets<GameObject>()) 
            {
                if (objecting != null) {

                    if (objecting.GetComponent<Unit>()) newBases.Add(objecting);
                    else if (objecting.GetComponent<WeaponItem>()) {
                        newWeapons.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

                                totalSubmeshes += rend.sharedMesh.subMeshCount;
                            }
                        }
                        if (totalSubmeshes != 0) {
                            float average = 1f / totalSubmeshes;
                            var averageList = new List<float>();
                            for (int i = 0; i < totalSubmeshes; i++) { averageList.Add(average); }
                            objecting.GetComponent<WeaponItem>().SubmeshArea = null;
                            objecting.GetComponent<WeaponItem>().SubmeshArea = averageList.ToArray();
                        }
                    }
                    else if (objecting.GetComponent<ProjectileEntity>()) newProjectiles.Add(objecting);
                    else if (objecting.GetComponent<SpecialAbility>()) newAbilities.Add(objecting);
                    else if (objecting.GetComponent<PropItem>()) {
                        newProps.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

                                totalSubmeshes += rend.sharedMesh.subMeshCount;
                            }
                        }
                        if (totalSubmeshes != 0) {
                            float average = 1f / totalSubmeshes;
                            var averageList = new List<float>();
                            for (int i = 0; i < totalSubmeshes; i++) { averageList.Add(average); }
                            objecting.GetComponent<PropItem>().SubmeshArea = null;
                            objecting.GetComponent<PropItem>().SubmeshArea = averageList.ToArray();
                        }
                    }
                }
            }
            SummonerStats summonerStats = new SummonerStats
            {
                cooldown = 15f,
                minionsPerSpawn = 5,
                spawnables = new List<UnitBlueprint>(realList).ToArray()
            };
            SummonerSingleton shitstance = SummonerSingleton.GetInstance();
            shitstance.summonerFaction = summon.LoadAsset<Faction>("Summoners");
            shitstance.summonerOriginal = summon.LoadAsset<UnitBlueprint>("Summoner");
            shitstance.summonerStats = summonerStats;
            shitstance.ConfirmValues();

            AddContentToDatabase();
        }
        
        public void AddContentToDatabase()
        {
            var contentAdder = new ModContentAdder(
                landfallContentDatabase: ContentDatabase.Instance().LandfallContentDatabase, 
                assetLoader: ContentDatabase.Instance().AssetLoader);

            contentAdder.AddUnitsToDb(newUnits)
                .AddFactionsToDb(newFactions)
                .AddCampaignsToDb(newCampaigns)
                .AddCampaignLevelsToDb(newCampaignLevels)
                .AddVoiceBundlesToDb(newVoiceBundles)
                .AddFactionIconsToDb(newFactionIcons)
                .AddUnitBasesToDb(newBases)
                .AddPropsToDb(newProps)
                .AddAbilitiesToDb(newAbilities)
                .AddWeaponsToDb(newWeapons)
                .AddProjectilesToDb(newProjectiles);

            ServiceLocator.GetService<CustomContentLoaderModIO>().QuickRefresh(WorkshopContentType.Unit, null);
        }
    }
}