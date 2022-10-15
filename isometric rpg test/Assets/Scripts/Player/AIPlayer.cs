using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class AIPlayer : Player
{
	private System.Random random;
	private TileGrid tileGrid;

	public override void Play(TileGrid grid)
	{
		grid.GridState = new TileGridStateAITurn(tileGrid, this);
		random = new System.Random();
		tileGrid = grid;

		StartCoroutine(Play());

		//Coroutine is necessary to allow Unity to run updates on other objects (like UI).
	}

	public IEnumerator Play()
	{
		tileGrid.GridState = new TileGridStateBlockInput(tileGrid);
		var myUnits = tileGrid.GetCurrentPlayerUnits();

		foreach (Unit unit in myUnits)
		{
			var moveAbility = unit.GetComponent<MoveAbility>();
			var attackAbility = unit.GetComponent<AttackAbility>();

			if (GetUnitToAttack(unit))
            {
				attackAbility.OnAbilitySelected(tileGrid);
				attackAbility.UnitToAttack = GetUnitToAttack(unit);
				StartCoroutine(unit.GetComponent<AttackAbility>().AIExecute(tileGrid));
				yield return new WaitForSeconds(1f);
				continue;
			}
			else if(GetDestination(unit))
            {
				moveAbility.OnAbilitySelected(tileGrid);
				moveAbility.Destination = GetDestination(unit);
				StartCoroutine(unit.GetComponent<MoveAbility>().AIExecute(tileGrid));
				while (unit.IsMoving)
					yield return 0;

				if(GetUnitToAttack(unit))
                {
					attackAbility.OnAbilitySelected(tileGrid);
					attackAbility.UnitToAttack = GetUnitToAttack(unit);
					StartCoroutine(unit.GetComponent<AttackAbility>().AIExecute(tileGrid));
					yield return new WaitForSeconds(1f);
					continue;
				}				
			}
		}
		tileGrid.EndTurn();
	}

	Unit GetUnitToAttack(Unit unit)
    {
		var enemyUnits = tileGrid.GetEnemyUnits(this);
		var unitsInRange = enemyUnits.Where(e => unit.IsUnitAttackable(e)).ToList();

		if (unitsInRange.Count != 0)
		{
			var index = random.Next(0, unitsInRange.Count);
			return unitsInRange[index];
		}
		return null; 
	}

	OverlayTile GetDestination(Unit unit)
    {
		List<OverlayTile> potentialDestinations = new List<OverlayTile>();
		//Order enemies units by how close they are to the unit
		var enemyUnits = tileGrid.GetEnemyUnits(this).OrderByDescending(e => tileGrid.GetManhattenDistance(e.Tile, unit.Tile)).ToList();

		//Find potential tiles that a unit can to move and attack an enemy from
		foreach (var enemyUnit in enemyUnits)
		{
			potentialDestinations.AddRange(tileGrid.TileList.FindAll(t => unit.IsTileMovableTo(t) && unit.IsUnitAttackable(enemyUnit)));		
		}

		//Find all available destinations
		var availableDestinations = unit.GetAvailableDestinations(tileGrid);

		//if there are no tiles that a unit can move to and attack an enemy from, then add a random tile from the list of tiles that are within move range
		if (potentialDestinations.Count == 0 && availableDestinations.Count != 0)
		{
			potentialDestinations.AddRange(availableDestinations);
		}
		else if(availableDestinations.Count == 0)
        {
			return unit.Tile;
        }

		//Get closest enemy from the unit, which we'll use to find the best movement path 
		var closestEnemy = enemyUnits.First();

		//order potential destinations by how close they are to the closest enemy
		potentialDestinations = potentialDestinations.OrderByDescending(t => tileGrid.GetManhattenDistance(t, closestEnemy.Tile)).ToList();

		OverlayTile bestDestination = null;

		foreach (var potentialDestination in potentialDestinations)
		{ 
			var distanceFromEnemy = tileGrid.GetManhattenDistance(potentialDestination, closestEnemy.Tile);

			//if there is no path
			if ((bestDestination == null) ||
				//if the current destination is closer to the enemy than the best destination
				bestDestination != null && distanceFromEnemy < tileGrid.GetManhattenDistance(bestDestination, closestEnemy.Tile))
				bestDestination = potentialDestination;		
		}

		return bestDestination;

	}
}

    