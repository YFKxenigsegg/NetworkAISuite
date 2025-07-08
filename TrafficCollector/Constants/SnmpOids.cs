namespace TrafficCollector.Constants;

public static class SnmpOids
{
    // System Information
    public static class System
    {
        public const string Description = "1.3.6.1.2.1.1.1.0";
        public const string ObjectId = "1.3.6.1.2.1.1.2.0";
        public const string UpTime = "1.3.6.1.2.1.1.3.0";
        public const string Contact = "1.3.6.1.2.1.1.4.0";
        public const string Name = "1.3.6.1.2.1.1.5.0";
        public const string Location = "1.3.6.1.2.1.1.6.0";
        public const string Services = "1.3.6.1.2.1.1.7.0";
    }

    // Interface Statistics
    public static class Interface
    {
        public const string Number = "1.3.6.1.2.1.2.1.0";
        public const string Index = "1.3.6.1.2.1.2.2.1.1";
        public const string Description = "1.3.6.1.2.1.2.2.1.2";
        public const string Type = "1.3.6.1.2.1.2.2.1.3";
        public const string Mtu = "1.3.6.1.2.1.2.2.1.4";
        public const string Speed = "1.3.6.1.2.1.2.2.1.5";
        public const string PhysAddress = "1.3.6.1.2.1.2.2.1.6";
        public const string AdminStatus = "1.3.6.1.2.1.2.2.1.7";
        public const string OperStatus = "1.3.6.1.2.1.2.2.1.8";
        public const string LastChange = "1.3.6.1.2.1.2.2.1.9";
        public const string InOctets = "1.3.6.1.2.1.2.2.1.10";
        public const string InUcastPkts = "1.3.6.1.2.1.2.2.1.11";
        public const string InNUcastPkts = "1.3.6.1.2.1.2.2.1.12";
        public const string InDiscards = "1.3.6.1.2.1.2.2.1.13";
        public const string InErrors = "1.3.6.1.2.1.2.2.1.14";
        public const string InUnknownProtos = "1.3.6.1.2.1.2.2.1.15";
        public const string OutOctets = "1.3.6.1.2.1.2.2.1.16";
        public const string OutUcastPkts = "1.3.6.1.2.1.2.2.1.17";
        public const string OutNUcastPkts = "1.3.6.1.2.1.2.2.1.18";
        public const string OutDiscards = "1.3.6.1.2.1.2.2.1.19";
        public const string OutErrors = "1.3.6.1.2.1.2.2.1.20";
    }

    // TCP Statistics
    public static class Tcp
    {
        public const string RtoAlgorithm = "1.3.6.1.2.1.6.1.0";
        public const string RtoMin = "1.3.6.1.2.1.6.2.0";
        public const string RtoMax = "1.3.6.1.2.1.6.3.0";
        public const string MaxConn = "1.3.6.1.2.1.6.4.0";
        public const string ActiveOpens = "1.3.6.1.2.1.6.5.0";
        public const string PassiveOpens = "1.3.6.1.2.1.6.6.0";
        public const string AttemptFails = "1.3.6.1.2.1.6.7.0";
        public const string EstabResets = "1.3.6.1.2.1.6.8.0";
        public const string CurrEstab = "1.3.6.1.2.1.6.9.0";
        public const string InSegs = "1.3.6.1.2.1.6.10.0";
        public const string OutSegs = "1.3.6.1.2.1.6.11.0";
        public const string RetransSegs = "1.3.6.1.2.1.6.12.0";
        public const string InErrs = "1.3.6.1.2.1.6.14.0";
        public const string OutRsts = "1.3.6.1.2.1.6.15.0";
    }

    // UDP Statistics
    public static class Udp
    {
        public const string InDatagrams = "1.3.6.1.2.1.7.1.0";
        public const string NoPorts = "1.3.6.1.2.1.7.2.0";
        public const string InErrors = "1.3.6.1.2.1.7.3.0";
        public const string OutDatagrams = "1.3.6.1.2.1.7.4.0";
    }

    // ICMP Statistics
    public static class Icmp
    {
        public const string InMsgs = "1.3.6.1.2.1.5.1.0";
        public const string InErrors = "1.3.6.1.2.1.5.2.0";
        public const string InDestUnreachs = "1.3.6.1.2.1.5.3.0";
        public const string InTimeExcds = "1.3.6.1.2.1.5.4.0";
        public const string InParmProbs = "1.3.6.1.2.1.5.5.0";
        public const string InSrcQuenchs = "1.3.6.1.2.1.5.6.0";
        public const string InRedirects = "1.3.6.1.2.1.5.7.0";
        public const string InEchos = "1.3.6.1.2.1.5.8.0";
        public const string InEchoReps = "1.3.6.1.2.1.5.9.0";
        public const string OutMsgs = "1.3.6.1.2.1.5.14.0";
        public const string OutErrors = "1.3.6.1.2.1.5.15.0";
    }

    // IP Statistics
    public static class Ip
    {
        public const string Forwarding = "1.3.6.1.2.1.4.1.0";
        public const string DefaultTTL = "1.3.6.1.2.1.4.2.0";
        public const string InReceives = "1.3.6.1.2.1.4.3.0";
        public const string InHdrErrors = "1.3.6.1.2.1.4.4.0";
        public const string InAddrErrors = "1.3.6.1.2.1.4.5.0";
        public const string ForwDatagrams = "1.3.6.1.2.1.4.6.0";
        public const string InUnknownProtos = "1.3.6.1.2.1.4.7.0";
        public const string InDiscards = "1.3.6.1.2.1.4.8.0";
        public const string InDelivers = "1.3.6.1.2.1.4.9.0";
        public const string OutRequests = "1.3.6.1.2.1.4.10.0";
        public const string OutDiscards = "1.3.6.1.2.1.4.11.0";
        public const string OutNoRoutes = "1.3.6.1.2.1.4.12.0";
    }
}
