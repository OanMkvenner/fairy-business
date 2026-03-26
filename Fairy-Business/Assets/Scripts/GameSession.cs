using System;
using System.Collections;
using System.Collections.Generic;
using ComponentsHYBR.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using DG.Tweening;
using Locations;
using Player;
using UI;
using UI.Gameplay;
using UI.Menu;
using UI.Menu.BaseMenu;

public enum ScanAction
{
    None = 0,
    CreditCard = 1,
    ControlCard = 2,
}
public class GameSession : MonobehaviourSingletonCustom<GameSession>
{
    public int MaxRoundCount => maxRoundCount;
    public static event Action<LocationDefinition, int> OnSpyCardPlayed;
    public static event Action OnTurnReset;
    public static event Action<PlayerColor, ScanAction> OnCardScanned;
    
    public CardInput cardInput;
    public Image ScanEffect;

    public Dictionary<PlayerColor, int> VictoryPointCounters
    {
        get => victoryPointCounters;
        set => victoryPointCounters = value;
    }

    [SerializeField] private int maxRoundCount;

    [Space]
    [SerializeField] private TurnRoundUI turnRoundUI;

    private int turnCounter;

    private int roundCounter;

    private Dictionary<PlayerColor, int> victoryPointCounters;
    
    private Coroutine sendEventCoroutine;

    private void Start() {

        cardInput.onStartEvaluation.AddListener(NewCard);
        cardInput.onAcceptCardEvaluation.AddListener(IngredientPaused);
        cardInput.onCancelCurrentEvaluation.AddListener(IngredientPaused);
    }

    private void IngredientPaused(ScanResult result){
        //IngredientPaused();
    }

    private void IngredientPaused(){
        //StopIngredient();
    }

    public void ResetSelectedLocationTypes()
    {
        if(LocationManager.instance.SelectedLocations != null)
            LocationManager.instance.SelectedLocations.Clear();
    }

    private void ResetGamesession(){
        
        LocationManager.instance.ResetGameLocations();
        
        turnCounter = 5; // first "NextTurn" action iterates this back down to 1
        roundCounter = 0; // first "NextTurn" action iterates this up to 1
        victoryPointCounters = new();
        victoryPointCounters[PlayerColor.Blue] = 0;
        victoryPointCounters[PlayerColor.Red] = 0;

        UpdateVictoryPointDisplay();
        disallowNewCards = false;
        
        HidePower();
    }

    public void NewRound()
    {
        ResetGamesession();
        
        LocationManager.instance.CreateGameLocations();
        
        // update Territory owners
        ReattributeTerritories();
        // start first turn
        NextTurn(); 
    }

    private PlayerColor CheckLocationOwner(LocationsIdentifier location){
        
        PlayerColor currentMarketOwner = PlayerColor.Neutral;
        
        foreach (LocationDefinition loc in LocationManager.instance.GameLocations){
            if (loc.LocationIdentifier == location){
                currentMarketOwner = loc.CurrentOwner;
            }
        }
        
        return currentMarketOwner;
    }

    private void ReattributeTerritories(){
        
        foreach (LocationDefinition loc in LocationManager.instance.GameLocations)
        {
            if (loc.LocationIdentifier != LocationsIdentifier.GingerbreadHouse)
                continue;
            
            // on tie, whoever currently owns the special place becomes the new owner! (if its part of the current match, otherwise its Neutral)
            PlayerColor tieLocationOwner = CheckLocationOwner(LocationsIdentifier.GingerbreadHouse);
            loc.CurrentOwner = tieLocationOwner;
            
        }
        
        LocationManager.instance.UpdateLocationAnimation();
    }
    
    public struct Card
    {
        public string playerColor;
        public string effect;
        public string value;
    }
    
    private void NewCard(ScanResult result){
        Debug.Log($"New Card: {result.name}");
        string[] labelData = result.name.Split('_');
        Card card = new Card();
        if (labelData.Length < 1) return;
        card.playerColor = labelData[0];
        if (labelData.Length >= 2) 
        card.effect = labelData[1];
        if (labelData.Length >= 3) 
        card.value = labelData[2];
        AddTurnAction(card);
    }
    
    bool disallowNewCards = false;
    
    public enum CardAction
    {
        Invalid = 0,

