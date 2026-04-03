using System;

namespace Boards_WP
{
    public static class TokenEvents
    {
        public static event Action<int>? TokensUpdated;

        public static void NotifyTokensUpdated(int newAmount)
        {
            TokensUpdated?.Invoke(newAmount);
        }
    }
}