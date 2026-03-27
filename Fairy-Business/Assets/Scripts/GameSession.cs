using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ComponentsHYBR.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Locations;
using Player;
using UI.Gameplay;
using UI.Menu.BaseMenu;
using Random = System.Random;

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
        private set => victoryPointCounters = value;
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
    /// <remarks>I want to distance myself from this method, but that's what happens when you are not allowed to refactor.</remarks>
    private void SolveTurn()
    {
        // ---------- POLITICS (Plus) ----------
        // Politics actions increase the acting player's power on a chosen location.
        // Some locations provide special modifiers (e.g. bonus power or card upgrade).

        // Check who owns the Enchanted Forest (gives +1 power on Politics actions)
        PlayerColor enchantedForestOwner = CheckLocationOwner(LocationsIdentifier.EnchantedForest);

        // Check who owns the Magic Library Expert (upgrades a value 3 card into value 5)
        PlayerColor magicLibraryExpert = CheckLocationOwner(LocationsIdentifier.MagicLibraryExpert);

        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Only resolve players who played a Politics card
            if (turnActions[actingPlayer].CardAction == CardAction.Politics)
            {
                // Magic Library Expert effect:
                // If the acting player owns the Magic Library Expert location and played a value 3 card,
                // the card is upgraded to value 5.
                if (actingPlayer == magicLibraryExpert)
                {
                    if (turnActions[actingPlayer].value == 3)
                    {
                        turnActions[actingPlayer].value = 5;
                    }
                }

                // Enchanted Forest modifier:
                // Gain +1 extra power if the acting player owns the Enchanted Forest.
                int politicsMod = enchantedForestOwner == actingPlayer ? 1 : 0;

                // Add power to the selected location for the acting player
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .AddPlayerPower(actingPlayer, turnActions[actingPlayer].value + politicsMod);

                // Update the location state:
                // Recalculate power totals and determine which player controls the location now.
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .FinalizePowerAndDetermineWinner();
            }
        }

        // ---------- ARMY (Minus) ----------
        // Army actions reduce the enemy player's power at one or more locations.
        // Several locations modify the way attacks work (bonus attack, attack all locations, etc.).

        // Pirate Ship gives +1 attack power
        PlayerColor sourceOwner = CheckLocationOwner(LocationsIdentifier.PirateShip);

        // Pirate Ship Expert makes Army attack all locations at half strength
        PlayerColor WeakAttackOnAllOwner = CheckLocationOwner(LocationsIdentifier.PirateShipExpert);

        // Bottom of the Sea Expert awards VP when the enemy drops below 0
        PlayerColor below0Gain2VPOwner = CheckLocationOwner(LocationsIdentifier.BottomOfTheSeaExpert);

        // Gingerbread House Expert converts Army (-) actions into Politics (+) actions
        PlayerColor gingerbreadExpert = CheckLocationOwner(LocationsIdentifier.GingerbreadHouseExpert);

        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Only resolve players who played an Army card
            if (turnActions[actingPlayer].CardAction == CardAction.Army)
            {
                // Get the enemy player
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);

                // Magic Library Expert effect:
                // Upgrades a value 3 card into value 5.
                if (actingPlayer == magicLibraryExpert)
                {
                    if (turnActions[actingPlayer].value == 3)
                    {
                        turnActions[actingPlayer].value = 5;
                    }
                }

                // Pirate Ship modifier:
                // Gain +1 attack value if the acting player owns the Pirate Ship.
                int armyMod = sourceOwner == actingPlayer ? 1 : 0;

                // Minimum allowed control value (usually 0 in this game)
                // Used to determine if "below 0" logic is allowed.
                int minControlNumber = 0;

                // Default behavior: attack only the selected location
                int[] attackedLocationNumbers = new int[] { turnLocations[actingPlayer].locationNumber };

                // Attack value is based on card value + possible modifiers
                int attackValue = turnActions[actingPlayer].value + armyMod;

                // Pirate Ship Expert effect:
                // Attack ALL locations but only with half the attack strength.
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

                    // Current enemy power before the attack
                    int currentEnemyControlValue = attackedLocation.GetPlayerPower(enemyPlayer);

                    // Calculate what the enemy control value would become after the attack
                    // (without clamping to minimum values yet)
                    int newTheoreticalControlValue = currentEnemyControlValue - attackValue;

                    // Gingerbread House Expert effect:
                    // Instead of attacking, the Army card is treated like a Politics card
                    // and adds power to the acting player's selected location.
                    if (actingPlayer == gingerbreadExpert)
                    {
                        // Apply Enchanted Forest bonus if applicable
                        int politicsMod = enchantedForestOwner == actingPlayer ? 1 : 0;

                        // Add power to the targeted location
                        LocationManager.instance
                            .GameLocations[turnLocations[actingPlayer].locationNumber]
                            .AddPlayerPower(actingPlayer, turnActions[actingPlayer].value + politicsMod);

                        // Recalculate power totals and determine new controller
                        LocationManager.instance
                            .GameLocations[turnLocations[actingPlayer].locationNumber]
                            .FinalizePowerAndDetermineWinner();
                    }
                    else
                    {
                        // Standard Army effect:
                        // Reduce enemy power at the attacked location.
                        attackedLocation.AddPlayerPower(enemyPlayer, -attackValue);
                    }

                    // Update the location state after modification
                    attackedLocation.FinalizePowerAndDetermineWinner();

                    // Bottom of the Sea Expert effect:
                    // If the enemy drops below 0 control and negative values are allowed,
                    // award 2 victory points to the acting player.
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

        // ---------- WAR / TAKEOVER ----------
        // War actions attempt to wipe enemy power from a location.
        // War can be cancelled if the enemy plays Peace at the same location.

        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Only resolve players who played a War card
            if (turnActions[actingPlayer].CardAction == CardAction.War)
            {
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);

                // Through the Mirror gives bonus VP when playing War
                PlayerColor throughTheMirror = CheckLocationOwner(LocationsIdentifier.ThroughTheMirror);

                // Through the Mirror effect:
                // If the acting player owns this location and plays War, gain 2 VP immediately.
                if (throughTheMirror == actingPlayer)
                {
                    victoryPointCounters[actingPlayer] += 2;
                    UpdateVictoryPointDisplay();
                }

                // War is blocked if the enemy played Peace at the same location.
                // If not blocked, remove all enemy power from the targeted location.
                if (turnLocations[enemyPlayer].locationNumber != turnLocations[actingPlayer].locationNumber ||
                    turnActions[enemyPlayer].CardAction != CardAction.Peace)
                {
                    // Set enemy power to 0 at the targeted location (complete takeover)
                    LocationManager.instance
                        .GameLocations[turnLocations[actingPlayer].locationNumber]
                        .SetPlayerPower(enemyPlayer, 0);
                }
            }
        }

        // ---------- PEACE / CASH-IN ----------
        // Peace converts the player's current power at a location into victory points.
        // Peace can be cancelled if the enemy plays War at the same location.

        foreach (PlayerColor actingPlayer in playerColors)
        {
            // Skip players who did not play Peace
            if (turnActions[actingPlayer].CardAction != CardAction.Peace)
                continue;

            PlayerColor enemyPlayer = GetEnemy(actingPlayer);

            // Through the Mirror provides a bonus to Peace conversion
            PlayerColor throughTheMirror = CheckLocationOwner(LocationsIdentifier.ThroughTheMirror);

            // Enchanted Forest Expert reduces enemy Peace rewards (halves them)
            PlayerColor enemyEnchantedForestExpert = CheckLocationOwner(LocationsIdentifier.EnchantedForestExpert);

            int additionalPeacePower = 0;

            // Through the Mirror effect:
            // If the acting player owns it and plays Peace, add +2 extra VP conversion.
            if (throughTheMirror == actingPlayer)
            {
                additionalPeacePower = 2;
            }

            // Peace is blocked if the enemy played War on the same location
            bool isPeaceBlocked =
                (turnLocations[enemyPlayer].locationNumber == turnLocations[actingPlayer].locationNumber &&
                 turnActions[enemyPlayer].CardAction == CardAction.War);

            // If Peace is not blocked, cash in the player's power for victory points
            if (!isPeaceBlocked)
            {
                // Read how much power the acting player currently has at this location
                int victoryPoints = LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .GetPlayerPower(actingPlayer);

                // Remove the player's power from the location (cash-in consumes the power)
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .SetPlayerPower(actingPlayer, 0);

                // Total Peace reward = current power + any bonus
                int peacePower = victoryPoints + additionalPeacePower;

                // Enchanted Forest Expert effect:
                // If the enemy owns this expert location, the Peace reward is halved.
                if (enemyEnchantedForestExpert != actingPlayer && enemyEnchantedForestExpert != PlayerColor.Neutral)
                {
                    peacePower /= 2;
                }

                // Add the victory points to the acting player's score
                victoryPointCounters[actingPlayer] += peacePower;

                // Update UI
                UpdateVictoryPointDisplay();
            }
        }

        // ---------- END OF TURN ----------
        // After all actions have been resolved, update the board and proceed.

        // Recalculate territory ownership based on final power values
        ReattributeTerritories();

        // Apply special end-of-turn effects from locations or cards
        EndOfTurnEffects();

        // Check whether any win/end-game conditions have been met
        CheckEndGame();

        // Move to the next turn
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
            
            List<LocationDefinition> gameLocations = LocationManager.instance.GameLocations.OrderBy
                (p => p.LocationPriority).ToList();
            
            foreach (LocationDefinition loc in gameLocations){
                
                if(!loc.AreVictoryPointsApplied)
                {
                    AddVictoryPointsByPlayer(loc.CurrentOwner, loc.VictoryPoints);
                    loc.AreVictoryPointsApplied = true;
                }

                switch (loc.LocationIdentifier)
                {
                    case LocationsIdentifier.ThroughTheMirrorExpert:
                    {
                        Random rnd = new Random();

                        int randomVp = rnd.Next(1, 7);
                        
                        AddVictoryPointsByPlayer(loc.CurrentOwner, randomVp);
                        break;
                    }
                    case LocationsIdentifier.DragonCaveExpert:
                    {
                        if (victoryPointCounters[loc.CurrentOwner] < victoryPointCounters[GetEnemy(loc.CurrentOwner)])
                        {
                            victoryPointCounters[loc.CurrentOwner]++;
                            victoryPointCounters[GetEnemy(loc.CurrentOwner)]--;
                        }
                        break;
                    }
                    case LocationsIdentifier.MagicLibrary:
                    {
                        if (loc.LastOwner == loc.CurrentOwner)
                        {
                            AddVictoryPointsByPlayer(loc.CurrentOwner, 8);
                            break;
                        }

                        if (loc.LastOwner != PlayerColor.Neutral)
                        {
                            //special case bc of first round, bc there is no "last owner"
                            if (roundCounter == 2)
                            {
                                AddVictoryPointsByPlayer(loc.CurrentOwner, 8);
                                break;
                            }
                        }
                      
                        AddVictoryPointsByPlayer(loc.CurrentOwner, 8);
                        AddVictoryPointsByPlayer(loc.LastOwner, -8);
                        break;
                        
                    }case LocationsIdentifier.CastleExpert:
                    {
                        foreach (LocationDefinition location in gameLocations)
                        {
                            if (loc.CurrentOwner == location.CurrentOwner)
                            {
                                AddVictoryPointsByPlayer(loc.CurrentOwner, location.VictoryPoints * 2);
                                location.AreVictoryPointsApplied = true;
                            }
                        }
                        break;
                    }
                }
                
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