        // these numbers are used for ORDERING the actions as well! be mindful when you change them..
        Politics = 1,
        Army = 2,
        //Fire = 3,
        War = 4,
        Peace = 5,
        //Corruption = 6,
        Spy = 7,
    }
    
    class TurnAction
    {
        public CardAction CardAction;
        public int value;
    }
    
    class TurnLocation
    {
        public int locationNumber;
    }

    Dictionary<PlayerColor, TurnAction> turnActions = new();
    Dictionary<PlayerColor, TurnLocation> turnLocations = new();

    public void AddTurnAction(Card card){
        if (disallowNewCards)
            return;

        TurnAction turnAction = new TurnAction();
        TurnLocation turnLocation = new TurnLocation();

        if (card.playerColor == "Spy") {
            ShowPower();
            ShowWhiteFlash();
            return;
        }

        bool actionFound = false;
        bool locationFound = false;
        
        if (card.effect == "Politics") {
            actionFound = true;
            turnAction.CardAction = CardAction.Politics;
            turnAction.value = int.Parse(card.value);
        }
        if (card.effect == "Army") {
            actionFound = true;
            turnAction.CardAction = CardAction.Army;
            turnAction.value = int.Parse(card.value);
        }
        if (card.effect == "Peace") {
            actionFound = true;
            turnAction.CardAction = CardAction.Peace;
        }
        if (card.effect == "War") {
            actionFound = true;
            turnAction.CardAction = CardAction.War;
        }

        if (card.effect == "1" || card.effect == "2" || card.effect == "3") {
            locationFound = true;
            turnLocation.locationNumber = int.Parse(card.effect) -1;
        }

        // remember action/location
        if (card.playerColor == "Blue"){
            if (actionFound) {
                turnActions[PlayerColor.Blue] = turnAction;
                NewActionLoggedIn(PlayerColor.Blue);
            }
            if (locationFound) {
                if (LocationManager.instance.GameLocations[turnLocation.locationNumber] == null){
                    // location not found in current match, cancel turn addition!
                    Debug.LogError($"Location {turnLocation.locationNumber} is not part of the current match!");
                    return;
                }
                turnLocations[PlayerColor.Blue] = turnLocation;
                NewLocationLoggedIn(PlayerColor.Blue);
            }
        }
        if (card.playerColor == "Red"){
            if (actionFound) {
                turnActions[PlayerColor.Red] = turnAction;
                NewActionLoggedIn(PlayerColor.Red);
            }
            if (locationFound) {
                if (LocationManager.instance.GameLocations[turnLocation.locationNumber] == null){
                    // location not found in current match, cancel turn addition!
                    Debug.LogError($"Location {turnLocation.locationNumber} is not part of the current match!");
                    return;
                }
                
                turnLocations[PlayerColor.Red] = turnLocation;
                NewLocationLoggedIn(PlayerColor.Red);
            }
        }
        
        if (locationFound || actionFound)
        {
            HidePower();
            CheckTurnComplete();
        }
    }
    
    private void ShowWhiteFlash(){
        Sounds.instance.Play("ConfirmSpyCard");
        ScanEffect.color = new Color(1,1,1,0);
        ScanEffect.DOFade(0.78f, 0.2f)
            .SetLoops(2, LoopType.Yoyo);
    }

    private void NewActionLoggedIn(PlayerColor playerColor){
        OnCardScanned?.Invoke(playerColor, ScanAction.ControlCard);
        Sounds.instance.Play("ConfirmScannedCard");
    }

    private void NewLocationLoggedIn(PlayerColor playerColor){
        OnCardScanned?.Invoke(playerColor, ScanAction.CreditCard);
        Sounds.instance.Play("ConfirmScannedCard");
    }

    private void CheckTurnComplete(){
        
        if (turnActions[PlayerColor.Blue] != null && turnLocations[PlayerColor.Blue] != null && turnActions[PlayerColor.Red] != null && turnLocations[PlayerColor.Red] != null){
            SolveTurn();
        } else {
            // more actions/locations needed for turn to resolve
        }
    }
    
    PlayerColor[] playerColors= new PlayerColor[]{
        PlayerColor.Red,
        PlayerColor.Blue,
    };

    private PlayerColor GetEnemy(PlayerColor firstPlayer)
    {
        switch (firstPlayer)
        {
            case PlayerColor.Red:
                return PlayerColor.Blue;
            case PlayerColor.Blue:
                return PlayerColor.Red;
            default:
                Debug.LogError("invalid color provided");
                return PlayerColor.Red;
        }
    }

