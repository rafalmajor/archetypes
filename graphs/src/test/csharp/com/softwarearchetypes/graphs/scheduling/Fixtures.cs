namespace com.softwarearchetypes.graphs.scheduling;

internal static class Fixtures
{
    internal static readonly ProcessStep DRYING = new("Drying");
    internal static readonly ProcessStep CALIBRATION = new("Calibration");
    internal static readonly ProcessStep MEASUREMENT = new("Measurement");
    internal static readonly ProcessStep SPECTROSCOPY = new("Spectroscopy");
    internal static readonly ProcessStep ANALYSIS = new("Analysis");
    internal static readonly ProcessStep VALIDATION = new("Validation");
    internal static readonly ProcessStep PREPARATION = new("Preparation");
    internal static readonly ProcessStep FINALIZATION = new("Finalization");
    internal static readonly ProcessStep STEP_1 = new("Step 1");
    internal static readonly ProcessStep STEP_2 = new("Step 2");
    internal static readonly ProcessStep STEP_3 = new("Step 3");
    internal static readonly ProcessStep PATH_1 = new("Path 1");
    internal static readonly ProcessStep PATH_2 = new("Path 2");
}