using UnityEngine;
using TMPro;

namespace Match3 {
    public class Gem : MonoBehaviour {
        public GemType type;
        public int Value;

        public bool upgradeMe = false;
        
    [SerializeField] private TextMeshPro _text;

    [SerializeField] public BlockType _type;
        public void SetType(GemType type) {//set the type of the gem based on the sprite
            this.type = type;
        switch (type.sprite.name.ToLower())
            {
                case "bronze":
                    this.Value = 1;
                    break;
                case "silver":
                    this.Value = 2;
                    break;
                case "gold":
                    this.Value = 3;
                    break;
                case "bag":
                    this.Value = 4;
                    break;
                case "chest":
                    this.Value = 5;
                    break;
                case "betterChest":
                    this.Value = 6;
                    break;
                case "bettererChest":
                    this.Value = 7;
                    break;
                case "vault":
                    this.Value = 8;
                    break;
                default:
                    this.Value = 0;
                    break;
            }
        Debug.Log(this.Value.ToString());
            GetComponent<SpriteRenderer>().sprite = type.sprite;
        }
        
        public GemType GetType() => type;
    
    //method for setting the type of the gem based on the value, followed by changing the sprite to match



    public void Init(int val){
        this._type = (BlockType)val;
    }



    public void SetBlockType(int val)
    {
        this._type = (BlockType)val;
        this.Value = val;
    }

    
    public enum BlockType {   
        blank = 0,
        bronze = 1,
        silver = 2,
        gold = 3,
        bag = 4,
        chest = 5,
        betterChest = 6,
        bettererChest = 7,
        vault = 8
        
    }
        
    //return value

    public int GetValue(){
        return this.Value;
    }

    public BlockType GetBlockType() => _type;

    public void Upgrade(){
        this._type = (BlockType)this.Value;
        //change the sprite to match the new value
        switch (this.Value)
        {
            case 1:
                GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 2:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 3:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 4:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 5:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 6:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 7:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 8:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            default:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
        }    
       switch (this.Value)
        {
            case 1:
                GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 2:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 3:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 4:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 5:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 6:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 7:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            case 8:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
            default:
                 GetComponent<SpriteRenderer>().sprite = type.sprite;
                break;
        }

    }
    }
}