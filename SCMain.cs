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
        private IList<UnitBlueprint> newUnits = new List<UnitBlueprint>();
        private IList<Faction> newFactions = new List<Faction>();
        private IList<TABSCampaignAsset> newCampaigns = new List<TABSCampaignAsset>();
        private IList<TABSCampaignLevelAsset> newCampaignLevels = new List<TABSCampaignLevelAsset>();
        private IList<VoiceBundle> newVoiceBundles = new List<VoiceBundle>();
        private IList<FactionIcon> newFactionIcons = new List<FactionIcon>();
        private IList<GameObject> newBases = new List<GameObject>();
        private IList<GameObject> newProps = new List<GameObject>();
        private IList<GameObject> newAbilities = new List<GameObject>();
        private IList<GameObject> newWeapons = new List<GameObject>();
        private IList<GameObject> newProjectiles = new List<GameObject>();

        private AssetBundle summon = AssetBundle.LoadFromMemory(Properties.Resources.summon);

        public SCMain()
        {
            var db = ContentDatabase.Instance().LandfallContentDatabase;
            List<UnitBlueprint> realList = (from unit in db.GetUnitBlueprints()
                                            where !db.GetUnitBlueprint(unit.Entity.GUID).IsCustomUnit
                                            orderby db.GetUnitBlueprint(unit.Entity.GUID).Name
                                            select db.GetUnitBlueprint(unit.Entity.GUID)).ToList();
            LoadFactions();
            LoadUnitBlueprints(db);
            LoadGameObjects();

            SummonerSingleton instance = SummonerSingleton.GetInstance();
            instance.summonerFaction = summon.LoadAsset<Faction>("Summoners");
            instance.summonerOriginal = summon.LoadAsset<UnitBlueprint>("Summoner");
            instance.summonerStats = new SummonerStats // seems like this is where the default values of a summoner are set
            {
                cooldown = 15f,
                minionsPerSpawn = 5,
                spawnables = new List<UnitBlueprint>(realList).ToArray()
            };;
            instance.ConfirmValues();

            AddContentToDatabase();
        }

        private void LoadGameObjects()
        {
            foreach (var gameObject in summon.LoadAllAssets<GameObject>())
            {
                if (gameObject == null) continue;
                
                if (gameObject.GetComponent<Unit>())
                {
                    newBases.Add(gameObject);
                }
                else if (gameObject.GetComponent<WeaponItem>())
                {
                    newWeapons.Add(gameObject);
                    HandleGameObject<WeaponItem>(gameObject);
                }
                else if (gameObject.GetComponent<ProjectileEntity>())
                {
                    newProjectiles.Add(gameObject);
                }
                else if (gameObject.GetComponent<SpecialAbility>())
                {
                    newAbilities.Add(gameObject);
                }
                else if (gameObject.GetComponent<PropItem>())
                {
                    newProps.Add(gameObject);
                    HandleGameObject<PropItem>(gameObject);
                }
            }
        }

        // todo: rename method
        private void HandleGameObject<T>(GameObject gameObject) where T : CharacterItem
        {
            int totalSubmeshes = 0;

            foreach (var rend in gameObject.GetComponentsInChildren<MeshFilter>()) 
            {
                if (rend.gameObject.activeSelf 
                    && rend.gameObject.activeInHierarchy 
                    && rend.mesh.subMeshCount > 0 
                    && rend.GetComponent<MeshRenderer>() != null
                    && rend.GetComponent<MeshRenderer>().enabled == true) {

                    totalSubmeshes += rend.mesh.subMeshCount;
                }
            }

            foreach (var rend in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>()) 
            {
                if (rend.gameObject.activeSelf 
                    && rend.sharedMesh.subMeshCount > 0 
                    && rend.enabled) {

                    totalSubmeshes += rend.sharedMesh.subMeshCount;
                }
            }

            if (totalSubmeshes == 0) return;
            
            float average = 1f / totalSubmeshes;
            var averageList = new List<float>();
            for (int i = 0; i < totalSubmeshes; i++)
            {
                averageList.Add(average);
            }

            gameObject.GetComponent<T>().SubmeshArea = null; // todo: why is this row needed when we overwrite it on the the next row?
            gameObject.GetComponent<T>().SubmeshArea = averageList.ToArray();
        }

        private void LoadUnitBlueprints(LandfallContentDatabase db)
        {
            foreach (var unit in summon.LoadAllAssets<UnitBlueprint>())
            {
                newUnits.Add(unit);

                foreach (var unitBase in db.GetUnitBases().ToList()) // todo: why ToList()?
                {
                    if (unit.UnitBase != null && unitBase.name == unit.UnitBase.name)
                    {
                        unit.UnitBase = unitBase;
                    }
                }

                foreach (var weapon in db.GetWeapons().ToList()) // todo: why ToList()?
                {
                    if (unit.RightWeapon != null && weapon.name == unit.RightWeapon.name)
                    {
                        unit.RightWeapon = weapon;
                    }

                    if (unit.LeftWeapon != null && weapon.name == unit.LeftWeapon.name)
                    {
                        unit.LeftWeapon = weapon;
                    }
                }
            }
        }

        private void LoadFactions()
        {
            foreach (var fac in summon.LoadAllAssets<Faction>())
            {
                newFactions.Add(fac);
            }
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

            // todo: document why
            ServiceLocator.GetService<CustomContentLoaderModIO>().QuickRefresh(WorkshopContentType.Unit, null);
        }
    }
}