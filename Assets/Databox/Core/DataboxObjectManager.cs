﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Databox
{
	// Databox Object Manager.
	// Manage multiple databox object in one place
	//
	[System.Serializable]
	[CreateAssetMenu(menuName = "Databox/New Databox Object Manager")]
	public class DataboxObjectManager : ScriptableObject
	{
		
		int loadedCount = 0;
		// EVENTS
		public delegate void DataboxManagerEvents();
		public DataboxManagerEvents OnAllDatabasesLoaded;
		
		
		[System.Serializable]
		public class DataboxObjects
		{
			public string id;
			public DataboxObject dataObject;
			public bool editorOpen; // setting for runtime editor
			
			public DataboxObjects (string _id, DataboxObject _dataObject)
			{
				id = _id;
				dataObject = _dataObject;
			}
		}
		
		
		public List<DataboxObjects> dataObjects = new List<DataboxObjects>();
		
	
		
		public DataboxObject GetDataboxObject(string _id)
		{
			for (int i = 0; i < dataObjects.Count; i ++)
			{
				if (_id == dataObjects[i].id)
				{
					return dataObjects[i].dataObject;
				}
			}
		
			Debug.LogError("Databox: No Databox Object with id: " + _id + " has been found");
			return null;
		}
		
		public void LoadAll()
		{
			loadedCount = 0;
			
			for (int i = 0; i < dataObjects.Count; i ++)
			{
				dataObjects[i].dataObject.OnDatabaseLoaded += Loaded;
				dataObjects[i].dataObject.LoadDatabase();
		
			}
		}
		
		void Loaded()
		{
			loadedCount ++;
		
			if (loadedCount == dataObjects.Count)
			{
				if (OnAllDatabasesLoaded != null)
				{
					OnAllDatabasesLoaded();
				}
			}
		}
	}
}
