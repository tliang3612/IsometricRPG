using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DisplayAttackStatsAbility : DisplayAbility
{
    public event EventHandler<DisplayStatsChangedEventArgs> DisplayStatsChanged;

    private HashSet<OverlayTile> _tilesInAttackRange;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Display(TileGrid tileGrid)
    {
        foreach (var t in _tilesInAttackRange)
            t.MarkAsAttackableTile();

    }

    public override void OnUnitHighlighted(Unit unit, TileGrid tileGrid)
    {
        if(_tilesInAttackRange.Contains(unit.Tile))
            unit.Tile.HighlightedOnUnit();

        if (UnitReference.IsUnitAttackable(unit, true) && !UnitReference.Equals(unit))
        {
            //face the unit we're highlighting
            var direction = UnitReference.GetDirectionToFace(unit.Tile.transform.position);
            UnitReference.SetState(new UnitStateMoving(UnitReference, direction));

            var range = tileGrid.GetManhattenDistance(UnitReference.Tile, unit.Tile);
            var attackerStats = GetStats(UnitReference, unit, range);
            var defenderStats = GetStats(unit, UnitReference, range);

            if (DisplayStatsChanged != null)
                DisplayStatsChanged.Invoke(this, new DisplayStatsChangedEventArgs(attackerStats, defenderStats));
        }
    }

    public override void OnUnitDehighlighted(Unit unit, TileGrid tileGrid)
    {
        if (_tilesInAttackRange.Contains(unit.Tile))
            unit.Tile.DeHighlightedOnUnit();
    }

    //In case the player clicks on the unit's tile accidently instead of the unit
    public override void OnTileClicked(OverlayTile tile, TileGrid tileGrid)
    {
        if(tile.CurrentUnit)
            OnUnitClicked(tile.CurrentUnit, tileGrid);    
    }

    public override void OnUnitClicked(Unit unit, TileGrid tileGrid)
    {
        if (UnitReference.IsUnitAttackable(unit, true) && !UnitReference.Equals(unit))
        {
            UnitReference.GetComponentInChildren<AttackAbility>().UnitToAttack = unit;

            StartCoroutine(TransitionAbility(tileGrid, UnitReference.GetComponentInChildren<AttackAbility>()));
        }
    }

    public override void OnTileSelected(OverlayTile tile, TileGrid tileGrid)
    {
        if (_tilesInAttackRange.Contains(tile))
            tile.MarkAsHighlighted();
    }

    public override void OnTileDeselected(OverlayTile tile, TileGrid tileGrid)
    {
        if (_tilesInAttackRange.Contains(tile))
            tile.MarkAsDeHighlighted();
    }

    public override void OnAbilitySelected(TileGrid tileGrid)
    {
        base.OnAbilitySelected(tileGrid);

        _tilesInAttackRange = UnitReference.GetTilesInRange(tileGrid, UnitReference.EquippedWeapon.Range);
    }

    public override void OnRightClick(TileGrid tileGrid)
    {
        StartCoroutine(TransitionAbility(tileGrid, UnitReference.GetComponentInChildren<SelectWeaponToAttackAbility>()));
    }

    protected CombatStats GetStats(Unit unit, Unit unitToAttack, int range)
    {
        return new CombatStats(unit, unitToAttack, range);
    }
}


public class DisplayStatsChangedEventArgs : EventArgs
{
    public CombatStats AttackerStats;
    public CombatStats DefenderStats;

    public DisplayStatsChangedEventArgs(CombatStats attackerStats, CombatStats defenderStats)
    {
        AttackerStats = attackerStats;
        DefenderStats = defenderStats;
    }
}
