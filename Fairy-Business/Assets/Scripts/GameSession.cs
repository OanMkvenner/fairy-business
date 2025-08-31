using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using DG.Tweening;
using Locations;
using Player;
using UI;
using UI.Menu;
using UI.Menu.BaseMenu;


public class GameSession : MonobheaviourSingletonCustom<GameSession>
{

    public static event Action<LocationDefinition, int> OnSpyCardPlayed;
    
    public bool dynamicInput = false;
    public CardInput cardInput;

    public GameObject soundsContainer;
    public Image ScanEffect;

    public Dictionary<PlayerColor, int> VictoryPointCounters
    {
        get => victoryPointCounters;
        set => victoryPointCounters = value;
    }

    [SerializeField] private int maxRoundCount = 5;

    [Space]
    [SerializeField] private  List<TurnRoundUI> turnRoundUIs;

    private int turnCounter;

    private int roundCounter;

    private Dictionary<PlayerColor, int> victoryPointCounters;

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

    public void ResetGamesession(){
        
        LocationManager.instance.ResetGameLocations();

        foreach (TurnRoundUI turnRoundUI in turnRoundUIs)
        {
            turnRoundUI.ResetUI();
        }
        
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

    private PlayerColor CheckLocationOwner(LocationsType location){
        
        PlayerColor currentMarketOwner = PlayerColor.Neutral;
        
        foreach (LocationDefinition loc in LocationManager.instance.GameLocations){
            if (loc.LocationType == location){
                currentMarketOwner = loc.CurrentOwner;
            }
        }
        
        return currentMarketOwner;
    }

    private void ReattributeTerritories(){
        
        foreach (LocationDefinition loc in LocationManager.instance.GameLocations)
        {

            if (loc.LocationType != LocationsType.GingerbreadHouse)
                continue;
            
            // on tie, whoever currently owns the special place becomes the new owner! (if its part of the current match, otherwise its Neutral)
            PlayerColor tieLocationOwner = CheckLocationOwner(LocationsType.GingerbreadHouse);
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
    
    public enum Action
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
        public Action action;
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
            soundsContainer.GetComponent<Sounds>().Play("ConfirmCard");
            ShowWhiteFlash();
            return;
        }

        bool actionFound = false;
        bool locationFound = false;
        
        if (card.effect == "Politics") {
            actionFound = true;
            turnAction.action = Action.Politics;
            turnAction.value = int.Parse(card.value);
        }
        if (card.effect == "Army") {
            actionFound = true;
            turnAction.action = Action.Army;
            turnAction.value = int.Parse(card.value);
        }
        if (card.effect == "Peace") {
            actionFound = true;
            turnAction.action = Action.Peace;
        }
        if (card.effect == "War") {
            actionFound = true;
            turnAction.action = Action.War;
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
                //Todo: Marie Problem hier lösen
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
                //Todo:Problem hier lösen
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
            soundsContainer.GetComponent<Sounds>().Play("ConfirmCard");
            ShowWhiteFlash();
            HidePower(); // hide power as soon as any card was played!?
            CheckTurnComplete();
        }
    }
    
    public void ShowWhiteFlash(){
        ScanEffect.color = new Color(1,1,1,0);
        ScanEffect.DOFade(0.78f, 0.2f)
            .SetLoops(2, LoopType.Yoyo);
    }

    public void NewActionLoggedIn(PlayerColor playerColor){
        //TagJK_implement visuals for logged in action
        //TagJK_implement audio for logged in action
        if (playerColor == PlayerColor.Red){
            UniqueNameHash.Get("ActionRedActive2").gameObject.SetActive(true);
        } else if (playerColor == PlayerColor.Blue){
            UniqueNameHash.Get("ActionBlueActive2").gameObject.SetActive(true);
        }
    }
    
    public void NewLocationLoggedIn(PlayerColor playerColor){
        //TagJK_implement visuals for logged in location
        //TagJK_implement audio for logged in location
        if (playerColor == PlayerColor.Red){
            UniqueNameHash.Get("ActionRedActive1").gameObject.SetActive(true);
        } else if (playerColor == PlayerColor.Blue){
            UniqueNameHash.Get("ActionBlueActive1").gameObject.SetActive(true);
        }
    }
    
    public void CheckTurnComplete(){
        
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
    
    public PlayerColor GetEnemy(PlayerColor firstPlayer){
        if (firstPlayer == PlayerColor.Red){
            return PlayerColor.Blue;
        } else if (firstPlayer == PlayerColor.Blue){
            return PlayerColor.Red;
        } else {
            Debug.LogError("invalid color provided");
            return PlayerColor.Red;
        }
    }
    
    int[] allLocationNumbers = new int[]{ 0, 1, 2};
    
    public void SolveTurn(){
        // politics
        PlayerColor marketOwner = CheckLocationOwner(LocationsType.EnchantedForest);
        foreach (var actingPlayer in playerColors){
            if (turnActions[actingPlayer].action == Action.Politics){
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                int politicsMod = marketOwner == actingPlayer ? 1 : 0;
                LocationManager.instance
                    .GameLocations[turnLocations[actingPlayer].locationNumber]
                    .AddPlayerPower(actingPlayer, turnActions[actingPlayer].value + politicsMod);

            }
        }
        // Army
        PlayerColor sourceOwner = CheckLocationOwner(LocationsType.PirateShip);
        //PlayerColor controlCap3Owner = CheckLocationOwner(LocationsType.CantGoBelow3);
        PlayerColor WeakAttackOnAllOwner = CheckLocationOwner(LocationsType.ThroughTheMirror);
        PlayerColor below0Gain2VPOwner = CheckLocationOwner(LocationsType.BottomOfTheSea);
        
        foreach (PlayerColor actingPlayer in playerColors){
            
            if (turnActions[actingPlayer].action == Action.Army){
                
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                int armyMod = sourceOwner == actingPlayer ? 1 : 0;
                int minControlNumber = 0;
                int[] attackedLocationNumbers = new int[] { turnLocations[actingPlayer].locationNumber };
                int attackValue = turnActions[actingPlayer].value + armyMod;
                
                if(WeakAttackOnAllOwner == actingPlayer){
                    attackedLocationNumbers = allLocationNumbers;
                    attackValue /= 2;
                }
                foreach (var attackedLocationNumber in attackedLocationNumbers)
                {
                    LocationDefinition attackedLocation = LocationManager.instance.GameLocations[attackedLocationNumber];
                    int currentEnemyControlValue = attackedLocation.GetPlayerPower(enemyPlayer);
                    // reduce control value (but cap it at 'minControlNumber'; usually at 0)
                    int newTheoreticalControlValue = currentEnemyControlValue - attackValue;
                    attackedLocation.AddPlayerPower(enemyPlayer, math.max(newTheoreticalControlValue, minControlNumber));
                    // If you own the below0Gain2VP location, EVERY TIME you manage to reduce your enemy below 0 you gain 2VP. Effects that block this loss also block this effect
                    if (below0Gain2VPOwner == actingPlayer && newTheoreticalControlValue < 0 && minControlNumber <= 0){
                        victoryPointCounters[below0Gain2VPOwner] += 2;
                    }
                }
            }
        }
        
        // War
        foreach (PlayerColor actingPlayer in playerColors){
            
            if (turnActions[actingPlayer].action == Action.War){
                
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                // if red has same location and the opposite effect, they cancel each other!
                if (turnLocations[enemyPlayer].locationNumber != turnLocations[actingPlayer].locationNumber || turnActions[enemyPlayer].action != Action.Peace)
                {
                    LocationManager.instance.GameLocations[turnLocations[actingPlayer].locationNumber].SetPlayerPower(enemyPlayer, 0);
                }
            }
        }
        
        foreach (var actingPlayer in playerColors){
            
            if (turnActions[actingPlayer].action == Action.Peace){
                
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                // if red has same location and the opposite effect, they cancel each other!
                if (turnLocations[enemyPlayer].locationNumber != turnLocations[actingPlayer].locationNumber || turnActions[enemyPlayer].action != Action.War)
                {
                    int victoryPoints = LocationManager.instance.GameLocations[turnLocations[actingPlayer].locationNumber].GetPlayerPower(actingPlayer);
                    LocationManager.instance.GameLocations[turnLocations[actingPlayer].locationNumber]
                        .SetPlayerPower(actingPlayer, 0);
                    victoryPointCounters[actingPlayer] += victoryPoints;
                } 
                
            }
        }
        
        // update territory ownership
        ReattributeTerritories();
        // end of turn effects (if any) are applied
        EndOfTurnEffects();

        CheckEndGame();
        
        NextTurn();
    }
    
    public void EndOfTurnEffects(){
        
        foreach (LocationDefinition loc in LocationManager.instance.GameLocations)
        {
            if (loc.LocationType != LocationsType.DragonCave) continue;
            
            if (loc.CurrentOwner != PlayerColor.Neutral){
                    
                PlayerColor actingPlayer = loc.CurrentOwner;
                victoryPointCounters[actingPlayer]++;
            }
        }
    }
    
    public void ResetTurn(){
        
        turnActions[PlayerColor.Blue] = null;
        turnActions[PlayerColor.Red] = null;
        turnLocations[PlayerColor.Blue] = null;
        turnLocations[PlayerColor.Red] = null;
        
        UniqueNameHash.Get("ActionRedActive1").gameObject.SetActive(false);
        UniqueNameHash.Get("ActionBlueActive1").gameObject.SetActive(false);
        UniqueNameHash.Get("ActionRedActive2").gameObject.SetActive(false);
        UniqueNameHash.Get("ActionBlueActive2").gameObject.SetActive(false);
    }
    
    public void NextTurn(){

        turnCounter++;
        
        if (turnCounter >= 5)
        {
            if(roundCounter > 0) turnRoundUIs[roundCounter-1].FillFinishedRound();
            
            turnCounter = 1;
            roundCounter++;
        }
        
        CheckScoringPhase();
        
        bool gameEnded = CheckEndGame();
        
        if (!gameEnded)
        {
            turnRoundUIs[roundCounter-1].FillTurn(turnCounter-1);
            ResetTurn();
        }

    }
    public void AddVictoryPointsByPlayer(PlayerColor color, int vp){
        
        if (color != PlayerColor.Neutral)
            return;
        
        victoryPointCounters[color] += vp;
    }
    
    public void CheckScoringPhase(){
        
        if (turnCounter == 1 && roundCounter > 1){
            // apply owned territory points to main score
            
            foreach (LocationDefinition loc in LocationManager.instance.GameLocations){
                AddVictoryPointsByPlayer(loc.CurrentOwner, loc.VictoryPoints);
            }
            
            UpdateVictoryPointDisplay();

            // show current power values (until a player starts playing a card? or until turn is played fully?)
            ShowPower();
        }
    }
    public void UpdateVictoryPointDisplay(){
        
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