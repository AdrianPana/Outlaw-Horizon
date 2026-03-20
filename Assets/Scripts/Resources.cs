using UnityEngine;

namespace Game.Resources
{
    public enum Modifier
    {
        NONE,
        WIND_NORTH,
        WIND_EAST,
        WIND_SOUTH,
        WIND_WEST,
        GRAVITY_INVERTED,
    };

    public enum WindDirection
    {
        NONE,
        NORTH,
        EAST,
        SOUTH,
        WEST
    };

    public class GravityEventSource
    {
        bool gravityInverted;
        Vector3 sourcePosition;
    }

    public class WindEventSource
    {
        WindDirection windDirection;
        Vector3 sourcePosition;
    }

    public static class ResourceHelper
    {
        public static Modifier WindDirectionToModifier(WindDirection windDirection)
        {
            switch (windDirection)
            {
                case WindDirection.NORTH:
                    return Modifier.WIND_NORTH;
                case WindDirection.EAST:
                    return Modifier.WIND_EAST;
                case WindDirection.SOUTH:
                    return Modifier.WIND_SOUTH;
                case WindDirection.WEST:
                    return Modifier.WIND_WEST;
                default:
                    return Modifier.NONE;
            }
        }

        public static WindDirection ModifierToWindDirection(Modifier modifier)
        {
            switch (modifier)
            {
                case Modifier.WIND_NORTH:
                    return WindDirection.NORTH;
                case Modifier.WIND_EAST:
                    return WindDirection.EAST;
                case Modifier.WIND_SOUTH:
                    return WindDirection.SOUTH;
                case Modifier.WIND_WEST:
                    return WindDirection.WEST;
                default:
                    return WindDirection.NONE;
            }
        }
    }

    public static class OH_Helpers
    {
        public static bool isInRangeNoHeight(Vector3 point, Vector3 origin, float range)
        {
            if (range < 0) return true; // negative range means infinite range

            Vector2 point2D = new Vector2(point.x, point.z);
            Vector2 origin2D = new Vector2(origin.x, origin.z);
            return Vector2.Distance(point2D, origin2D) <= range;
        }
    }
}