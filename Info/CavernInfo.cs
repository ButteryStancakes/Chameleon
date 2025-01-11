using UnityEngine;

namespace Chameleon.Info
{
    internal enum CavernType
    {
        Vanilla = -1,
        Ice,
        Amethyst,
        Desert,
        Mesa,
        Gravel
    }

    internal class CavernInfo
    {
        internal string tag = null;
        internal bool waterColor = false;
        internal Color waterColor1 = new(0.3018868f, 0.24540168f, 0.22926308f, 0.972549f), waterColor2 = new(0.254717f, 0.2132654f, 0.17181382f, 0.9882353f);
        internal bool noDrips = false;
    }
}
