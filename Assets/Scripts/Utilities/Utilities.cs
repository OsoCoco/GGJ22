using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xolito.Utilities
{
    public static class Utilities
    {
        public static float Get_AngleDirection(Vector2 direction)
        {
            if (direction == Vector2.right)
                return 0;
            else if (direction == Vector2.left)
                return 180;
            else if (direction == Vector2.up)
                return 90;
            else if (direction == Vector2.down)
                return 270;

            return 0;
        }

        public static bool Find_TagIn(List<string> tagsToAvoid, string tagToFind)
        {
            bool founded = false;
            foreach (string tag in tagsToAvoid)
            {
                if (tagToFind == tag)
                {
                    founded = true;
                    break;
                }
            }

            return founded;
        }
    } 
}
