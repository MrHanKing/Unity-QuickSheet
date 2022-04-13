using UnityEngine;
using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class ExcelExampleData
{
  [SerializeField]
  int id;
  public int Id { get {return id; } set { this.id = value;} }
  
  [SerializeField]
  string name;
  public string Name { get {return name; } set { this.name = value;} }
  
  [SerializeField]
  int strength;
  public int Strength { get {return strength; } set { this.strength = value;} }
  
  [SerializeField]
  Difficulty difficulty;
  public Difficulty DIFFICULTY { get {return difficulty; } set { this.difficulty = value;} }
  
  [SerializeField]
  int[] some = new int[0];
  public int[] Some { get {return some; } set { this.some = value;} }
  
}