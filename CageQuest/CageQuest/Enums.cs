using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageQuest
{
    public enum UserAction
    {
        GoLeft,
        GoRight,
        GoForward,
        GoBack
    }

    public enum LocationType
    {
        Enter,
        Tunnel,
        DeadEnd,
        Fork,
        End
    }

    public enum PlayerDirection
    {
        North,
        East,
        South,
        West
    }
}
