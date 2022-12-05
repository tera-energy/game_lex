using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrCSVParser : MonoBehaviour
{
    public static TrIAPData[] zParse(){

        // Include Row of Front Number
        int colLength = 5;

        List<TrIAPData> iapList = new List<TrIAPData>();
        TextAsset csvData = Resources.Load<TextAsset>("IAP/IAP"); // csv ÆÄÀÏ °¡Á®¿È

        string[] data = csvData.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++){
            string[] row = data[i].Split(new char[] { ',' });
            if (i > colLength)
                continue;
            else if (row.Length < 2)
                break;

            TrIAPData iap = new TrIAPData
            {
                _serialNumber = int.Parse(row[0]),
                _id = string.Format("{0}{1}", TrProjectSettings._character, row[1]),
                _price = row[2],
                _type = (TrIAPType)(int.Parse(row[3])),
                _number = int.Parse(row[4])
            };

            iapList.Add(iap);
        }

        return iapList.ToArray();
    }
}
