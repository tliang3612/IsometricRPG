using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum WeaponType
{
	SWORD,
	LANCE,
	AXE,
	BOW,
	STAFF,
	TOME		
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Weapon", order = 2)]
public class Weapon : ScriptableObject
{
	public string Name;
	public Sprite WeaponSprite;

	public int Attack, Hit, Crit, Weight;
	public int Range;

	public WeaponType Type;

	public GameObject HitEffect;
	public GameObject CritEffect;

	public int GetEffectiveness(WeaponType other)
	{
		switch (Type)
		{
			case WeaponType.SWORD:
				if (other == WeaponType.LANCE) return -1;
				if (other == WeaponType.AXE) return 1;
				return 0;
			case WeaponType.LANCE:
				if (other == WeaponType.AXE) return -1;
				if (other == WeaponType.SWORD) return 1;
				return 0;
			case WeaponType.AXE:
				if (other == WeaponType.SWORD) return -1;
				if (other == WeaponType.LANCE) return 1;
				return 0;
			default:
				return 0;
		}
	}

}