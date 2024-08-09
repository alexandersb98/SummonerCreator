using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DM;
using JetBrains.Annotations;
using Landfall.TABS;
using Landfall.TABS.UnitEditor;
using Landfall.TABS.Workshop;
using UnityEngine;

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
        var field = GetFieldFromLandfallContentDatabase("m_projectiles");
        var projectiles = (Dictionary<DatabaseID, GameObject>)field.GetValue(landfallContentDatabase);

        foreach (var proj in newProjectiles)
        {
            var guid = proj.GetComponent<ProjectileEntity>().Entity.GUID;
            if (projectiles.ContainsKey(guid)) continue;

            projectiles.Add(guid, proj);
            nonStreamableAssets.Add(guid, proj);
        }

        field.SetValue(landfallContentDatabase, projectiles);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewWeaponsToDb([NotNull, ItemNotNull] IList<GameObject> newWeapons)
    {
        var field = GetFieldFromLandfallContentDatabase("m_weapons");
        var weapons = (Dictionary<DatabaseID, GameObject>)field.GetValue(landfallContentDatabase);

        foreach (var weapon in newWeapons)
        {
            var guid = weapon.GetComponent<WeaponItem>().Entity.GUID;
            if (weapons.ContainsKey(guid)) continue;

            weapons.Add(guid, weapon);
            nonStreamableAssets.Add(guid, weapon);
        }

        field.SetValue(landfallContentDatabase, weapons);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewAbilitiesToDb([NotNull, ItemNotNull] IList<GameObject> newAbilities)
    {
        var field = GetFieldFromLandfallContentDatabase("m_combatMoves");
        var abilities = (Dictionary<DatabaseID, GameObject>)field.GetValue(landfallContentDatabase);

        foreach (var ability in newAbilities)
        {
            var guid = ability.GetComponent<SpecialAbility>().Entity.GUID;
            if (abilities.ContainsKey(guid)) continue;

            abilities.Add(guid, ability);
            nonStreamableAssets.Add(guid, ability);
        }

        field.SetValue(landfallContentDatabase, abilities);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewPropsToDb([NotNull, ItemNotNull] IList<GameObject> newProps)
    {
        var field = GetFieldFromLandfallContentDatabase("m_characterProps");
        var props = (Dictionary<DatabaseID, GameObject>)field.GetValue(landfallContentDatabase);

        foreach (var prop in newProps)
        {
            var guid = prop.GetComponent<PropItem>().Entity.GUID;
            if (props.ContainsKey(guid)) continue;

            props.Add(guid, prop);
            nonStreamableAssets.Add(guid, prop);
        }

        field.SetValue(landfallContentDatabase, props);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewUnitBasesToDb([NotNull, ItemNotNull] IList<GameObject> newUnitBases)
    {
        var field = GetFieldFromLandfallContentDatabase("m_unitBases");
        var unitBases = (Dictionary<DatabaseID, GameObject>)field.GetValue(landfallContentDatabase);

        foreach (var unitBase in newUnitBases)
        {
            var guid = unitBase.GetComponent<Unit>().Entity.GUID;
            if (unitBases.ContainsKey(guid)) continue;

            unitBases.Add(guid, unitBase);
            nonStreamableAssets.Add(guid, unitBase);
        }

        field.SetValue(landfallContentDatabase, unitBases);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewFactionIconsToDb([NotNull, ItemNotNull] IList<FactionIcon> newFactionIcons)
    {
        var field = GetFieldFromLandfallContentDatabase("m_factionIconIds");
        var factionIcons = (List<DatabaseID>)field.GetValue(landfallContentDatabase);

        foreach (var factionIcon in newFactionIcons)
        {
            var guid = factionIcon.Entity.GUID;
            if (factionIcons.Contains(guid)) continue;

            factionIcons.Add(guid);
            nonStreamableAssets.Add(guid, factionIcon);
        }

        field.SetValue(landfallContentDatabase, factionIcons);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewVoiceBundlesToDb([NotNull, ItemNotNull] IList<VoiceBundle> newVoiceBundles)
    {
        var field = GetFieldFromLandfallContentDatabase("m_voiceBundles");
        var voiceBundles = (Dictionary<DatabaseID, VoiceBundle>)field.GetValue(landfallContentDatabase);

        foreach (var voiceBundle in newVoiceBundles)
        {
            var guid = voiceBundle.Entity.GUID;
            if (voiceBundles.ContainsKey(guid)) continue;

            voiceBundles.Add(guid, voiceBundle);
            nonStreamableAssets.Add(guid, voiceBundle);
        }

        field.SetValue(landfallContentDatabase, voiceBundles);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewCampaignLevelsToDb([NotNull, ItemNotNull] IList<TABSCampaignLevelAsset> newCampaignLevels)
    {
        var field = GetFieldFromLandfallContentDatabase("m_campaignLevels");
        var campaignLevels = (Dictionary<DatabaseID, TABSCampaignLevelAsset>)field.GetValue(landfallContentDatabase);

        foreach (var campaignLevel in newCampaignLevels)
        {
            var guid = campaignLevel.Entity.GUID;
            if (campaignLevels.ContainsKey(guid)) continue;

            campaignLevels.Add(guid, campaignLevel);
            nonStreamableAssets.Add(guid, campaignLevel);
        }

        field.SetValue(landfallContentDatabase, campaignLevels);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewCampaignsToDb([NotNull, ItemNotNull] IList<TABSCampaignAsset> newCampaigns)
    {
        var field = GetFieldFromLandfallContentDatabase("m_campaigns");
        var campaigns = (Dictionary<DatabaseID, TABSCampaignAsset>)field.GetValue(landfallContentDatabase);

        foreach (var campaign in newCampaigns)
        {
            var guid = campaign.Entity.GUID;
            if (campaigns.ContainsKey(guid)) continue;

            campaigns.Add(guid, campaign);
            nonStreamableAssets.Add(guid, campaign);
        }

        field.SetValue(landfallContentDatabase, campaigns);
        return this;
    }

    [NotNull]
    public ModContentAdder AddNewFactionsToDb([NotNull, ItemNotNull] IList<Faction> newFactions)
    {
        var factionsField = GetFieldFromLandfallContentDatabase("m_factions");
        var factions = (Dictionary<DatabaseID, Faction>)factionsField.GetValue(landfallContentDatabase);

        var defaultHotbarFactionIdsField = GetFieldFromLandfallContentDatabase("m_defaultHotbarFactionIds");
        var defaultHotbarFactionIds = (List<DatabaseID>)defaultHotbarFactionIdsField.GetValue(landfallContentDatabase);

        foreach (var faction in newFactions)
        {
            var guid = faction.Entity.GUID;
            if (factions.ContainsKey(guid)) continue;

            factions.Add(guid, faction);
            nonStreamableAssets.Add(guid, faction);
            defaultHotbarFactionIds.Add(guid);
        }

        factionsField.SetValue(landfallContentDatabase, factions);
        defaultHotbarFactionIdsField.SetValue(landfallContentDatabase,
            defaultHotbarFactionIds.OrderBy(x => factions[x].index).ToList());

        return this;
    }

    public ModContentAdder AddNewUnitsToDb([NotNull, ItemNotNull] IList<UnitBlueprint> newUnits)
    {
        var field = GetFieldFromLandfallContentDatabase("m_unitBlueprints");
        var units = (Dictionary<DatabaseID, UnitBlueprint>)field.GetValue(landfallContentDatabase);

        foreach (var unit in newUnits)
        {
            var guid = unit.Entity.GUID;
            if (units.ContainsKey(guid)) continue;

            units.Add(guid, unit);
            nonStreamableAssets.Add(guid, unit);
        }

        field.SetValue(landfallContentDatabase, units);
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