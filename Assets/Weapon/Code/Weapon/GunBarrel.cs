using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������� �����
/// </summary>
[Serializable]
public class GunBarrel
{
	/// <summary>
	/// ����� ��������
	/// </summary>
	public Transform ShootPoint;

	// ���� ��������
	public float Force = 100f;

	// �������� ����� ����������/���������
	public float SingleDelay = 1f;

	// �������� ����� ���������� � �������
	public float BurstDelay = 0.1f;

	public bool AllowShoot = true;

	/// <summary>
	/// ������ ����/�������
	/// </summary>
	private GameObject Bullet;
	/// <summary>
	/// ���� ���������
	/// </summary>
	private List<AudioClip> ImpactSounds;
	/// <summary>
	/// ���� �������
	/// </summary>
	private List<AudioClip> MissSounds;

	public void Init(GameObject argBullet, List<AudioClip> argImpactSounds, List<AudioClip> argMissSounds)
	{ 
		Bullet = argBullet;
		ImpactSounds = argImpactSounds;
		MissSounds = argMissSounds;
	}


	public IEnumerator SingleFire(AudioSource hitAudio, bool IsHit)
	{
		if (AllowShoot == true)
		{
			AllowShoot = false;
			Fire(hitAudio, IsHit);
			yield return new WaitForSeconds(SingleDelay);
			AllowShoot = true;
		}
	}

	public IEnumerator BurstFire(AudioSource hitAudio, List<bool> IsHit)
	{
		if (AllowShoot == true)
		{
			AllowShoot = false;
			for (int i = 0; i < IsHit.Count; i++)
			{
				Fire(hitAudio, IsHit[i]);
				yield return new WaitForSeconds(BurstDelay);
			}
			yield return new WaitForSeconds(SingleDelay);
			AllowShoot = true;
		}
	}

	private void Fire(AudioSource hitAudio, bool IsHit)
	{
		GameObject bullet = World.Create(Bullet, ShootPoint.position, ShootPoint.rotation);
		bullet.GetComponent<Rigidbody>().AddForce(ShootPoint.forward * Force);
		PlaySound(hitAudio, IsHit);
	}


	public void PlaySound(AudioSource hitAudio, bool IsHit)
	{
		if (hitAudio != null)
		{
			hitAudio.spatialBlend = 1.0f;
			hitAudio.minDistance = 1.0f;
			hitAudio.maxDistance = 100.0f;
			hitAudio.volume = 1.0f;
			hitAudio.pitch = 1.0f;

			if (IsHit == true)
			{
				hitAudio.clip = ImpactSounds[UnityEngine.Random.Range(0, ImpactSounds.Count)];
			}
			else
			{
				hitAudio.clip = MissSounds[UnityEngine.Random.Range(0, MissSounds.Count)]; ;
			}
			hitAudio.Play();
		}
	}


}
