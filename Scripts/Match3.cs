using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
 
namespace Match3 {
    public class Match3 : MonoBehaviour {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool bugDe = true;
        
        [SerializeField] Gem gemPrefab;
        [SerializeField] GemType[] gemTypes;
        [SerializeField] Ease ease = Ease.InQuad;
       // [SerializeField] GameObject explosion;
        
        List<Vector2Int> matches;
        InputReader inputReader;
        AudioManager audioManager;
 
        GridSystem2D<GridObject<Gem>> grid;
 
        Vector2Int selectedGem = Vector2Int.one * -1;
        Vector2Int matchedGem = Vector2Int.one * -1;
        List <Vector2Int> adjacentPositions;
        //create a dictionary to track upgraded gems and their value
   
        List <Vector2Int> update = new List<Vector2Int>();
        List <Vector2Int> updated = new List<Vector2Int>();
 
 
       
 
        void Awake() {
            inputReader = GetComponent<InputReader>();
            audioManager = GetComponent<AudioManager>();
        }
        
        void Start() {
            InitializeGrid();
            inputReader.Fire += OnSelectGem;
        }
 
        void OnDestroy() {
            inputReader.Fire -= OnSelectGem;
        }
 
        void OnSelectGem() {
            //get grid position of selected gem from input reader
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));
            //copy gridpos so it doesn't get modified
            var gridPosCopy = gridPos;
            //create list of adjacent positions 
            var adjacentPositions = new List<Vector2Int>();
            //add adjacent positions to list
            adjacentPositions.Add(new Vector2Int(gridPosCopy.x + 1, gridPosCopy.y));
            adjacentPositions.Add(new Vector2Int(gridPosCopy.x - 1, gridPosCopy.y));
            adjacentPositions.Add(new Vector2Int(gridPosCopy.x, gridPosCopy.y + 1));
            adjacentPositions.Add(new Vector2Int(gridPosCopy.x, gridPosCopy.y - 1));
            //check if adjacent positions are valid, if not remove them from the list
            for (var i = 0; i < adjacentPositions.Count; i++) {
                if (!IsValidPosition(adjacentPositions[i])) {
                    adjacentPositions.RemoveAt(i);
                    i--;
                }
            }
            //check if selected gem is valid, if not return
            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;
            //check if selected gem is already selected, if so deselect
            if (selectedGem == gridPos) {
                DeselectGem();
                adjacentPositions = new List<Vector2Int>();//clear adjacent positions list
                //audioManager.PlayDeselect();
            } else if (selectedGem == Vector2Int.one * -1) {//check if no gem is selected, if so select
                SelectGem(gridPos);
            Debug.Log("selected" + selectedGem.ToString() + " " + "Value" + GetGemTypeValue(selectedGem).ToString());
                //audioManager.PlaySelect();
            } else {//if a gem is already selected, check if selected gem is adjacent
                if (adjacentPositions.Contains(selectedGem)) {
                    StartCoroutine(RunGameLoop(gridPos, selectedGem));
                    adjacentPositions = new List<Vector2Int>(); //clear adjacent positions list
                }
                    else { //return if not adjacent
                    adjacentPositions = new List<Vector2Int>(); //clear adjacent positions list
                    DeselectGem();
                    return;
 
                }
             
                //audioManager.PlayClick();
            } 
            
        }
 
        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB) {
            //if either gridpos is a vault at value 8, return
            if (GetGemTypeValue(gridPosA) == 8 || GetGemTypeValue(gridPosB) == 8) yield break;
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            // Matches?
            //matches = FindMatches();
            List<Vector2Int> connected = new List<Vector2Int>();
            List<Vector2Int> connectedInside = new List<Vector2Int>();
            //if there are no matches, swap back and return
            matches = isConnected(gridPosA, true);
            var gemA = grid.GetValue(gridPosA.x, gridPosA.y).GetValue();
            var gemB = grid.GetValue(gridPosB.x, gridPosB.y).GetValue();
            if (matches.Count > 0){
                updated.Add(gridPosA);
                //if there's a match on gridPosA we still need to check if there is a match on gridPosB, so do that
         
            }       
            connectedInside = isConnected(gridPosB, true);
            if (connectedInside.Count > 0){
                updated.Add(gridPosB);  
                for (int i = 0; i < connectedInside.Count; i++){         //add all of the values of connectedinside to connected if there are any
                matches.Add(connectedInside[i]);
            }
            }
            
            
   

            // TODO: Calculate score
            // Make Gems explode
            do{
                //load gridpos gem into a variable
          
                //upgrade it if it was matched
                if (matches.Count > 0 && updated.Contains(gridPosA)){
                    Debug.Log("line 139");
                    UpgradeGem(gemA);
                    matches.Remove(gridPosA);
                }
                //upgrade gridposB gem if it was matched
                
                if (matches.Count > 0 && updated.Contains(gridPosB)){
                    Debug.Log("line 145");
                    UpgradeGem(gemB);
                    matches.Remove(gridPosB);
                }

                if (matches.Count == 0) {
                    StartCoroutine(SwapGems(gridPosB, gridPosA));
                    DeselectGem();
                    yield break;
                }
                
                //yield return new WaitForSeconds(0.5f); //wait and see if the upgrade happened before doing anything else
                //if there are matches, explode them
                yield return StartCoroutine(ExplodeGems(matches));
//check the update list for gems to upgrade and insert them before making the gems fall
                
                for (int i = 0; i < updated.Count; i++)
                {
                    Vector2Int pos = updated[i];

                    //check if pos is connected to anything and upgrade it if it is
                    List<Vector2Int> connectedAgain = isConnected(pos, true);
                    if (connectedAgain.Count > 0){
                        updated.Add(pos);
                        UpgradeGem(pos, GetGemTypeValue(pos));
                        connectedAgain.Remove(pos);
                    }
                    //then explode the gems that are not upgraded
                    if (connectedAgain.Count > 0){

                        yield return StartCoroutine(ExplodeGems(connectedAgain));
                    }
                    
                }
                
                yield return StartCoroutine(MakeGemsFall());
                // Fill empty spots
 
                yield return StartCoroutine(FillEmptySpots());
                //go through the update list and check for matches on each one
                for (int i = 0; i < update.Count; i++)
                {
                    Vector2Int pos = update[i];
                    //check for matches
                    matches = isConnected(pos, true);
                    //if there are no matches, continue
                    if (matches.Count == 0) continue;
                    //if there are matches, explode them
                    //for each match, upgrade the gem and explode the others
                    bool upgraded = false;
                    for (int j = 0; j < matches.Count; j++)
                    {
                        Vector2Int match = matches[j];
                        if (match == pos && !upgraded)
                        {
                            updated.Add(pos);
                            UpgradeGem(pos, GetGemTypeValue(pos));
                            upgraded = true;
                        }
                        else if (!upgraded)
                        {
                            updated.Add(pos);
                            var gem = grid.GetValue(pos.x, pos.y).GetValue();
                            UpgradeGem(gem);
                            Debug.Log("upgraded gem at " + pos.ToString());
                            upgraded = true;
                        }
                    }

                    yield return StartCoroutine(ExplodeGems(matches));



                    // Make gems fall
                    yield return StartCoroutine(MakeGemsFall());
                    // Fill empty spots
                    yield return StartCoroutine(FillEmptySpots());
                }
            }while (matches.Count > 0);
            // TODO: Check if game is over
 
            DeselectGem();
            update.Clear();
            updated.Clear();
        }
         void UpgradeGem(Gem gem)
        {   
            //get the x and y position of the gem
            if (gem == null) return;
            var x = (int) gem.transform.position.x;
            var y = (int) gem.transform.position.y;
            int typeValue = gem.Value;
           //destroy the gem
           grid.SetValue(x, y, null);
            Destroy(gem.gameObject);
            //replace the gridobject with a new one
            if (typeValue != 8){
                CreateGem(x, y, typeValue);
                }
            //if the grid value is null let us know
            if (grid.GetValue(x, y) == null) Debug.Log("grid value is null");
        }

        void UpgradeGem(Vector2Int gridPos, int typeValue){
         //upgrade the gem  
            var x = gridPos.x;
            var y = gridPos.y;
            //destroy the gem at gridpos
            var gem = grid.GetValue(gridPos.x, gridPos.y).GetValue();
            grid.SetValue(x, y, null);
            Destroy(gem.gameObject);
            //create a new gem at gridpos
            if (typeValue != 8){
            CreateGem(x, y, typeValue);
            }

            if (grid.GetValue(x, y) == null) Debug.Log("grid value is null");
        }
 
        IEnumerator FillEmptySpots() {
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    if (grid.GetValue(x, y) == null) {
                        CreateGem(x, y, -1);
                        //audioManager.PlayPop();
                        yield return new WaitForSeconds(0f);;
                        
                    }
                }
            }
            
        }
 
        IEnumerator MakeGemsFall() {
            // TODO: Make this more efficient
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    if (grid.GetValue(x, y) == null) {
                        for (var i = y + 1; i < height; i++) {
                            if (grid.GetValue(x, i) != null) {
                                var gem = grid.GetValue(x, i).GetValue();
                                grid.SetValue(x, y, grid.GetValue(x, i));
                                grid.SetValue(x, i, null);
                                gem.transform
                                    .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0f)
                                    .SetEase(ease);
                                update.Add(new Vector2Int(x, y));
                               // audioManager.PlayWoosh();
                                yield return new WaitForSeconds(0.01f);
                                break;
                            }
                        }
                    }
                }
            }
        }
 IEnumerator ExplodeGems(List<Vector2Int> matches) {
    audioManager.PlayPop();
    //go through each match in matches and make sure there are no null values, remove them if there are
    for (int i = 0; i < matches.Count; i++)
    {
        if (grid.GetValue(matches[i].x, matches[i].y).GetValue() == null)
        {
            matches.Remove(matches[i]);
            i--;
        }
    }

    foreach (var match in matches) {
        if (grid.GetValue(match.x, match.y).GetValue() == null) continue;
        var gem = grid.GetValue(match.x, match.y).GetValue();
        if (!updated.Contains(match))
        {
            grid.SetValue(match.x, match.y, null);
        }
        

        ExplodeVFX(match);
        
        gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);
        
        yield return new WaitForSeconds(0.1f);
        
        //if the gem is not in the updated list, destroy it
        if(!updated.Contains(match))
        {
            Destroy(gem.gameObject);
        } 
    }     
    matches.Clear(); 
}
          /*  foreach (KeyValuePair<Vector2Int, int> kvp in matchedGems)
                {
                    Debug.Log("Key = {0}, Value = {1}"+ kvp.Key + kvp.Value);
                    GetGemTypeValue(kvp.Key);
                }*/
     
        //method to create a gem at a coordinate vector2int and upgrade it
 
        void ExplodeVFX(Vector2Int match) {
            // TODO: Pool
            //var fx = Instantiate(explosion, transform);
            //fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
           // Destroy(fx, 5f);
        }
 
        List<Vector2Int> FindMatches() {   //deprecated
            HashSet<Vector2Int> matches = new();
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var gem = grid.GetValue(x, y);
                    if (gem == null) continue;

                    // Check adjacent gems horizontally
                    if (x < width - 2) {
                        var gemA = grid.GetValue(x, y);
                        var gemB = grid.GetValue(x + 1, y);
                        var gemC = grid.GetValue(x + 2, y);

                        if (gemA != null && gemB != null && gemC != null &&
                            gemA.GetValue().GetType() == gemB.GetValue().GetType() &&
                            gemB.GetValue().GetType() == gemC.GetValue().GetType()) {
                            matches.Add(new Vector2Int(x, y));
                            matches.Add(new Vector2Int(x + 1, y));
                            matches.Add(new Vector2Int(x + 2, y));
                        }
                    }

                    // Check adjacent gems vertically
                    if (y < height - 2) {
                        var gemA = grid.GetValue(x, y);
                        var gemB = grid.GetValue(x, y + 1);
                        var gemC = grid.GetValue(x, y + 2);

                        if (gemA != null && gemB != null && gemC != null &&
                            gemA.GetValue().GetType() == gemB.GetValue().GetType() &&
                            gemB.GetValue().GetType() == gemC.GetValue().GetType()) {
                            matches.Add(new Vector2Int(x, y));
                            matches.Add(new Vector2Int(x, y + 1));
                            matches.Add(new Vector2Int(x, y + 2));
                        }
                    }
                }
            }

            if (matches.Count == 0) {
                // audioManager.PlayNoMatch();
            } else {
                // audioManager.PlayMatch();
            }

            return new List<Vector2Int>(matches);
        }
            
    
        IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB) {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);
            
            // See README for a link to the DOTween asset
            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);
            
            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);
            
            yield return new WaitForSeconds(0.5f);
        }
 
        void InitializeGrid() {
            grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, bugDe);
            
            
            
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    CreateGem(x, y);
                }
            }

        }
        
        List <Vector2Int> getAdjacentPositions(Vector2Int gridPos){
            var adjacentPositions = new List<Vector2Int>();
            //add adjacent positions to list
            adjacentPositions.Add(new Vector2Int(gridPos.x + 1, gridPos.y));
            adjacentPositions.Add(new Vector2Int(gridPos.x - 1, gridPos.y));
            adjacentPositions.Add(new Vector2Int(gridPos.x, gridPos.y + 1));
            adjacentPositions.Add(new Vector2Int(gridPos.x, gridPos.y - 1));
            //check if adjacent positions are valid, if not remove them from the list
            for (var i = 0; i < adjacentPositions.Count; i++) {
                if (!IsValidPosition(adjacentPositions[i])) {
                    adjacentPositions.RemoveAt(i);
                    i--;
                }
            }
            return adjacentPositions;
        }

