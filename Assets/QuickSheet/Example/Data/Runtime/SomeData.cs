using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityQuickSheet;

///
/// !!! Machine generated code !!!
///
/// A class which deriveds ScritableObject class so all its data 
/// can be serialized onto an asset data file.
/// 
[System.Serializable]
[ExcelSOClassAttribute]
public class SomeData : ExcelTableBase<SomeDataData> 
{	

    void OnEnable()
    {		
//#if UNITY_EDITOR
        //hideFlags = HideFlags.DontSave;
//#endif
        // Important:
        //    It should be checked an initialization of any collection data before it is initialized.
        //    Without this check, the array collection which already has its data get to be null 
        //    because OnEnable is called whenever Unity builds.
        // 		
        if (dataArray == null)
            dataArray = new SomeDataData[0];

    }
    
    //
    // Highly recommand to use LINQ to query the data sources.
    //

}