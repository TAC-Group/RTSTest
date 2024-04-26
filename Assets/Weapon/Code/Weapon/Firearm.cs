using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tac.HealthSystem;
using RTSToolkitFree;
using static RTSToolkitFree.BattleSystem;

/// <summary>
/// Огнестрельное оружие
/// </summary>
public class Firearm : Weapon
{
	/// <summary>
	/// Вид оружия
	/// </summary>
	public WeaponFireType Type;

	/// <summary>
	/// Количество патрон в очереде (только при возможности автоматической стрельбы очередью)
	/// </summary>
	public int BurstFireCount = 3;

	/// <summary>
	/// Минимальное рассеивание в см. (точность стрельбы, зависит от расстояния)
	/// </summary>
	public float MinDispersion = 3;

	/// <summary>
	/// Максимальное рассеивание в см. (точность стрельбы, зависит от расстояния) 
	/// </summary>
	public float MaxDispersion = 30;

	/// <summary>
	/// Префаб пули/снаряда
	/// </summary>
	public GameObject Bullet;
	/// <summary>
	/// Звук попадания
	/// </summary>
	public List<AudioClip> ImpactSounds;
	/// <summary>
	/// Звук промаха
	/// </summary>
	public List<AudioClip> MissSounds;

	/// <summary>
	/// Список стволов оружия
	/// </summary>
	public List<GunBarrel> GunBarrels = new List<GunBarrel>();

	/// <summary>
	/// Первый ствол (часто он единственный)
	/// </summary>
	public GunBarrel SingleBarrel { get { return GunBarrels[0]; } }


	private TacShootSim.Shoot ShootLib = new TacShootSim.Shoot();


	private void Start()
	{
		foreach (var gun in GunBarrels)
		{
			gun.Init(Bullet, ImpactSounds, MissSounds);
		}
	}

	public override void Attack(GameObject argAgent, GameObject argEnemy)
	{
		RotateToTarget(argAgent, argEnemy);

		IAgentState agentState = argAgent.GetComponent<IAgentState>();
		IAgentState enemyState = argEnemy.GetComponent<IAgentState>();
		if (agentState != null && enemyState != null)
		{
			float locDistance = GetDistance(argAgent, argEnemy);

			// Стреляет только тогда, когда расстояние до объекта меньше максимальной до которой стреляет оружие
			if (locDistance < MaxDistance)
			{
				float locRealDispersion = GetDispersion(argAgent, argEnemy);

				if (AllowSingleShot() && SingleBarrel.AllowShoot == true)
				{
					SingleShot(agentState, enemyState, locDistance, locRealDispersion);
				}
				else if (AllowBurstFire() && SingleBarrel.AllowShoot == true)
				{
					BurstFire(agentState, enemyState, locDistance, locRealDispersion);
				}
			}
		}
	}

	/// <summary>
	/// Логический расчет попадания (без поддержки отображения и применения к стволу)
	/// </summary>
	private bool Fire(IAgentState agentState, IAgentState enemyState, float argDistance, float argRealDispersion)
	{
		bool retIsHit = false;
		float locPrecision = agentState.Precision.ComplexState;
		(BodyParts locBodyPart, float locBodyPartSize) = GetBodyPartSize();
		float locHitPrecision = ShootLib.GetHitPrecision(locPrecision, argRealDispersion, locBodyPartSize);

		if (locHitPrecision != 0) // попал
		{
			retIsHit = true;
			float locDamage = MinDamage;
			locDamage += (MaxDamage - MinDamage) * locHitPrecision;

			enemyState.ApplyDamage(locBodyPart, locDamage);

			bool locIsLearning = IsLearningDistance(argDistance, locPrecision);
			if (locIsLearning == true)
			{
				agentState.Precision.State += locHitPrecision / 10f;
			}
		}
		return retIsHit;
	}


	public void SingleShot(IAgentState agentState, IAgentState enemyState, float argDistance, float argRealDispersion)
	{
		foreach (var gun in GunBarrels)
		{
			bool isHit = Fire(agentState, enemyState, argDistance, argRealDispersion);
			StartCoroutine(gun.SingleFire(AudioSource, isHit));
		}
	}

	public void BurstFire(IAgentState agentState, IAgentState enemyState, float argDistance, float argRealDispersion)
	{
		List<bool> isHit = new List<bool>();
		foreach (var gun in GunBarrels)
		{
			for (int i = 0; i < BurstFireCount; i++)
			{
				bool tmpHit = Fire(agentState, enemyState, argDistance, argRealDispersion);
				isHit.Add(tmpHit);
			}
			StartCoroutine(gun.BurstFire(AudioSource, isHit));
		}
	}

	public void PrecisionShot(GameObject argAgent, GameObject argEnemy)
	{
		//todo

		//SingleShot();
	}