List<Vector2Int> isConnected(Vector2Int p, bool main)
{
    List<Vector2Int> connected = new List<Vector2Int>();
    int val = GetGemTypeValue(p);
    Vector2Int[] directions =
    {
        new Vector2Int(0, 1), // up
        new Vector2Int(1, 0), // right
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0) // left
    };

    //make sure directions are valid

    foreach(Vector2Int dir in directions) //Checking if there is 2 or more same shapes in the directions
    {
        //check if the direction is valid first
        //if (!IsValidPosition(p + dir)) break;
        
        List<Vector2Int> line = new List<Vector2Int>();

        int same = 0;
        for(int i = 1; i < 3; i++)
        {
            Vector2Int check = p + dir * i;
            if(GetGemTypeValue(check) == val)
            {
                line.Add(check);
                same++;
            }
        }

        if (same > 1) //If there are more than 1 of the same shape in the direction then we know it is a match
            AddPoints(ref connected, line); //Add these points to the overarching connected list
            Debug.Log("Match found");
    }

    for(int i = 0; i < 2; i++) //Checking if we are in the middle of two of the same shapes
    {
        List<Vector2Int> line = new List<Vector2Int>();

        int same = 0;
        Vector2Int[] check = { p + directions[i], p + directions[i + 2] };
        foreach (Vector2Int next in check) //Check both sides of the piece, if they are the same value, add them to the list
        {
            if (GetGemTypeValue(next) == val)
            {
                line.Add(next);
                same++;
            }
        }

        if (same > 1)
            AddPoints(ref connected, line);
    }


    if(main) //Checks for other matches along the current match
    {
        for (int i = 0; i < connected.Count; i++)
            AddPoints(ref connected, isConnected(connected[i], false));
    }

    return connected;
}

    void AddPoints(ref List<Vector2Int> points, List<Vector2Int> add)
    {
        foreach(Vector2Int p in add)
        {
            bool doAdd = true;
            for(int i = 0; i < points.Count; i++)
            {
                if(points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) points.Add(p);
        }
    }
        

        int GetGemTypeValue(Vector2Int gridPos) {
            if (IsValidPosition(gridPos) && !IsEmptyPosition(gridPos) ){
                var gem = grid.GetValue(gridPos.x, gridPos.y);
                int blah = gem.GetValue().Value;
               // Debug.Log("poobar gem value " + blah.ToString());
                return blah;
            }
            else {
             //   Debug.Log("Invalid grid position");
                return -1; // or any other appropriate value
            }
        }

        void CreateGem(int x, int y, int type = -1) {
            //if the grid value is not null, return
            if (grid.GetValue(x, y) != null) return;
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(type == -1 ? gemTypes[Random.Range(0, 4)] : gemTypes[type]);
            Debug.Log(gem.Value.ToString() + " gem value foo");
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            grid.SetValue(x, y, gridObject);
        }
 


        void DeselectGem() => selectedGem = new Vector2Int(-1, -1);
        void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;
 
        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;
 
        bool IsValidPosition(Vector2 gridPosition) {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}