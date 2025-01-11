namespace Chameleon.Info
{
    internal class Queries
    {
        internal static bool IsSnowLevel()
        {
            return StartOfRound.Instance.currentLevel.levelIncludesSnowFootprints && (Common.artificeBlizzard == null || Common.artificeBlizzard.activeSelf);
        }

        internal static bool IsCameraInside()
        {
            if (GameNetworkManager.Instance.localPlayerController == null)
                return false;

            if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                return GameNetworkManager.Instance.localPlayerController.isInsideFactory;

            return GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null && GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInsideFactory;
        }
    }
}
