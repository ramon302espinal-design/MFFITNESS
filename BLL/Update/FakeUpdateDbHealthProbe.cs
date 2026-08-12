namespace BLL.Update
{
    public sealed class FakeUpdateDbHealthProbe : IUpdateDbHealthProbe
    {
        public bool ShouldSucceed { get; set; } = true;
        public int SchemaVersion { get; set; } = 1;
        public bool ForcePending { get; set; }
        public string FailMessage { get; set; } = "DB health FAIL (fake).";
        public int CallCount { get; private set; }

        public UpdateDbHealthProbeResult Probe(int targetDbVersion, string? migrationsDirectory)
        {
            CallCount++;
            if (!ShouldSucceed)
            {
                return new UpdateDbHealthProbeResult
                {
                    Success = false,
                    SqlConnected = true,
                    SchemaVersionExists = true,
                    SchemaVersion = SchemaVersion,
                    MatchesTarget = SchemaVersion == targetDbVersion,
                    NoPendingUntilTarget = !ForcePending,
                    IntegrityQueryOk = true,
                    Message = FailMessage
                };
            }

            bool matches = SchemaVersion == targetDbVersion && !ForcePending;
            return new UpdateDbHealthProbeResult
            {
                Success = matches,
                SqlConnected = true,
                SchemaVersionExists = true,
                SchemaVersion = SchemaVersion,
                MatchesTarget = SchemaVersion == targetDbVersion,
                NoPendingUntilTarget = !ForcePending,
                IntegrityQueryOk = true,
                Message = matches ? "DB health OK (fake)." : FailMessage
            };
        }
    }
}
