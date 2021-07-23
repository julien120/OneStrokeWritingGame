using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageData : MonoBehaviour
{
    public struct StageDataFormat
    {
        public readonly int stageIndex;
        public readonly int xPos;
        public readonly int yPos;
        public readonly int depressionxPos;
        public readonly int depressionyPos;

        //後ほど追加でPrefab名参照

        public StageDataFormat(int _stageIndex, int _xPos, int _yPos ,int _depressionxPos, int _depressionyPos)
        {
            stageIndex = _stageIndex;
            xPos = _xPos;
            yPos = _yPos;
            depressionxPos = _depressionxPos;
            depressionyPos = _depressionyPos;
        }
    }
}
