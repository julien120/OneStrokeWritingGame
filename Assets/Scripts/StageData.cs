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

        //後ほど追加でPrefab名参照

        public StageDataFormat(int _stageIndex, int _xPos, int _yPos)
        {
            stageIndex = _stageIndex;
            xPos = _xPos;
            yPos = _yPos;
        }
    }

    public struct DeleteDataFormat
    {
        public readonly int xPos;
        public readonly int yPos;

        public DeleteDataFormat(int _xPos, int _yPos)
        {
            xPos = _xPos;
            yPos = _yPos;
        }
    }
}
