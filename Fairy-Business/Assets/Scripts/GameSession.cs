using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using DG.Tweening;
using Locations;
using UI;
using UI.Menu;

public enum LocationsType
{
    //some types are not valid anymore
    //Todo: Marie 14.08 Remove after refactoring locations.
    Invalid = -1,
    Throne = 0,
    PirateShip = 1,
    EnchantedForest = 2,
    Castle = 3,
    
    Stealer = 4,
    DragonCave = 5,
    GingerbreadHouse = 6,
    GainSpyAfterCount = 7,
    DrawPeaceAfterCount = 8,
    CantGoBelow3 = 9,
    VP3OnPeace = 10,
    HauntedHouse = 11,
    BottomOfTheSea = 12,
    ThroughTheMirror = 13,
    BackFromTheGrave = 14,
}

public class GameSession : MonoBehaviour {
    
    public bool dynamicInput = false;
    public CardInput cardInput;

    public GameObject soundsContainer;
    public Image ScanEffect;

    public List<LocationDefinition> SelectedLocationTypes => selectedLocationTypes;
    public List<FlipButton> locationTypesFlipper = new List<FlipButton>();
    public List<FlipButton> locationFlipper = new List<FlipButton>();

    [SerializeField] private Button locationSelectButton;
    [SerializeField] private  List<TurnRoundUI> turnRoundUIs;

    private List<LocationDefinition> selectedLocationTypes = new List<LocationDefinition>();
    private Dictionary<int, Location> locations;
    private int turnCounter;
    private int roundCounter;
    private Dictionary<PlayerColor, int> victoryPointCounters;
    
    private void Start() {
        //cardInput.onStartEvaluation.AddListener(delegate(ScanResult result){
        //    //code here using "result"
        //});
        //cardInput.onStartEvaluation.AddListener(delegate{
        //    //code here without any parameter
        //});

        cardInput.onStartEvaluation.AddListener(NewCard);
        cardInput.onAcceptCardEvaluation.AddListener(IngredientPaused);
        cardInput.onCancelCurrentEvaluation.AddListener(IngredientPaused);
        
        locationSelectButton.onClick.AddListener(OpenLocationsSelectionsMenu);
        locationSelectButton.gameObject.SetActive(false);
    }

    private void IngredientPaused(ScanResult result){
        //IngredientPaused();
    }

    private void IngredientPaused(){
        //StopIngredient();
    }

    public void ResetSelectedLocationTypes()
    {
        selectedLocationTypes = new List<LocationDefinition>();
        
        for (int i = 0; i < locationTypesFlipper.Count; i++){
            locationTypesFlipper[i].SetSideInstant(FlipButton.ActiveSide.back);
        }
    }

    public void ResetGamesession(){
        
        locations = new Dictionary<int, Location>();
        
        for (int i = 0; i < locationFlipper.Count; i++){
            
            locationFlipper[i].SetSideInstant(FlipButton.ActiveSide.back);
            locationFlipper[i].BackImage.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
            locationFlipper[i].FrontImage.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
            Vector3 correctedPos = locationFlipper[i].GetComponent<RectTransform>().position;
            correctedPos.y = 0;
            locationFlipper[i].GetComponent<RectTransform>().position = correctedPos;
        }
        
        turnCounter = 5; // first "NextTurn" action iterates this back down to 1
        roundCounter = 0; // first "NextTurn" action iterates this up to 1
        victoryPointCounters = new();
        victoryPointCounters[PlayerColor.Blue] = 0;
        victoryPointCounters[PlayerColor.Red] = 0;

        UpdateVictoryPointDisplay();
        disallowNewCards = false;
        
        HidePower();
        UniqueNameHash.Get("WinnerTextImageRed").gameObject.SetActive(false);
        UniqueNameHash.Get("WinnerTextImageBlue").gameObject.SetActive(false);
        UniqueNameHash.Get("WinnerText").gameObject.SetActive(false);
        UniqueNameHash.Get("WinnerScreen").gameObject.SetActive(false);
    }
    public enum PlayerColor
    {
        Neutral = 0,
        Red = 1,
        Blue = 2,
    }
    public void NewRound()
    {
        ResetGamesession();
        
        // apply the power setups of 5-3, 4-4 and 3-5 randomly over the locations
        List<int> ints = new List<int>{5,4,3};
        Utilities.ShuffleList(ints);

        CreateLocations();
        int i = 0;
        
        foreach (LocationDefinition loc in selectedLocationTypes)
        {
            int powerRed = ints[i];
            locations[i+1].SetPlayerPower(PlayerColor.Red, powerRed);
            locations[i+1].SetPlayerPower(PlayerColor.Blue, (8 - powerRed));
            i++;
        }
        
        // update Territory owners
        ReattributeTerritories();
        // start first turn
        NextTurn(); 
    }

