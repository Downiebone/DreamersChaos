using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keepingTrackOfLevels : MonoBehaviour
{
    public string[] levels;
    public string lastLevel;

    public string getRandomNewLevel()
    {
        int randomLevelind = Random.Range(0, levels.Length);

        string level = levels[randomLevelind];

        if(level == lastLevel)
        {
            randomLevelind++;
            if (randomLevelind >= levels.Length)
                randomLevelind = 0;

            level = levels[randomLevelind];
        }

        lastLevel = level;
        return level;
    }
}
