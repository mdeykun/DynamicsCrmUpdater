namespace Cwru.Common
{
    public static class Info
    {
        static Info()
        {
            Version = "0.06.14";
            UsdtErc20 = "0xeCBe23D390269f66b9DB27606d3666Be269219d9";
            UsdtTrc20 = "TNoKzwHHbjX6Czp1p19zSwZa7iqftUSHdt";
            Btc = "1PL3HhYFvaqCUDjYaCMuxAsPPukoJCY1j6";
            EthErc20 = "0xeCBe23D390269f66b9DB27606d3666Be269219d9";
            OpenBugUrl = $"https://github.com/mdeykun/DynamicsCrmUpdater/issues/new?body=%0A%0A%0A---%0Atool%20version:%20{Version}";
        }

        public static string Version { get; }

        public static string UsdtErc20 { get; }

        public static string UsdtTrc20 { get; }

        public static string Btc { get; }

        public static string EthErc20 { get; }

        public static string OpenBugUrl { get; }
    }
}
