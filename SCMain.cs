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
	        var nonStreamableAssets = (Dictionary<DatabaseID, UnityEngine.Object>)typeof(AssetLoader)
                .GetField("m_nonStreamableAssets", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(ContentDatabase.Instance().AssetLoader);
	        
            var db = ContentDatabase.Instance().LandfallContentDatabase;
            
            AddNewUnitsToDb(db, nonStreamableAssets);
            AddNewFactionsToDb(db, nonStreamableAssets);
            AddNewCampaignsToDb(db, nonStreamableAssets);
            AddNewCampaignLevelsToDb(db, nonStreamableAssets);
            AddNewVoiceBundlesToDb(db, nonStreamableAssets);
            AddNewFactionIconsToDb(db, nonStreamableAssets);
            AddNewUnitBasesToDb(db, nonStreamableAssets);
            AddNewPropsToDb(db, nonStreamableAssets);
            AddNewAbilitiesToDb(db, nonStreamableAssets);
            AddNewWeaponsToDb(db, nonStreamableAssets);
            AddNewProjectilesToDb(db, nonStreamableAssets);

            ServiceLocator.GetService<CustomContentLoaderModIO>().QuickRefresh(WorkshopContentType.Unit, null);
        }

        private void AddNewProjectilesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_projectiles");
            var projectiles = (Dictionary<DatabaseID, GameObject>) field.GetValue(db);

            foreach (var proj in newProjectiles)
            {
                var projectileGuid = proj.GetComponent<ProjectileEntity>().Entity.GUID;
                if (projectiles.ContainsKey(projectileGuid)) continue;

                projectiles.Add(projectileGuid, proj);
                nonStreamableAssets.Add(projectileGuid, proj);
            }

            field.SetValue(db, projectiles);
        }

        [NotNull]
        private static FieldInfo GetFieldInLandfallContentDb([NotNull] string fieldName)
        {
            var result = typeof(LandfallContentDatabase).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (result == null)
            {
                throw new System.Exception($"Field '{fieldName}' not found in ${nameof(LandfallContentDatabase)}");
            }

            return result;
        }

        private void AddNewWeaponsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_weapons");
            var weapons = (Dictionary<DatabaseID, GameObject>) field.GetValue(db);

            foreach (var weapon in newWeapons)
            {
                var weaponGuid = weapon.GetComponent<WeaponItem>().Entity.GUID;
                if (weapons.ContainsKey(weaponGuid)) continue;

                weapons.Add(weaponGuid, weapon);
                nonStreamableAssets.Add(weaponGuid, weapon);
            }
            field.SetValue(db, weapons);
        }

        private void AddNewAbilitiesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var abilities = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase)
                .GetField("m_combatMoves", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var ability in newAbilities)
            {
                if (!abilities.ContainsKey(ability.GetComponent<SpecialAbility>().Entity.GUID))
                {
                    abilities.Add(ability.GetComponent<SpecialAbility>().Entity.GUID, ability);
                    nonStreamableAssets.Add(ability.GetComponent<SpecialAbility>().Entity.GUID, ability);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_combatMoves", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, abilities);
        }

        private void AddNewPropsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var props = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase)
                .GetField("m_characterProps", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var prop in newProps)
            {
                if (!props.ContainsKey(prop.GetComponent<PropItem>().Entity.GUID))
                {
                    props.Add(prop.GetComponent<PropItem>().Entity.GUID, prop);
                    nonStreamableAssets.Add(prop.GetComponent<PropItem>().Entity.GUID, prop);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_characterProps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, props);
        }

        private void AddNewUnitBasesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var unitBases = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase)
                .GetField("m_unitBases", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var unitBase in newBases)
            {
                if (!unitBases.ContainsKey(unitBase.GetComponent<Unit>().Entity.GUID))
                {
                    unitBases.Add(unitBase.GetComponent<Unit>().Entity.GUID, unitBase);
                    nonStreamableAssets.Add(unitBase.GetComponent<Unit>().Entity.GUID, unitBase);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_unitBases", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, unitBases);
        }

        private void AddNewFactionIconsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var factionIcons = (List<DatabaseID>)typeof(LandfallContentDatabase)
                .GetField("m_factionIconIds", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var factionIcon in newFactionIcons)
            {
                if (!factionIcons.Contains(factionIcon.Entity.GUID))
                {
                    factionIcons.Add(factionIcon.Entity.GUID);
                    nonStreamableAssets.Add(factionIcon.Entity.GUID, factionIcon);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_factionIconIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, factionIcons);
        }

        private void AddNewVoiceBundlesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var voiceBundles = (Dictionary<DatabaseID, VoiceBundle>)typeof(LandfallContentDatabase)
                .GetField("m_voiceBundles", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var voiceBundle in newVoiceBundles)
            {
                if (!voiceBundles.ContainsKey(voiceBundle.Entity.GUID))
                {
                    voiceBundles.Add(voiceBundle.Entity.GUID, voiceBundle);
                    nonStreamableAssets.Add(voiceBundle.Entity.GUID, voiceBundle);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_voiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, voiceBundles);
        }

        private void AddNewCampaignLevelsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var campaignLevels = (Dictionary<DatabaseID, TABSCampaignLevelAsset>)typeof(LandfallContentDatabase)
                .GetField("m_campaignLevels", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var campaignLevel in newCampaignLevels)
            {
                if (!campaignLevels.ContainsKey(campaignLevel.Entity.GUID))
                {
                    campaignLevels.Add(campaignLevel.Entity.GUID, campaignLevel);
                    nonStreamableAssets.Add(campaignLevel.Entity.GUID, campaignLevel);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_campaignLevels", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, campaignLevels);
        }

        private void AddNewCampaignsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var campaigns = (Dictionary<DatabaseID, TABSCampaignAsset>)typeof(LandfallContentDatabase)
                .GetField("m_campaigns", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var campaign in newCampaigns)
            {
                if (!campaigns.ContainsKey(campaign.Entity.GUID))
                {
                    campaigns.Add(campaign.Entity.GUID, campaign);
                    nonStreamableAssets.Add(campaign.Entity.GUID, campaign);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_campaigns", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, campaigns);
        }

        private void AddNewFactionsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var factions = (Dictionary<DatabaseID, Faction>)typeof(LandfallContentDatabase)
                .GetField("m_factions", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            var defaultHotbarFactions = (List<DatabaseID>)typeof(LandfallContentDatabase)
                .GetField("m_defaultHotbarFactionIds", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var faction in newFactions)
            {
                if (!factions.ContainsKey(faction.Entity.GUID))
                {
                    factions.Add(faction.Entity.GUID, faction);
                    nonStreamableAssets.Add(faction.Entity.GUID, faction);
                    defaultHotbarFactions.Add(faction.Entity.GUID);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_factions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, factions);
            typeof(LandfallContentDatabase).GetField("m_defaultHotbarFactionIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, defaultHotbarFactions.OrderBy(x => factions[x].index).ToList());
        }

        private void AddNewUnitsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var units = (Dictionary<DatabaseID, UnitBlueprint>)typeof(LandfallContentDatabase)
                .GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db);

            foreach (var unit in newUnits)
            {
                if (!units.ContainsKey(unit.Entity.GUID))
                {
                    units.Add(unit.Entity.GUID, unit);
                    nonStreamableAssets.Add(unit.Entity.GUID, unit);
                }
            }
            typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, units);
        }

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
    }
}