    private void CreateLocations()
    {
        locations = new Dictionary<int, Location>();
        int i = 0;

        foreach (LocationDefinition tgtLocation in selectedLocationTypes)
        {
            // update Location buttons in order
            FlipButton tgtBtn = locationFlipper[i];
            i++;
            // using images from their respective new location
            LocationDefinition newComp = tgtBtn.gameObject.AddComponent<LocationDefinition>();
            newComp.CopyFrom(tgtLocation);
            newComp.UpdateFlipButton();
            Location newLocation = new Location { type = tgtLocation.locationType, VPGainedOnScorePhase = tgtLocation.VPGainedOnScorePhase };
            
            locations.Add(i, newLocation);
        }
    }

    public PlayerColor CheckLocationOwner(LocationsType location){
        
        PlayerColor currentMarketOwner = PlayerColor.Neutral;
        
        foreach (KeyValuePair<int, Location> loc in locations){
            if (loc.Value.type == location){
                currentMarketOwner = loc.Value.currentOwner;
            }
        }
        
        return currentMarketOwner;
    }

    public void ReattributeTerritories(){
        
        foreach (KeyValuePair<int, Location> loc in locations){
            
            Location location = loc.Value;
            
            if (location.GetPlayerPower(PlayerColor.Red) > location.GetPlayerPower(PlayerColor.Blue)){
                
                location.currentOwner = PlayerColor.Red;
                
            } else if (location.GetPlayerPower(PlayerColor.Red) < location.GetPlayerPower(PlayerColor.Blue)) {
                
                location.currentOwner = PlayerColor.Blue;
                
            } else {
                
                location.currentOwner = PlayerColor.Neutral;
                // on tie, whoever currently owns the special place becomes the new owner! (if its part of the current match, otherwise its Neutral)
                PlayerColor tieLocationOwner = CheckLocationOwner(LocationsType.GingerbreadHouse);
                location.currentOwner = tieLocationOwner;
                
            }
        }
        
         UpdateLocationVisuals();
    }

    Sequence rotationSequence = null;
    public void UpdateLocationVisuals(){
        
        if (rotationSequence != null) 
            rotationSequence.Kill();
        
        float initialPauseTime = 0.5f;
        Ease rotationEaseMode = Ease.InOutCubic;
        rotationSequence = DOTween.Sequence();
        
        int i = 0;
        
        foreach (KeyValuePair<int, Location> loc in locations){
            
            Location location = loc.Value;
            
            if (location.currentOwner != PlayerColor.Neutral){
                
                if (location.currentOwner == PlayerColor.Red){
                    
                    locationFlipper[i].GetComponent<RectTransform>().DOLocalMoveY(150, 0.6f);
                    rotationSequence.Insert(initialPauseTime, locationFlipper[i].BackContent.transform.DORotate(new Vector3(0,0,-180), 0.7f).SetEase(rotationEaseMode));
                    rotationSequence.Insert(initialPauseTime, locationFlipper[i].FrontContent.transform.DORotate(new Vector3(0,0,-180), 0.7f).SetEase(rotationEaseMode));
                    
                } else {
                    
                    locationFlipper[i].GetComponent<RectTransform>().DOLocalMoveY(-150, 0.6f);
                    rotationSequence.Insert(initialPauseTime, locationFlipper[i].BackContent.transform.DORotate(new Vector3(0,0,0), 0.7f).SetEase(rotationEaseMode));
                    rotationSequence.Insert(initialPauseTime, locationFlipper[i].FrontContent.transform.DORotate(new Vector3(0,0,0), 0.7f).SetEase(rotationEaseMode));
                    
                }
                
                locationFlipper[i].SetSideWithAnim(FlipButton.ActiveSide.front);
                
            } else {
                
                locationFlipper[i].GetComponent<RectTransform>().DOLocalMoveY(0, 0.6f);
                rotationSequence.Insert(initialPauseTime, locationFlipper[i].BackContent.transform.DORotate(new Vector3(0,0,-90), 0.7f).SetEase(rotationEaseMode));
                rotationSequence.Insert(initialPauseTime, locationFlipper[i].FrontContent.transform.DORotate(new Vector3(0,0,-90), 0.7f).SetEase(rotationEaseMode));
                locationFlipper[i].SetSideWithAnim(FlipButton.ActiveSide.back);
                
            }
            
            i++;
        }
    }
    
