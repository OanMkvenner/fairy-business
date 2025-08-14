// squirrel-based randomizer with static state
using System;
using UnityEngine;

public class RandomizerUtil
{
	int  rndState = 0;
	uint  seed = 0;
	public void SetSeed(uint  seedVal){
		seed = seedVal;
	}
	public uint RandomInt(uint maxValInclusive){
		rndState++;
		if (maxValInclusive == 0) return 0;
		return RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1);
	}
	static public int RandomIntSeeded(uint maxValInclusive, uint seed, int rndState = 0){
		rndState++;
		return (int)(RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1));
	}
}
public class RandomizerStatic
{
	static int staticRndState = 0;
	static uint staticSeed = 0;
	static public void SetStaticSeed(uint seedVal){
		staticSeed = seedVal;
	}
	static public uint RandomUint(uint maxValInclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		if (maxValInclusive == 0) return 0;
		return RandomSquirrel.Get1dNoiseUint(staticRndState, staticSeed) % (maxValInclusive + 1);
	}
	static public uint RandomUint(int maxValInclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		if (maxValInclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return RandomUint((uint)maxValInclusive);
	}
	static public int RandomInt(uint maxValInclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		if (maxValInclusive == 0) return 0;
		return (int)(RandomSquirrel.Get1dNoiseUint(staticRndState, staticSeed) % (maxValInclusive + 1));
	}
	static public int RandomInt(int maxValInclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		if (maxValInclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return (int)RandomUint((uint)maxValInclusive);
	}
	static public float RandomFloat(float maxValInclusive = 1.0f){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		return maxValInclusive * RandomSquirrel.Get1dNoiseZeroToOne(staticRndState, staticSeed);
	}
	static public int RandomIntRange(int minValInclusive, int maxValInclusive){
		if (maxValInclusive < minValInclusive) return minValInclusive;
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		maxValInclusive -= minValInclusive;
		if (maxValInclusive == 0) return minValInclusive + 0;
		return minValInclusive + (int)(RandomSquirrel.Get1dNoiseUint(staticRndState, staticSeed) % (maxValInclusive + 1));
	}
	static public uint RandomUintThreadsafe(uint maxValInclusive, int positionX, uint seed){
		return RandomSquirrel.Get1dNoiseUint(positionX, seed) % (maxValInclusive + 1);
	}
	static public bool RandomChanceBool(float chance = 1.0f){
		if (chance == 0f) return false;
		if (RandomFloat() <= chance) return true;
		else return false;
	}
	// returns "1" on success and "0" on miss
	static public int RandomChancInt(float chance = 1.0f){
		if (chance == 0f) return 0;
		if (RandomFloat() <= chance) return 1;
		else return 0;
	}
	// returns "1" on success and "0" on miss
	static public float RandomChancFloat(float chance = 1.0f){
		if (chance == 0f) return 0f;
		if (RandomFloat() <= chance) return 1f;
		else return 0f;
	}
	
	// perlin noise on 2d plane, return values transformed to 0-1 value range
	static public float PerlinNoise_0_to_1(float x, float y, int seed = 0){
		var val = Icaria.Engine.Procedural.IcariaNoise.GradientNoise(x, y, seed);
		val = (val + 1.0f) / 2.0f;
		return val;
	}
}