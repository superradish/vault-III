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
            Debug.Log("selected" + selectedGem.ToString());
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
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            // Matches?
            //matches = FindMatches();
            List<Vector2Int> connected = new List<Vector2Int>();
            //if there are no matches, swap back and return
            Debug.Log("entering possible infinite loop");
            matches = isConnected(gridPosA, true);
            if (matches.Count > 0){
                updated.Add(gridPosA);
            }
            //if there are no matches check gridPosB for matches
            if (matches.Count == 0) {
                matches = isConnected(gridPosB, true);
                if (matches.Count > 0){
                    updated.Add(gridPosB);
                }
                //if there are no matches swap back and return
                if (matches.Count == 0) {
                    StartCoroutine(SwapGems(gridPosB, gridPosA));
                    DeselectGem();
                    yield break;
                }
            }

            // TODO: Calculate score
            // Make Gems explode
            do{
                //load gridpos gem into a variable
                var gem = grid.GetValue(gridPosA.x, gridPosA.y).GetValue();
                //upgrade it
                UpgradeGem(gem);
                
                
                yield return new WaitForSeconds(0.5f); //wait and see if the upgrade happened before doing anything else
                //if there are matches, explode them
                yield return StartCoroutine(ExplodeGems(matches));
//check the update list for gems to upgrade and insert them before making the gems fall
                for (int i = 0; i < update.Count; i++)
                {
                    Vector2Int pos = update[i];
                    //load gem object from pos
                    
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
                    yield return StartCoroutine(ExplodeGems(matches));

                    // Make gems fall
                    yield return StartCoroutine(MakeGemsFall());
                    // Fill empty spots
                    yield return StartCoroutine(FillEmptySpots());
                }
            }while (matches.Count > 0);
            // TODO: Check if game is over
 
            DeselectGem();
        }
 
        void UpgradeGem(Vector2Int gridPos, int typeValue){
         //upgrade the gem  
                     //if the grid value is not null, return
            if (grid.GetValue(gridPos.x, gridPos.y) != null) return;
            //retrieve the value using gridpos and the matchedgems dictionary
            //destroy the gem at gridpos
            var gem = grid.GetValue(gridPos.x, gridPos.y).GetValue();
            Destroy(gem.gameObject);
            //create a new gem at gridpos
            
            gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(gridPos.x, gridPos.y), Quaternion.identity, transform);
            if (typeValue != 7){
                typeValue++;
            }
            gem.SetType(gemTypes[typeValue]);
            var gridObject = new GridObject<Gem>(grid, gridPos.x, gridPos.y);
            gridObject.SetValue(gem);
            grid.SetValue(gridPos.x, gridPos.y, gridObject);
            
            var matchedGemObject = grid.GetValue(gridPos.x, gridPos.y).GetValue();
 
           
 
            grid.SetValue(matchedGem.x, matchedGem.y, gridObject);
            
           
            
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

            foreach (var match in matches) {
                var gem = grid.GetValue(match.x, match.y).GetValue();
                grid.SetValue(match.x, match.y, null);

                ExplodeVFX(match);
                
                gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);
                
                yield return new WaitForSeconds(0f);
                
                //if the gem is not in the updated list, destroy it
                if(!updated.Contains(match)){Destroy(gem.gameObject, 0.1f);} 
            }      
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
        if (!IsValidPosition(p + dir)) break;
        
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
            if (IsValidPosition(gridPos)) {
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
 
        void UpgradeGem(Gem gem)
        {   
            //get the x and y position of the gem
            var x = (int) gem.transform.position.x;
            var y = (int) gem.transform.position.y;
            int val = gem.Value;
           //destroy the gem
           grid.SetValue(x, y, null);
            Destroy(gem.gameObject);
            //replace the gridobject with a new one
            CreateGem(x, y, val);
            //if the grid value is null let us know
            if (grid.GetValue(x, y) == null) Debug.Log("grid value is null");
        }

        void DeselectGem() => selectedGem = new Vector2Int(-1, -1);
        void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;
 
        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;
 
        bool IsValidPosition(Vector2 gridPosition) {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}