    public void SetupSelectButton(FlipButton flipper){
        SetupSelectLocation(flipper);
    }
    
    public void SetupSelectLocation(FlipButton flipper){
        
        LocationDefinition loc = flipper.gameObject.GetComponent<LocationDefinition>();
        
        if (selectedLocationTypes.Contains(loc))
        {
            selectedLocationTypes.Remove(loc);
            flipper.SetSideWithAnim(FlipButton.ActiveSide.front);
        } else {
            selectedLocationTypes.Add(loc);
            flipper.SetSideWithAnim(FlipButton.ActiveSide.back);
        }
        
        CheckEnoughLocationsSelected();
    }

    private void CheckEnoughLocationsSelected(){
        if (selectedLocationTypes.Count == 3){
            UiManager.CallbackUiEvent("EnoughLocationsSelected");
            locationSelectButton.gameObject.SetActive(true);
        }
        else
        {
            locationSelectButton.gameObject.SetActive(false);
        }
    }

    private void OpenLocationsSelectionsMenu()
    {
        MenuManager.OpenMenu(MenuIdentifier.LocationSelectionMenu);
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
            turnLocation.locationNumber = int.Parse(card.effect);
        }

        // remember action/location
        if (card.playerColor == "Blue"){
            if (actionFound) {
                turnActions[PlayerColor.Blue] = turnAction;
                NewActionLoggedIn(PlayerColor.Blue);
            }
            if (locationFound) {
                if (!locations.ContainsKey(turnLocation.locationNumber)){
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
                if (!locations.ContainsKey(turnLocation.locationNumber)){
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
        NewCardLoggedIn();
    }
    public void NewLocationLoggedIn(PlayerColor playerColor){
        //TagJK_implement visuals for logged in location
        //TagJK_implement audio for logged in location
        if (playerColor == PlayerColor.Red){
            UniqueNameHash.Get("ActionRedActive1").gameObject.SetActive(true);
        } else if (playerColor == PlayerColor.Blue){
            UniqueNameHash.Get("ActionBlueActive1").gameObject.SetActive(true);
        }
        NewCardLoggedIn();
    }
    public void NewCardLoggedIn(){
        //TagJK_implement visuals for logged in card
        //TagJK_implement audio for logged in card
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
    
    int[] allLocationNumbers = new int[]{ 1, 2, 3};
    public void SolveTurn(){
        // politics
        PlayerColor marketOwner = CheckLocationOwner(LocationsType.EnchantedForest);
        foreach (var actingPlayer in playerColors){
            if (turnActions[actingPlayer].action == Action.Politics){
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                int politicsMod = marketOwner == actingPlayer ? 1 : 0;
                locations[turnLocations[actingPlayer].locationNumber].power[actingPlayer] += (turnActions[actingPlayer].value + politicsMod);
            }
        }
        // Army
        PlayerColor sourceOwner = CheckLocationOwner(LocationsType.PirateShip);
        PlayerColor controlCap3Owner = CheckLocationOwner(LocationsType.CantGoBelow3);
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
                    Location attackedLocation = locations[attackedLocationNumber];
                    int currentEnemyControlValue = attackedLocation.power[enemyPlayer];
                    // if the "Cant go below 3 control value" location is owned by your enemy, dont allow him to go lower than 3 (or his current, if its lower than 3)
                    if (controlCap3Owner == enemyPlayer){
                        minControlNumber = math.min(3, currentEnemyControlValue);
                    }
                    // reduce control value (but cap it at 'minControlNumber'; usually at 0)
                    int newTheoreticalControlValue = currentEnemyControlValue - attackValue;
                    attackedLocation.power[enemyPlayer] = math.max(newTheoreticalControlValue, minControlNumber);
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
                    locations[turnLocations[actingPlayer].locationNumber].power[enemyPlayer] = 0;
                }
            }
        }
        // Peace
        PlayerColor vp3OnPeaceOwner = CheckLocationOwner(LocationsType.VP3OnPeace);
        
        foreach (var actingPlayer in playerColors){
            
            if (turnActions[actingPlayer].action == Action.Peace){
                
                PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                // if red has same location and the opposite effect, they cancel each other!
                if (turnLocations[enemyPlayer].locationNumber != turnLocations[actingPlayer].locationNumber || turnActions[enemyPlayer].action != Action.War)
                {
                    int victoryPoints = locations[turnLocations[actingPlayer].locationNumber].power[actingPlayer];
                    locations[turnLocations[actingPlayer].locationNumber].power[actingPlayer] = 0;
                    victoryPointCounters[actingPlayer] += victoryPoints;
                }
                // EVERY TIME a peace is played, while the vp3OnPeace location is owned by a player, give that player 3VP
                if (vp3OnPeaceOwner != PlayerColor.Neutral){
                    
                    victoryPointCounters[vp3OnPeaceOwner] += 3;
                }
            }
        }
        
        // update territory ownership
        ReattributeTerritories();
        // end of turn effects (if any) are applied
        EndOfTurnEffects();
        
        NextTurn();
    }



    public void EndOfTurnEffects(){
        
        foreach (var loc in locations){
            
            Location location = loc.Value;
            
            if (location.type == LocationsType.Stealer){
                if (location.currentOwner != PlayerColor.Neutral){
                    
                    PlayerColor actingPlayer = location.currentOwner;
                    PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                    
                    if (victoryPointCounters[enemyPlayer] > victoryPointCounters[actingPlayer] && victoryPointCounters[enemyPlayer] > 0){
                        victoryPointCounters[enemyPlayer]--;
                        victoryPointCounters[actingPlayer]++;
                    }
                }
            }
            if (location.type == LocationsType.DragonCave){
                
                if (location.currentOwner != PlayerColor.Neutral){
                    
                    PlayerColor actingPlayer = location.currentOwner;
                    PlayerColor enemyPlayer = GetEnemy(actingPlayer);
                    victoryPointCounters[actingPlayer]++;
                }
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
        
        if (color != PlayerColor.Neutral){
            victoryPointCounters[color] += vp;
        }
        
    }
    
    public void CheckScoringPhase(){
        
        if (turnCounter == 1 && roundCounter > 1){
            // apply owned territory points to main score
            
            foreach (var loc in locations){
                var location = loc.Value;
                
                AddVictoryPointsByPlayer(location.currentOwner, location.VPGainedOnScorePhase);
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
    
    public void ShowPower(){
        
        int i = 0;
        
        foreach (var loc in locations){
            var location = loc.Value;
            for (int j = 0; j < 2; j++)
            {
                UniqueNameHash.Get($"RedPower{i}_{j}").GetComponent<TMP_Text>().text = location.power[PlayerColor.Red].ToString();
                UniqueNameHash.Get($"BluePower{i}_{j}").GetComponent<TMP_Text>().text = location.power[PlayerColor.Blue].ToString();
            }
            i++;
        }
        
        UniqueNameHash.Get("PointOverview").gameObject.SetActive(true);
    }
    
    public void HidePower(){
        UniqueNameHash.Get("PointOverview").gameObject.SetActive(false);
    }
    
    public bool CheckEndGame(){
        
        if (roundCounter >= 5){
            FinishGameAndShowWinner();
            return true;
        }
        
        return false;
    }
    
    public void FinishGameAndShowWinner(){
        
        disallowNewCards = true;
        UniqueNameHash.Get("TurnAndRoundCounter").gameObject.SetActive(false);
        string winnerText = "Draw!";
        
        if (victoryPointCounters[PlayerColor.Red] > victoryPointCounters[PlayerColor.Blue]){
            
            winnerText = "Player Red Won!";
            UniqueNameHash.Get("WinnerTextImageRed").gameObject.SetActive(true);
            
        } else if (victoryPointCounters[PlayerColor.Red] < victoryPointCounters[PlayerColor.Blue]){
            
            winnerText = "Player Blue Won!";
            UniqueNameHash.Get("WinnerTextImageBlue").gameObject.SetActive(true);
        }
        
        UniqueNameHash.Get("WinnerText").GetComponent<TMP_Text>().text = winnerText;
        
        if (victoryPointCounters[PlayerColor.Red] == victoryPointCounters[PlayerColor.Blue]){
            // currently only used for draw, because i dont have an image there. will probably be used for all text once i have a ttf file though!
            UniqueNameHash.Get("WinnerText").gameObject.SetActive(true); 
        }
        
        UniqueNameHash.Get("WinnerScreen").gameObject.SetActive(true); 
    }
    
}