// squirrel-based randomizer with static state
using System;
using UnityEngine;

[Serializable]
public class RandomizerUtil
{
	static public RandomizerUtil global = new(initialGlobalRandomizer : true);
	int  rndState = 0;
	uint seed = 0;
	public RandomizerUtil(uint initialSeed = 0, int initialRndState = 0){
		SetSeed(initialSeed);
		rndState = initialRndState;
	}
	// without parameters it seeds randomly
	public RandomizerUtil(){
		rndState = 0;
		CheckGlobalRandomizerInitialized(); // this is relevant here, as im unsure in which order constructors are called during initialization
		RandomizeSeed();
	}
	// special Initializer for global RandomizerUtil
	private RandomizerUtil(bool initialGlobalRandomizer = true){
		rndState = 0;
		SetSeed((uint)System.Environment.TickCount);
		SetSeed(RandomUint());
	}
	static void CheckGlobalRandomizerInitialized(){
		if (global.seed == 0){
			global = new(initialGlobalRandomizer : true);
		}
	}
	// allows creating a new Randomizer from another one. 
	// This can be used to e.g. create one-shot randomizers that will be used for an unknown amount of times, but add a known amount of uses to the 
	// original Randomizer. This can be useful for repeatable save/load Randomization simulation
	public RandomizerUtil ForkRandomizer(){
		return new RandomizerUtil(RandomUint(), RandomInt());
	}
	public RandomizerUtil(RandomizerUtil randomizerToCopy){
		seed = randomizerToCopy.seed;
		rndState = randomizerToCopy.rndState;
	}
	public RandomizerUtil Copy(){
		return new RandomizerUtil(this);
	}
	// uses global Randomizer to randomize seed of the calling randomizer
	public void RandomizeSeed(){
		seed = global.RandomUint();
	}
	public void SetSeed(uint seedVal){
		seed = seedVal;
	}
	public void UpdateRndState(){
		rndState++;
	}
	public void SetRndState(int newRndState){
		rndState = newRndState;
	}
	// 0 to uint.MaxValue
	public uint RandomUint(){
		UpdateRndState();
		return RandomSquirrel.Get1dNoiseUint(rndState, seed);
	}
	// 0 to maxValInclusive
	public uint RandomUint(uint maxValInclusive){
		if (maxValInclusive == uint.MaxValue) return RandomUint();
		UpdateRndState();
		if (maxValInclusive == 0) return 0;
		return RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1);
	}
	// 0 to maxValInclusive
	public uint RandomUint(int maxValInclusive){
		//UpdateStaticSeed(); already done in called function below
		if (maxValInclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return RandomUint((uint)maxValInclusive);
	}
	// int.MinValue (negative) to int.MaxValue
	public int RandomInt(){
		UpdateRndState();
		uint val = RandomSquirrel.Get1dNoiseUint(rndState, seed);
		// this is an unsafe conversion, meaning high uint's become negative ints. But we dont care as the values are completely random aynway
		return System.Runtime.CompilerServices.Unsafe.As<uint, int>(ref val);
	}
	// 0 to maxValInclusive
	public int RandomInt(uint maxValInclusive){
		//UpdateStaticSeed(); already done in called function below
		return (int)RandomUint(maxValInclusive);
	}
	// 0 to maxValInclusive
	public int RandomInt(int maxValInclusive){
		//UpdateStaticSeed(); already done in called function
		if (maxValInclusive < 0) Debug.LogError("called RandomUint with a negative int value - not supported!");
		return (int)RandomUint((uint)maxValInclusive);
	}
	// 0 to maxValInclusive
	public float RandomFloat(float maxValInclusive = 1.0f){
		UpdateRndState();
		return maxValInclusive * RandomSquirrel.Get1dNoiseZeroToOne(rndState, seed);
	}
	// minValInclusive to maxValInclusive
	public float RandomFloatRange(float minValInclusive, float maxValInclusive){
		//UpdateStaticSeed(); already done in called function below
		if (maxValInclusive < minValInclusive) return minValInclusive;
		maxValInclusive -= minValInclusive;
		if (maxValInclusive == 0) return minValInclusive + 0;
		return minValInclusive + RandomFloat(maxValInclusive);
	}
	// minValInclusive to maxValInclusive
	public int RandomIntRange(int minValInclusive, int maxValInclusive){
		if (maxValInclusive < minValInclusive) return minValInclusive;
		UpdateRndState();
		maxValInclusive -= minValInclusive;
		if (maxValInclusive == 0) return minValInclusive + 0;
		return minValInclusive + (int)(RandomSquirrel.Get1dNoiseUint(rndState, seed) % (maxValInclusive + 1));
	}
	/// <summary>
	/// This turns a float into an int, but any float fractions are treated as a chance to round up.
	/// This means e.g. 1,4f has a 60%chance to return as 1 and a 40% chance to return 2
	/// </summary>
	/// <returns></returns>
	public int RoundFloatToIntByChance(float rawValue){
		//UpdateStaticSeed(); already done in called function
		int result = Mathf.FloorToInt(rawValue);
		result += RandomChanceInt(rawValue - (float)result);
		return result;
	}
	// returns "true" on success and "false" on miss
	public bool RandomChanceBool(float chance = 1.0f){
		//UpdateStaticSeed(); already done in called function
		if (chance == 0f) return false;
		if (RandomFloat() <= chance) return true;
		else return false;
	}
	// returns "1" on success and "0" on miss
	public int RandomChanceInt(float chance = 1.0f){
		//UpdateStaticSeed(); already done in called function
		if (chance == 0f) return 0;
		if (RandomFloat() <= chance) return 1;
		else return 0;
	}
	// returns "1" on success and "0" on miss
	public float RandomChanceFloat(float chance = 1.0f){
		//UpdateStaticSeed(); already done in called function
		if (chance == 0f) return 0f;
		if (RandomFloat() <= chance) return 1f;
		else return 0f;
	}
	
	// 0 to maxValInclusive
	public static uint RandomUint_Threadsafe(uint maxValInclusive, int positionX, uint seed){
		return RandomSquirrel.Get1dNoiseUint(positionX, seed) % (maxValInclusive + 1);
	}
	// perlin noise on 2d plane, return values transformed to 0-1 value range
	public static float PerlinNoise_0_to_1(float x, float y, int seed = 0){
		var val = Icaria.Engine.Procedural.IcariaNoise.GradientNoise(x, y, seed);
		val = (val + 1.0f) / 2.0f;
		return val;
	}
}
