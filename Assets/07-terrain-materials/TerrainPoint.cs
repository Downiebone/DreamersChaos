using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainPoint {

    // The distance from the surface, we're setting to 1 by default because that is "above"
    // the surface. In other words, air.
    /// <summary>
    /// 1 = air,,,, 0 = ground
    /// </summary>
    public int dstToSurface = 1;

    // The ID of the texture this point will be located at.
    public int textureID = 0;

    // Having only one constructor that has parameters forces us to initialise new TerrainPoints
    // with values.
    public TerrainPoint (int dst, int tex) {

        dstToSurface = dst;
        textureID = tex;

    }

}
