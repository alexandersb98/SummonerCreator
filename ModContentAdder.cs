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

    private const string ProjectilesFieldName = "m_projectiles";
    private const string WeaponsFieldName = "m_weapons";
    private const string AbilitiesFieldName = "m_combatMoves";
    private const string PropsFieldName = "m_characterProps";
    private const string UnitBasesFieldName = "m_unitBases";
    private const string FactionIconsFieldName = "m_factionIconIds";
    private const string VoiceBundlesFieldName = "m_voiceBundles";
    private const string CampaignLevelsFieldName = "m_campaignLevels";
    private const string CampaignsFieldName = "m_campaigns";
    private const string UnitsFieldName = "m_unitBlueprints";
    private const string FactionsFieldName = "m_factions";
    private const string DefaultHotbarFactionsFieldName = "m_defaultHotbarFactionIds";

    [NotNull]
    public ModContentAdder AddProjectilesToDb([NotNull, ItemNotNull] IList<GameObject> projectiles)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: ProjectilesFieldName, 
            gameObjects: projectiles, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<ProjectileEntity>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddWeaponsToDb([NotNull, ItemNotNull] IList<GameObject> weapons)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: WeaponsFieldName, 
            gameObjects: weapons, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<WeaponItem>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddAbilitiesToDb([NotNull, ItemNotNull] IList<GameObject> abilities)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: AbilitiesFieldName, 
            gameObjects: abilities, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<SpecialAbility>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddPropsToDb([NotNull, ItemNotNull] IList<GameObject> props)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: PropsFieldName, 
            gameObjects: props, 
            getDatabaseIdFromGameObjectFn: x => x.GetComponent<PropItem>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddUnitBasesToDb([NotNull, ItemNotNull] IList<GameObject> unitBases)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: UnitBasesFieldName, 
            gameObjects: unitBases, 
            getDatabaseIdFromGameObjectFn: (x) => x.GetComponent<Unit>().Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddFactionIconsToDb([NotNull, ItemNotNull] IList<FactionIcon> factionIcons)
    {
        return AddGameObjectsToDbList(
            landfallContentDatabaseFieldName: FactionIconsFieldName, 
            gameObjects: factionIcons,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddVoiceBundlesToDb([NotNull, ItemNotNull] IList<VoiceBundle> voiceBundles)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: VoiceBundlesFieldName, 
            gameObjects: voiceBundles,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddCampaignLevelsToDb([NotNull, ItemNotNull] IList<TABSCampaignLevelAsset> campaignLevels)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: CampaignLevelsFieldName, 
            gameObjects: campaignLevels,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddCampaignsToDb([NotNull, ItemNotNull] IList<TABSCampaignAsset> campaigns)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: CampaignsFieldName, 
            gameObjects: campaigns, 
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddUnitsToDb([NotNull, ItemNotNull] IList<UnitBlueprint> units)
    {
        return AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: UnitsFieldName, 
            gameObjects: units,
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);
    }

    [NotNull]
    public ModContentAdder AddFactionsToDb([NotNull, ItemNotNull] IList<Faction> factions)
    {
        AddGameObjectsToDbDictionary(
            landfallContentDatabaseFieldName: FactionsFieldName, 
            gameObjects: factions, 
            getDatabaseIdFromGameObjectFn: x => x.Entity.GUID);

        AddFactionsToDefaultHotbarFactions(factions);

        return this;
    }

    private void AddFactionsToDefaultHotbarFactions([NotNull, ItemNotNull] IList<Faction> factions)
    {
        var factionsField = GetFieldFromLandfallContentDatabase(FactionsFieldName);
        var currentFactions = (Dictionary<DatabaseID, Faction>) factionsField.GetValue(landfallContentDatabase);

        var defaultHotbarFactionIdsField = GetFieldFromLandfallContentDatabase(DefaultHotbarFactionsFieldName);
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