using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIPlayer : Player
{
	Unit currentUnit;

	List<Unit> unitList;

	public AIPlanOfAttack Evaluate(TileGrid tileGrid, Unit unit)
	{

		currentUnit = unit;
		AIPlanOfAttack poa = new AIPlanOfAttack();

		DefaultAttackPattern(poa, tileGrid);

		PlanPosition(poa, tileGrid);
		
		if (poa.ability == null)
        {
			StartCoroutine(MoveUnit(poa, tileGrid));
		}
			

		return poa;
	}

    public override void Play(TileGrid tileGrid)
    {
		tileGrid.GridState = new TileGridStateBlockInput(tileGrid);
		var myUnits = tileGrid.GetCurrentPlayerUnits();
		foreach(Unit unit in myUnits)
        {
			Evaluate(tileGrid, unit);
        }
		
	}



    void DefaultAttackPattern(AIPlanOfAttack poa, TileGrid tileGrid)
	{		
		poa.ability = GetComponentInChildren<Ability>();
	}

	

	void PlanPosition(AIPlanOfAttack poa, TileGrid tileGrid)
	{
		List<OverlayTile> moveOptions = GetMoveOptions(poa, tileGrid);
		OverlayTile tile = moveOptions[Random.Range(0, moveOptions.Count)];
		poa.moveLocation = poa.fireLocation = tile.gridLocation2D;

	}

	IEnumerator MoveUnit(AIPlanOfAttack poa, TileGrid tileGrid)
	{
		List<OverlayTile> potentialDestinations = GetMoveOptions(poa, tileGrid);

		var nearestEnemy = FindNearestEnemy(tileGrid);
		if (nearestEnemy != null)
		{
			OverlayTile destinationTile = nearestEnemy.Tile;

			currentUnit.GetComponent<MoveAbility>().Destination = destinationTile;
			StartCoroutine(currentUnit.GetComponent<MoveAbility>().Act(tileGrid));
			while (currentUnit.IsMoving)
				yield return 0;
		}

		poa.moveLocation = currentUnit.Tile.gridLocation2D;
	}

	Unit FindNearestEnemy(TileGrid tileGrid)
    {
		var enemyUnits = tileGrid.GetEnemyUnits(this);
		var enemiesInRange = new List<Unit>();
		Unit nearestEnemy;

		foreach(Unit enemy in enemyUnits)
        {
			if (currentUnit.IsUnitAttackable(enemy))
            {
				enemiesInRange.Add(enemy);
            }
        }
		//Finds nearest enemy by comparing their mahatten distances
		nearestEnemy = enemiesInRange.OrderByDescending(e => tileGrid.GetManhattenDistance(currentUnit.Tile, e.Tile)).FirstOrDefault();

		return nearestEnemy;
	}

	List<OverlayTile> GetMoveOptions(AIPlanOfAttack poa, TileGrid tileGrid)
	{
		var potentialDestinations = new List<OverlayTile>();
		var enemyUnits = tileGrid.GetEnemyUnits(this);

		foreach (var enemyUnit in enemyUnits)
		{
			potentialDestinations.AddRange(tileGrid.TileList.FindAll(t => currentUnit.IsTileMovableTo(t) && currentUnit.IsUnitAttackable(enemyUnit)));
		}

		return potentialDestinations;
	}


}