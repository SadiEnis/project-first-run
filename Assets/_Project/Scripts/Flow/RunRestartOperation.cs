using System;

namespace ProjectFirstRun.Flow
{
    public enum RunRestartState { Idle, Loading, Failed }
    public enum RunRestartSource { Pause, Death, Completion, FailedEncounter }

    public interface IRunSceneReloader
    {
        // False/exception means loading never started. Success transfers ownership to Unity.
        bool Begin(string scenePath);
    }

    public sealed class RunRestartOperation
    {
        public RunRestartState State { get; private set; }
        public Exception Error { get; private set; }

        public bool TryStart(bool eligible, string scenePath, IRunSceneReloader reloader)
        {
            if (!eligible || State == RunRestartState.Loading) return false;
            State = RunRestartState.Loading;
            Error = null;
            try
            {
                if (string.IsNullOrWhiteSpace(scenePath)) throw new InvalidOperationException("Run scene has no saved path.");
                if (reloader == null || !reloader.Begin(scenePath))
                    throw new InvalidOperationException("Run scene reload did not start.");
                return true;
            }
            catch (Exception error)
            {
                Error = error;
                State = RunRestartState.Failed;
                return false;
            }
        }
    }
}
