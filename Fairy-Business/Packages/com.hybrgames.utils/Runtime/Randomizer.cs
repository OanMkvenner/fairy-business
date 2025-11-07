// squirrel-based randomizer with static state
using System;
using UnityEngine;

public class RandomizerUtil
{
	static public RandomizerUtil global = new();
	int  rndState = 0;
	uint seed = 0;
	public RandomizerUtil(uint initialSeed = 0, int initialRndState = 0){
		SetSeed(initialSeed);
		rndState = initialRndState;
	}
	public void SetSeed(uint seedVal){
		seed = seedVal;
	}
	public void UpdateRndState(){
		if (seed == 0) seed = (uint)DateTime.Now.Ticks;
		rndState++;
	}
	public uint RandomUint(uint maxValInclusive){
		UpdateRndState();
		if (maxValInclusive == 0) return 0;
		return RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1);
	}
	public uint RandomUint(int maxValInclusive){
		//UpdateStaticSeed(); already done in called function
		if (maxValInclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return RandomUint((uint)maxValInclusive);
	}
	public int RandomInt(uint maxValInclusive){
		UpdateRndState();
		if (maxValInclusive == 0) return 0;
		return (int)(RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1));
	}
	public int RandomInt(int maxValInclusive){
		//UpdateStaticSeed(); already done in called function
		if (maxValInclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return (int)RandomUint((uint)maxValInclusive);
	}
	public float RandomFloat(float maxValInclusive = 1.0f){
		UpdateRndState();
		return maxValInclusive * RandomSquirrel.Get1dNoiseZeroToOne(rndState, seed);
	}
	public float RandomFloatRange(float minValInclusive, float maxValInclusive){
		if (maxValInclusive < minValInclusive) return minValInclusive;
		maxValInclusive -= minValInclusive;
		if (maxValInclusive == 0) return minValInclusive + 0;
		return minValInclusive + RandomFloat(maxValInclusive);
	}
	public int RandomIntRange(int minValInclusive, int maxValInclusive){
		if (maxValInclusive < minValInclusive) return minValInclusive;
		UpdateRndState();
		maxValInclusive -= minValInclusive;
		if (maxValInclusive == 0) return minValInclusive + 0;
		return minValInclusive + (int)(RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1));
	}
	public uint RandomUintThreadsafe(uint maxValInclusive, int positionX, uint seed){
		return RandomSquirrel.Get1dNoiseUint(positionX, seed) % (maxValInclusive + 1);
	}
	/// <summary>
	/// This turns a float into an int, but any float fractions are treated as a chance to round up.
	/// This means e.g. 1,4f has a 60%chance to return as 1 and a 40% chance to return 2
	/// </summary>
	/// <returns></returns>
	public int RoundFloatToIntByChance(float rawValue){
		//UpdateStaticSeed(); already done in called function
		int result = Mathf.FloorToInt(rawValue);
		result += RandomChancInt(rawValue - (float)result);
		return result;
	}
	public bool RandomChanceBool(float chance = 1.0f){
		//UpdateStaticSeed(); already done in called function
		if (chance == 0f) return false;
		if (RandomFloat() <= chance) return true;
		else return false;
	}
	// returns "1" on success and "0" on miss
	public int RandomChancInt(float chance = 1.0f){
		//UpdateStaticSeed(); already done in called function
		if (chance == 0f) return 0;
		if (RandomFloat() <= chance) return 1;
		else return 0;
	}
	// returns "1" on success and "0" on miss
	public float RandomChancFloat(float chance = 1.0f){
		//UpdateStaticSeed(); already done in called function
		if (chance == 0f) return 0f;
		if (RandomFloat() <= chance) return 1f;
		else return 0f;
	}
	
	// perlin noise on 2d plane, return values transformed to 0-1 value range
	public float PerlinNoise_0_to_1(float x, float y, int seed = 0){
		var val = Icaria.Engine.Procedural.IcariaNoise.GradientNoise(x, y, seed);
		val = (val + 1.0f) / 2.0f;
		return val;
	}
}