    readonly int[] allLocationNumbers = new int[]{ 0, 1, 2};

    /// <summary>
    /// Resolves all player actions for the current turn. Processes card actions in the following order: Politics,
    /// Army, War, and Peace. Applies location modifiers, updates control values, awards victory points,
    /// resolves territory ownership, triggers end-of-turn effects, checks end game conditions, and advances to the next turn.
    /// </summary>
    private void SolveTurn()
    {
        // ---------- POLITICS ----------
        // Check who owns the Enchanted Forest (politics modifier location)
        PlayerColor enchantedForestOwner = CheckLocationOwner(LocationsIdentifier.EnchantedForest);

        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Only resolve players who played a Politics card
            if (turnActions[actingPlayer].CardAction == CardAction.Politics)
            {
                // Get the opposing player
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);

                // Gain +1 power if the acting player owns the Enchanted Forest
                int politicsMod = enchantedForestOwner == actingPlayer ? 1 : 0;

                // Add power to the targeted location
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .AddPlayerPower(actingPlayer, turnActions[actingPlayer].value + politicsMod);

                // Recalculate control and determine the new winner of the location
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .FinalizePowerAndDetermineWinner();
            }
        }

        // ---------- ARMY ----------
        // Location-based modifiers for Army actions
        PlayerColor sourceOwner = CheckLocationOwner(LocationsIdentifier.PirateShip);          // +1 attack
        PlayerColor WeakAttackOnAllOwner = CheckLocationOwner(LocationsIdentifier.ThroughTheMirror); // Attack all locations at half strength
        PlayerColor below0Gain2VPOwner = CheckLocationOwner(LocationsIdentifier.BottomOfTheSea);      // Gain VP when enemy drops below 0

        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Only resolve players who played an Army card
            if (turnActions[actingPlayer].CardAction == CardAction.Army)
            {
                // Get the opposing player
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);

                // Gain +1 attack if the acting player owns the Pirate Ship
                int armyMod = sourceOwner == actingPlayer ? 1 : 0;

                // Minimum allowed control value (usually 0)
                int minControlNumber = 0;

                // Default: attack only the selected location
                int[] attackedLocationNumbers = new int[] { turnLocations[actingPlayer].locationNumber };

                // Base attack value including modifiers
                int attackValue = turnActions[actingPlayer].value + armyMod;

                // If the player owns Through the Mirror, attack all locations at half strength
                if (WeakAttackOnAllOwner == actingPlayer)
                {
                    attackedLocationNumbers = allLocationNumbers;
                    attackValue /= 2;
                }

                // Apply the attack to each affected location
                foreach (int attackedLocationNumber in attackedLocationNumbers)
                {
                    LocationDefinition attackedLocation =
                        LocationManager.instance.GameLocations[attackedLocationNumber];

                    // Current enemy control before the attack
                    int currentEnemyControlValue = attackedLocation.GetPlayerPower(enemyPlayer);

                    // Theoretical control value after the attack (before clamping)
                    int newTheoreticalControlValue = currentEnemyControlValue - attackValue;

                    // Reduce enemy power at the location
                    attackedLocation.AddPlayerPower(enemyPlayer, -attackValue);

                    // Recalculate control and determine the new winner
                    attackedLocation.FinalizePowerAndDetermineWinner();

                    // If the enemy drops below 0 control and the player owns Bottom of the Sea,
                    // award 2 victory points (unless the loss was blocked)
                    if (below0Gain2VPOwner == actingPlayer &&
                        newTheoreticalControlValue < 0 &&
                        minControlNumber <= 0)
                    {
                        victoryPointCounters[below0Gain2VPOwner] += 2;
                        UpdateVictoryPointDisplay();
                    }
                }
            }
        }

        // ---------- WAR ----------
        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Only resolve players who played a War card
            if (turnActions[actingPlayer].CardAction == CardAction.War)
            {
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);

                // War is cancelled if the enemy played Peace at the same location
                if (turnLocations[enemyPlayer].locationNumber != turnLocations[actingPlayer].locationNumber ||
                    turnActions[enemyPlayer].CardAction != CardAction.Peace)
                {
                    // Remove all enemy power from the targeted location
                    LocationManager.instance
                        .GameLocations[turnLocations[actingPlayer].locationNumber]
                        .SetPlayerPower(enemyPlayer, 0);
                }
            }
        }

        // ---------- PEACE ----------
        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Skip players who did not play Peace
            if (turnActions[actingPlayer].CardAction != CardAction.Peace)
                continue;

            PlayerColor enemyPlayer = GetEnemy(actingPlayer);

            // Peace is cancelled if the enemy played War at the same location
            if (turnLocations[enemyPlayer].locationNumber != turnLocations[actingPlayer].locationNumber ||
                turnActions[enemyPlayer].CardAction != CardAction.War)
            {
                // Convert current control at the location into victory points
                int victoryPoints = LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .GetPlayerPower(actingPlayer);

                // Remove the player's power from the location
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .SetPlayerPower(actingPlayer, 0);

                // Award victory points to the acting player
                victoryPointCounters[actingPlayer] += victoryPoints;
            }
        }

        // ---------- END OF TURN ----------
        // Update territory ownership based on final control values
        ReattributeTerritories();

        // Apply any end-of-turn effects
        EndOfTurnEffects();

        // Check if end-game conditions are met
        CheckEndGame();

        // Advance to the next turn
        NextTurn();
    }


    private void EndOfTurnEffects(){
        
        foreach (LocationDefinition loc in LocationManager.instance.GameLocations)
        {
            if (loc.LocationIdentifier != LocationsIdentifier.DragonCave) continue;
            
            if (loc.CurrentOwner != PlayerColor.Neutral){
                    
                PlayerColor actingPlayer = loc.CurrentOwner;
                victoryPointCounters[actingPlayer]++;
            }
        }
    }

    private void ResetTurn(){
        
        turnActions[PlayerColor.Blue] = null;
        turnActions[PlayerColor.Red] = null;
        turnLocations[PlayerColor.Blue] = null;
        turnLocations[PlayerColor.Red] = null;

        sendEventCoroutine = StartCoroutine(SendEvent());
    }

    private IEnumerator SendEvent()
    {
        yield return new WaitForSeconds(2f);
        OnTurnReset?.Invoke();
        sendEventCoroutine = null;
    }

    private void NextTurn(){

        turnCounter++;
        
        if (turnCounter >= 5)
        {
            turnCounter = 1;
            roundCounter++;
            
            turnRoundUI.UpdateRoundCount(roundCounter);
        }
        
        CheckScoringPhase();
        
        bool gameEnded = CheckEndGame();
        
        if (!gameEnded)
        {
            turnRoundUI.FillCurrentTurn(turnCounter);
            ResetTurn();
        }
    }

    private void AddVictoryPointsByPlayer(PlayerColor color, int vp){
        
        if (color == PlayerColor.Neutral)
            return;
        
        victoryPointCounters[color] += vp;
    }

    private void CheckScoringPhase(){
        
        if (turnCounter == 1 && roundCounter > 1){
            // apply owned territory points to main score
            
            foreach (LocationDefinition loc in LocationManager.instance.GameLocations){
                AddVictoryPointsByPlayer(loc.CurrentOwner, loc.VictoryPoints);
            }
            
            UpdateVictoryPointDisplay();
        }
    }
    private void UpdateVictoryPointDisplay(){
        
        UniqueNameHash.Get("VictoryPointsRed").GetComponent<TMP_Text>().text = victoryPointCounters[PlayerColor.Red].ToString();
        UniqueNameHash.Get("VictoryPointsBlue").GetComponent<TMP_Text>().text = victoryPointCounters[PlayerColor.Blue].ToString();
    }

    private void ShowPower()
    {
        UniqueNameHash.Get("PointOverview").gameObject.SetActive(true);
        
        for (int index = 0; index < LocationManager.instance.GameLocations.Count; index++)
        {
            LocationDefinition loc = LocationManager.instance.GameLocations[index];
            OnSpyCardPlayed?.Invoke(loc, index);
        }
    }

    private void HidePower(){
       UniqueNameHash.Get("PointOverview").gameObject.SetActive(false);
    }

    private bool CheckEndGame(){
        
        if (roundCounter >= maxRoundCount){
            FinishGameAndShowWinner();
            return true;
        }
        
        return false;
    }

    private void FinishGameAndShowWinner(){
        
        disallowNewCards = true;
        MenuManager.instance.OpenMenu(MenuIdentifier.WinScreen);
    }
}