using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [SerializeField] private List<WorldBuildingData> _globalMapBuildings=new(); //TODO check if need to make it SerField
    [SerializeField] private string _playerJson;
 	[SerializeField] private SaveScrObj _saveObject;
    private WorldCharacter _playerCharacter;
    private string _filePath = "savefile.txt";


    public string[] TypesOfBuildingsOnLocation (int x, int y)
    {
        List<string> allBuildingsOnLocations = new (
            from building 
            in _globalMapBuildings 
            where (building.X == x && building.Y == y) 
            select building.type.ToString()
        );
        return allBuildingsOnLocations.Distinct().ToArray();

    }
    
    public string[] BuildingsOfTypeOnLocation (int x, int y, IWorldBuilding objectOfType) {
		WorldBuildingData.BuildingType type = BuildingTypeByObject(objectOfType);
		return _globalMapBuildings.Where(
				building=>building.X==x 
				&& building.Y==y
				&& building.type==type)
			.Select(building=>building.Name)
			.ToArray();
	}
	
	public string GetBuildingJsonString (int x, int y, string buildingName) {
		foreach (var building in _globalMapBuildings)
			if (building.X==x && building.Y==y && building.Name==buildingName)
            {
                SaveCharacter();
				return building.jSonString;
            }
		return "";
	}

    public void SaveBuilding (IWorldBuilding building)
    {
        if (building == null)
            return;

        WorldBuildingData savableData = FindBuildingOnMap(building);
        if (savableData==null)
        {
            savableData = new WorldBuildingData();
            _globalMapBuildings.Add(savableData);
            print($"WORNING. Building {building.Name} was not on the map. Have created new one");
        }

        savableData.X = building.X;
        savableData.Y = building.Y;
        savableData.Name = building.Name;
        savableData.type = BuildingTypeByObject(building);
        savableData.jSonString = building.ToJson();
        SaveCharacter();
    }

    public void SaveCharacter ()
    {
        _playerJson = _playerCharacter.ToJson();
    }
    public void LoadCharacter()
    {
        _playerCharacter.FromJson(_playerJson);
    }
    public string GetCharacterJson ()
    {
        return _playerJson;
    }
	
	private WorldBuildingData.BuildingType BuildingTypeByName (string typeName) {
		if (typeName is "Factory")
            return WorldBuildingData.BuildingType.Factory;
		else if (typeName is "Mine") 
			return WorldBuildingData.BuildingType.Mine;
		else
			return WorldBuildingData.BuildingType.Laboratory;
	}
		
	private WorldBuildingData.BuildingType BuildingTypeByObject (IWorldBuilding buildingOfType) {
		return BuildingTypeByName(buildingOfType.GetType().Name);
	}

    private WorldBuildingData FindBuildingOnMap(IWorldBuilding building) => _globalMapBuildings
        .Find(b => (b.X == building.X && b.Y == building.Y && b.Name == building.Name));

    internal void LoadSaveSystem()
    {
        _playerCharacter = GameObject.Find("PlayerCharacter").GetComponent<WorldCharacter>();
        if (_globalMapBuildings.Count != 0)
            return;
    }

    public void LoadFromFile()
    {
		if (!File.Exists(_filePath))
			return;
		string saveText = File.ReadAllText(_filePath);
		JsonUtility.FromJsonOverwrite(saveText, this);
        LoadCharacter();
    }
	
	public void SaveToFile() 
	{
		string saveText = JsonUtility.ToJson(this);
		File.WriteAllText(_filePath, saveText);
	}
	
	public void LoadFromObject()
    {
		if (_saveObject==null)
			return;
		JsonUtility.FromJsonOverwrite(_saveObject.Save, this);
        LoadCharacter();
    }
	
	public void SaveToOject() 
	{
		if (_saveObject==null)
			return;
		_saveObject.Save = JsonUtility.ToJson(this);
	}
	
    [Serializable]
    private class WorldBuildingData
    {
        public enum BuildingType {Factory, Mine, Laboratory}

        public int X = 0;
        public int Y = 0;
        public BuildingType type;
        public string Name;
        public string jSonString;
    }
}