	public bool AllowSingleShot()
	{
		bool ret = true;
		switch (Type)
		{
			case WeaponFireType.MP5:
			case WeaponFireType.AKSU:
			case WeaponFireType.UZI:
				ret = false;
				break;
		}
		return ret;
	}

	public bool AllowBurstFire()
	{
		bool ret = false;
		switch (Type)
		{
			case WeaponFireType.MP5:
			case WeaponFireType.AKSU:
			case WeaponFireType.UZI:

			case WeaponFireType.AK47:
			case WeaponFireType.M14:
			case WeaponFireType.FAMAS:
			case WeaponFireType.FNFAL:
				ret = true;
				break;
		}
		return ret;
	}

	public bool AllowPrecisionShot()
	{
		bool ret = false;
		switch (Type)
		{
			case WeaponFireType.Gewehr98:

			case WeaponFireType.AK47:
			case WeaponFireType.M14:
			case WeaponFireType.FAMAS:
			case WeaponFireType.FNFAL:
				ret = true;
				break;
		}
		return ret;
	}


	private void RotateToTarget(GameObject argAgent, GameObject argEnemy)
	{
		Vector3 lookPosition = argEnemy.transform.position - argAgent.transform.position;
		lookPosition.y = 0;
		argAgent.transform.rotation = Quaternion.LookRotation(lookPosition);
	}


	private (BodyParts part, float size) GetBodyPartSize()
	{
		BodyParts bodyPart = (BodyParts)World.rnd.Next(1, 11);
		return (bodyPart, GetBodyPartSize(bodyPart));
	}

	private float GetBodyPartSize(BodyParts argPart)
	{
		float locBodyPartSize = 0;
		switch (argPart)
		{
			case BodyParts.Head:
				locBodyPartSize = 18;
				break;
			case BodyParts.Thorax:
				locBodyPartSize = 40;
				break;
			case BodyParts.Abdomen:
				locBodyPartSize = 30;
				break;
			case BodyParts.ShoulderLeft:
			case BodyParts.ShoulderRight:
			case BodyParts.ThighLeft:
			case BodyParts.ThighRight:
				locBodyPartSize = 15;
				break;
			case BodyParts.ForearmLeft:
			case BodyParts.ForearmRight:
			case BodyParts.ShinLeft:
			case BodyParts.ShinRight:
				locBodyPartSize = 10;
				break;
		}
		return locBodyPartSize;
	}

	/// <summary>
	/// Получить расстояние между агентом и его врагом
	/// </summary>
	/// <param name="argAgent">агент</param>
	/// <param name="argEnemy">враг</param>
	/// <returns></returns>
	public float GetDistance(GameObject argAgent, GameObject argEnemy)
	{
		return Vector3.Distance(argAgent.transform.position, argEnemy.transform.position);
	}


	public float GetDispersion(GameObject argAgent, GameObject argEnemy)
	{
		float locDistance = GetDistance(argAgent, argEnemy);

		float locRealDispersion = MinDispersion;
		// Если расстояние превышает эффективную для оружия дистанцию, то рассеяность увеличивается, иначе минимальная
		if (locDistance > WorkDistance)
		{
			float locDistanceNorm = locDistance / (MaxDistance - WorkDistance);
			locRealDispersion += (MaxDispersion - MinDispersion) * locDistanceNorm;
		}

		return locRealDispersion;
	}


	/// <summary>
	/// Является ли дистанция пригодной для обучения
	/// </summary>
	/// <returns>true - да, false - нет</returns>
	public bool IsLearningDistance(GameObject argAgent, GameObject argEnemy)
	{
		bool retIsLearning = false;
		IAgentState agentState = argAgent.GetComponent<IAgentState>();
		if (agentState != null)
		{
			float locDistance = GetDistance(argAgent, argEnemy);
			retIsLearning = IsLearningDistance(locDistance, agentState.Precision.State);
		}
		return retIsLearning;
	}

	public bool IsLearningDistance(float argDistance, float argAgentPrecision)
	{
		return ShootLib.IsLearningDistance(argDistance, argAgentPrecision, WorkDistance, MaxDistance);
	}



	/// <summary>
	/// Получить вероятность попадания агентом в цель
	/// </summary>
	public float GetProbability(GameObject argAgent, GameObject argEnemy)
	{
		float retHitProbability = 0;
		IAgentState agentState = argAgent.GetComponent<IAgentState>();
		if (agentState != null)
		{
			float locDistance = GetDistance(argAgent, argEnemy);

			// Стреляет только тогда, когда расстояние до объекта меньше максимальной до которой стреляет оружие
			if (locDistance < MaxDistance)
			{
				float locRealDispersion = GetDispersion(argAgent, argEnemy);
				float locBodyPartSize = GetBodyPartSize().size;

				float locPrecision = agentState.Precision.ComplexState;
				retHitProbability = ShootLib.GetHitProbability(locPrecision, locRealDispersion, locBodyPartSize);
			}
		}
		return retHitProbability;
	}


}

