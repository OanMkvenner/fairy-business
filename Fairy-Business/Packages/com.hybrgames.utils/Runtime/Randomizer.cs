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
	public uint RandomInt(uint maxValExclusive){
		rndState++;
		return RandomSquirrel.Get1dNoiseUint(rndState, seed) % maxValExclusive;

	}

}
public class RandomizerStatic
{
	static int staticRndState = 0;
	static uint staticSeed = 0;
	static public void SetStaticSeed(uint  seedVal){
		staticSeed = seedVal;
	}
	static public uint RandomUint(uint maxValExclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		return RandomSquirrel.Get1dNoiseUint(staticRndState, staticSeed) % maxValExclusive;
	}
	static public uint RandomUint(int maxValExclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		if (maxValExclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return RandomUint((uint)maxValExclusive);
	}
	static public int RandomInt(uint maxValExclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		return (int)(RandomSquirrel.Get1dNoiseUint(staticRndState, staticSeed) % maxValExclusive);
	}
	static public int RandomInt(int maxValExclusive){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		if (maxValExclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return (int)RandomUint((uint)maxValExclusive);
	}
	static public float RandomFloat(float maxVal = 1.0f){
		if (staticSeed == 0) staticSeed = (uint)DateTime.Now.Ticks;
		staticRndState++;
		return maxVal * RandomSquirrel.Get1dNoiseZeroToOne(staticRndState, staticSeed);
	}
}