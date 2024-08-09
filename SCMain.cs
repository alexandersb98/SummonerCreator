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
                var guid = proj.GetComponent<ProjectileEntity>().Entity.GUID;
                if (projectiles.ContainsKey(guid)) continue;

                projectiles.Add(guid, proj);
                nonStreamableAssets.Add(guid, proj);
            }

            field.SetValue(db, projectiles);
        }

        private void AddNewWeaponsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_weapons");
            var weapons = (Dictionary<DatabaseID, GameObject>) field.GetValue(db);

            foreach (var weapon in newWeapons)
            {
                var guid = weapon.GetComponent<WeaponItem>().Entity.GUID;
                if (weapons.ContainsKey(guid)) continue;

                weapons.Add(guid, weapon);
                nonStreamableAssets.Add(guid, weapon);
            }

            field.SetValue(db, weapons);
        }

        private void AddNewAbilitiesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_combatMoves");
            var abilities = (Dictionary<DatabaseID, GameObject>) field.GetValue(db);

            foreach (var ability in newAbilities)
            {
                var guid = ability.GetComponent<SpecialAbility>().Entity.GUID;
                if (abilities.ContainsKey(guid)) continue;
                
                abilities.Add(guid, ability);
                nonStreamableAssets.Add(guid, ability);
            }

            field.SetValue(db, abilities);
        }

        private void AddNewPropsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_characterProps");
            var props = (Dictionary<DatabaseID, GameObject>) field.GetValue(db);

            foreach (var prop in newProps)
            {
                var guid = prop.GetComponent<PropItem>().Entity.GUID;
                if (props.ContainsKey(guid)) continue;

                props.Add(guid, prop);
                nonStreamableAssets.Add(guid, prop);
            }

            field.SetValue(db, props);
        }

        private void AddNewUnitBasesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_unitBases");
            var unitBases = (Dictionary<DatabaseID, GameObject>) field.GetValue(db);

            foreach (var unitBase in newBases)
            {
                var guid = unitBase.GetComponent<Unit>().Entity.GUID;
                if (unitBases.ContainsKey(guid)) continue;

                unitBases.Add(guid, unitBase);
                nonStreamableAssets.Add(guid, unitBase);
            }

            field.SetValue(db, unitBases);
        }

        private void AddNewFactionIconsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_factionIconIds");
            var factionIcons = (List<DatabaseID>) field.GetValue(db);

            foreach (var factionIcon in newFactionIcons)
            {
                var guid = factionIcon.Entity.GUID;
                if (factionIcons.Contains(guid)) continue;

                factionIcons.Add(guid);
                nonStreamableAssets.Add(guid, factionIcon);
            }

            field.SetValue(db, factionIcons);
        }

        private void AddNewVoiceBundlesToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_voiceBundles");
            var voiceBundles = (Dictionary<DatabaseID, VoiceBundle>) field.GetValue(db);

            foreach (var voiceBundle in newVoiceBundles)
            {
                var guid = voiceBundle.Entity.GUID;
                if (voiceBundles.ContainsKey(guid)) continue;
                
                voiceBundles.Add(guid, voiceBundle);
                nonStreamableAssets.Add(guid, voiceBundle);
            }

            field.SetValue(db, voiceBundles);
        }

        private void AddNewCampaignLevelsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_campaignLevels");
            var campaignLevels = (Dictionary<DatabaseID, TABSCampaignLevelAsset>) field.GetValue(db);

            foreach (var campaignLevel in newCampaignLevels)
            {
                var guid = campaignLevel.Entity.GUID;
                if (campaignLevels.ContainsKey(guid)) continue;
                
                campaignLevels.Add(guid, campaignLevel);
                nonStreamableAssets.Add(guid, campaignLevel);
            }

            field.SetValue(db, campaignLevels);
        }

        private void AddNewCampaignsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_campaigns");
            var campaigns = (Dictionary<DatabaseID, TABSCampaignAsset>) field.GetValue(db);

            foreach (var campaign in newCampaigns)
            {
                var guid = campaign.Entity.GUID;
                if (campaigns.ContainsKey(guid)) continue;
                
                campaigns.Add(guid, campaign);
                nonStreamableAssets.Add(guid, campaign);
            }

            field.SetValue(db, campaigns);
        }

        private void AddNewFactionsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var factionsField = GetFieldInLandfallContentDb("m_factions");
            var factions = (Dictionary<DatabaseID, Faction>) factionsField.GetValue(db);

            var defaultHotbarFactionIdsField = GetFieldInLandfallContentDb("m_defaultHotbarFactionIds");
            var defaultHotbarFactionIds = (List<DatabaseID>) defaultHotbarFactionIdsField.GetValue(db);

            foreach (var faction in newFactions)
            {
                var guid = faction.Entity.GUID;
                if (factions.ContainsKey(guid)) continue;
                
                factions.Add(guid, faction);
                nonStreamableAssets.Add(guid, faction);
                defaultHotbarFactionIds.Add(guid);
            }

            factionsField.SetValue(db, factions);
            defaultHotbarFactionIdsField.SetValue(db, defaultHotbarFactionIds.OrderBy(x => factions[x].index).ToList());
        }

        private void AddNewUnitsToDb(LandfallContentDatabase db, Dictionary<DatabaseID, Object> nonStreamableAssets)
        {
            var field = GetFieldInLandfallContentDb("m_unitBlueprints");
            var units = (Dictionary<DatabaseID, UnitBlueprint>) field.GetValue(db);

            foreach (var unit in newUnits)
            {
                var guid = unit.Entity.GUID;
                if (units.ContainsKey(guid)) continue;
                
                units.Add(guid, unit);
                nonStreamableAssets.Add(guid, unit);
            }

            field.SetValue(db, units);
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
    }
}