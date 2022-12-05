// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("jqx9HNQLPPK3yER/N1M+/FkwGq2oQwHHvc6HZh6WrPLxXxZC6z1lD+9KsNUmatvXFBhZcMpvEV2JlBpFEEXSiMk2K+x0RlAiLRdf+9Bxu050vGal5bVU0p2L6Uw068ukTKKWporjBtB7d46uNrH5A6r61+/wBwxjJQp/ZZEfjxkFqIjuEI6z9RazNrJ/jChpvu34qTkg61rdcjtO1eQPE+FibGNT4WJpYeFiYmP2HXPPf5MrT9U//wcySv54qR61VuASz/NmVba9U16Ia2JJGymWTzsb6dDg7Q+GAIbm77ppnPhCxi/dr8TVit4V9mRp+6qKUilXUOn6XDrHCZGMmOQouVpT4WJBU25laknlK+WUbmJiYmZjYCKNHBbSn+rjumFgYmNi");
        private static int[] order = new int[] { 2,6,8,4,7,13,13,9,13,12,12,11,12,13,14 };
        private static int key = 99;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
