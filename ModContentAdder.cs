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

    private static class FieldNames
    {
        public const string Projectiles = "m_projectiles";
        public const string Weapons = "m_weapons";
        public const string Abilities = "m_combatMoves";
        public const string Props = "m_characterProps";
        public const string UnitBases = "m_unitBases";
        public const string FactionIcons = "m_factionIconIds";
        public const string VoiceBundles = "m_voiceBundles";
        public const string CampaignLevels = "m_campaignLevels";
        public const string Campaigns = "m_campaigns";
        public const string Units = "m_unitBlueprints";
        public const string Factions = "m_factions";
        public const string DefaultHotbarFactions = "m_defaultHotbarFactionIds";
    }

    [NotNull]
    public ModContentAdder AddProjectilesToDb([NotNull, ItemNotNull] IList<GameObject> projectiles)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Projectiles, 
            gameObjects: projectiles, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<ProjectileEntity>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddWeaponsToDb([NotNull, ItemNotNull] IList<GameObject> weapons)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Weapons, 
            gameObjects: weapons, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<WeaponItem>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddAbilitiesToDb([NotNull, ItemNotNull] IList<GameObject> abilities)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Abilities, 
            gameObjects: abilities, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<SpecialAbility>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddPropsToDb([NotNull, ItemNotNull] IList<GameObject> props)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Props, 
            gameObjects: props, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<PropItem>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddUnitBasesToDb([NotNull, ItemNotNull] IList<GameObject> unitBases)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.UnitBases, 
            gameObjects: unitBases, 
            getDatabaseIdFromGameObjectFn: (x) => x.GetComponent<Unit>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddFactionIconsToDb([NotNull, ItemNotNull] IList<FactionIcon> factionIcons)
    {
        return AddGameObjectsToDbList(
            landfallContentDatabaseFieldName: FieldNames.FactionIcons, 
            gameObjects: factionIcons,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddVoiceBundlesToDb([NotNull, ItemNotNull] IList<VoiceBundle> voiceBundles)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.VoiceBundles, 
            gameObjects: voiceBundles,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddCampaignLevelsToDb([NotNull, ItemNotNull] IList<TABSCampaignLevelAsset> campaignLevels)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.CampaignLevels, 
            gameObjects: campaignLevels,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddCampaignsToDb([NotNull, ItemNotNull] IList<TABSCampaignAsset> campaigns)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Campaigns, 
            gameObjects: campaigns, 
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddUnitsToDb([NotNull, ItemNotNull] IList<UnitBlueprint> units)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Units, 
            gameObjects: units,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddFactionsToDb([NotNull, ItemNotNull] IList<Faction> factions)
    {
        AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FieldNames.Factions, 
            gameObjects: factions, 
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);

        AddFactionsToDefaultHotbarFactions(factions);

        return this;
    }

    private void AddFactionsToDefaultHotbarFactions([NotNull, ItemNotNull] IList<Faction> factions)
    {
        var factionsField = GetFieldFromLandfallContentDatabase(FieldNames.Factions);
        var currentFactions = (Dictionary<DatabaseID, Faction>) factionsField.GetValue(landfallContentDatabase);

        var defaultHotbarFactionIdsField = GetFieldFromLandfallContentDatabase(FieldNames.DefaultHotbarFactions);
        var defaultHotbarFactionIds = (List<DatabaseID>) defaultHotbarFactionIdsField.GetValue(landfallContentDatabase);

        foreach (var faction in factions)
        {
            var guid = faction.Entity.GUID;
            if (currentFactions.ContainsKey(guid)) continue;

            defaultHotbarFactionIds.Add(guid);
            nonStreamableAssets.Add(guid, faction);
        }
        
        var defaultHotbarFactionIdsOrderedAscendingly = defaultHotbarFactionIds.OrderBy(x => currentFactions[x].index).ToList();
        defaultHotbarFactionIdsField.SetValue(landfallContentDatabase,
            value: defaultHotbarFactionIdsOrderedAscendingly);
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