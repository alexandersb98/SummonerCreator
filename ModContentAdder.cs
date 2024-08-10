using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DM;
using JetBrains.Annotations;
using Landfall.TABS;
using Landfall.TABS.UnitEditor;
using Landfall.TABS.Workshop;
using UnityEngine;
using Object = UnityEngine.Object;

/// Method chaining "pseudo-builder" pattern for adding new content to the LandfallContentDatabase
public class ModContentAdder
{
    [NotNull] private readonly LandfallContentDatabase landfallContentDatabase;
    [NotNull] private readonly IDictionary<DatabaseID, Object> nonStreamableAssets;

    public ModContentAdder(
        [NotNull] LandfallContentDatabase landfallContentDatabase,
        [NotNull] AssetLoader assetLoader)
    {
        this.landfallContentDatabase = landfallContentDatabase;

        var nonStreamableAssetsField = GetFieldFromAssetLoader("m_nonStreamableAssets");
        this.nonStreamableAssets = (Dictionary<DatabaseID, UnityEngine.Object>) nonStreamableAssetsField.GetValue(assetLoader);
    }


    [NotNull]
    public ModContentAdder AddNewProjectilesToDb([NotNull, ItemNotNull] IList<GameObject> newProjectiles)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_projectiles", 
            gameObjects: newProjectiles, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<ProjectileEntity>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewWeaponsToDb([NotNull, ItemNotNull] IList<GameObject> newWeapons)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_weapons", 
            gameObjects: newWeapons, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<WeaponItem>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewAbilitiesToDb([NotNull, ItemNotNull] IList<GameObject> newAbilities)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_combatMoves", 
            gameObjects: newAbilities, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<SpecialAbility>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewPropsToDb([NotNull, ItemNotNull] IList<GameObject> newProps)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_characterProps", 
            gameObjects: newProps, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<PropItem>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewUnitBasesToDb([NotNull, ItemNotNull] IList<GameObject> newUnitBases)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_unitBases", 
            gameObjects: newUnitBases, 
            getDatabaseIdFromGameObjectFn: (x) => x.GetComponent<Unit>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewFactionIconsToDb([NotNull, ItemNotNull] IList<FactionIcon> newFactionIcons)
    {
        return AddGameObjectsToDbList(
            landfallContentDatabaseFieldName: "m_factionIconIds", 
            gameObjects: newFactionIcons,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewVoiceBundlesToDb([NotNull, ItemNotNull] IList<VoiceBundle> newVoiceBundles)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_voiceBundles", 
            gameObjects: newVoiceBundles,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewCampaignLevelsToDb([NotNull, ItemNotNull] IList<TABSCampaignLevelAsset> newCampaignLevels)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_campaignLevels", 
            gameObjects: newCampaignLevels,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddNewCampaignsToDb([NotNull, ItemNotNull] IList<TABSCampaignAsset> newCampaigns)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_campaigns", 
            gameObjects: newCampaigns, 
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    private ModContentAdder AddGameObjectsToDbDictionary<TGameObject>(
        [NotNull] string landfallContentDatabaseFieldName,
        [NotNull, ItemNotNull] IList<TGameObject> gameObjects,
        [NotNull] Func<TGameObject, DatabaseID> getDatabaseIdFromGameObjectFn) where TGameObject : UnityEngine.Object
    {
        var field = GetFieldFromLandfallContentDatabase(landfallContentDatabaseFieldName);
        var currentGameObjects = (Dictionary<DatabaseID, TGameObject>) field.GetValue(landfallContentDatabase);

        foreach (var gameObject in gameObjects)
        {
            var guid = getDatabaseIdFromGameObjectFn(gameObject);
            if (currentGameObjects.ContainsKey(guid)) continue;

            currentGameObjects.Add(guid, gameObject);
            nonStreamableAssets.Add(guid, gameObject);
        }

        field.SetValue(landfallContentDatabase, currentGameObjects);
        return this;
    }

    [NotNull]
    private ModContentAdder AddGameObjectsToDbList<TGameObject>(
        [NotNull] string landfallContentDatabaseFieldName,
        [NotNull, ItemNotNull] IList<TGameObject> gameObjects,
        [NotNull] Func<TGameObject, DatabaseID> getDatabaseIdFromGameObjectFn) where TGameObject : UnityEngine.Object
    {
        var field = GetFieldFromLandfallContentDatabase(landfallContentDatabaseFieldName);
        var currentGameObjects = (List<DatabaseID>) field.GetValue(landfallContentDatabase);

        foreach (var gameObject in gameObjects)
        {
            var guid = getDatabaseIdFromGameObjectFn(gameObject);
            if (currentGameObjects.Contains(guid)) continue;

            currentGameObjects.Add(guid);
            nonStreamableAssets.Add(guid, gameObject);
        }

        field.SetValue(landfallContentDatabase, currentGameObjects);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewFactionsToDb([NotNull, ItemNotNull] IList<Faction> newFactions)
    {
        AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_factions", 
            gameObjects: newFactions, 
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);

        AddNewFactionsToDefaultHotbarFactions(newFactions);

        return this;
    }

    private void AddNewFactionsToDefaultHotbarFactions(IList<Faction> newFactions)
    {
        var factionsField = GetFieldFromLandfallContentDatabase("m_factions");
        var factions = (Dictionary<DatabaseID, Faction>)factionsField.GetValue(landfallContentDatabase);

        var defaultHotbarFactionIdsField = GetFieldFromLandfallContentDatabase("m_defaultHotbarFactionIds");
        var defaultHotbarFactionIds = (List<DatabaseID>)defaultHotbarFactionIdsField.GetValue(landfallContentDatabase);

        foreach (var faction in newFactions)
        {
            var guid = faction.Entity.GUID;
            if (factions.ContainsKey(guid)) continue;

            defaultHotbarFactionIds.Add(guid);
            nonStreamableAssets.Add(guid, faction);
        }
        
        var defaultHotbarFactionIdsOrderedAscendingly = defaultHotbarFactionIds.OrderBy(x => factions[x].index).ToList();
        defaultHotbarFactionIdsField.SetValue(landfallContentDatabase,
            value: defaultHotbarFactionIdsOrderedAscendingly);
    }

    public ModContentAdder AddNewUnitsToDb([NotNull, ItemNotNull] IList<UnitBlueprint> newUnits)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: "m_unitBlueprints", 
            gameObjects: newUnits,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    private static FieldInfo GetFieldFromLandfallContentDatabase([NotNull] string fieldName)
    {
        return GetFieldFromClass<LandfallContentDatabase>(fieldName);
    }

    [NotNull]
    private static FieldInfo GetFieldFromAssetLoader([NotNull] string fieldName)
    {
        return GetFieldFromClass<AssetLoader>(fieldName);
    }

    [NotNull]
    private static FieldInfo GetFieldFromClass<TClass>([NotNull] string fieldName)
    {
        var result = typeof(TClass).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return result == null
            ? throw new System.Exception($"Field '{fieldName}' not found in ${nameof(TClass)}")
            : result;
    }
}