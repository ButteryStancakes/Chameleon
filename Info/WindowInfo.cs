using UnityEngine;

namespace Chameleon.Info
{
    internal enum WindowType
    {
        Pasture = -1,
        Canyon,
        Snowy,
        Flowery,
        Beach,
        HotSprings,
        BrokenScreen
    }

    internal class WindowInfo
    {
        internal float lightTemp = 8335f;
        internal Color filterColor = Color.white;
        internal bool blackWhenOff;
    }
}
