using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xolito.Utilities
{
    public static class Utilities
    {
        public static float Get_Angle(Vector2 direction)
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

        public static bool Find_TagIn(string[] tagsToAvoid, string tagToFind)
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

        public static void StopTime(bool shouldStop ) => Time.timeScale = shouldStop? 0 : 1;

        public static void StopTime()
        {
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }

        public static void DrawWireCube(Vector3 direction, float offset, float size)
        {
            Gizmos.DrawWireCube(Get_VectorWithOffset(direction, offset), new Vector3(size, size, 0));
        }

        private static Vector3 Get_VectorWithOffset(Vector3 direction, float offset)
        {
            Vector3 newStart = new Vector3()
            {
                x = direction.x + offset,
                y = direction.y + offset,
                z = 0
            };
            return newStart;
        }
    } 
}
