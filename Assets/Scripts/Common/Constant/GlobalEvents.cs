using Framework.Core.Event;

namespace Common.Constant
{
    public static class GlobalEvents
    {
        public static EventIdentity OnLanguageChanged = new("OnLanguageChanged");
        public static EventIdentity OnFontSizeChanged = new("OnFontSizeChanged");
        public static EventIdentity OnOptionSelected = new("OnOptionSelected");
        public static EventIdentity OnSettingsSaved = new("OnSettingsSaved");
        public static EventIdentity OnArchiveSlotsChanged = new("OnArchiveSlotsChanged");
        public static EventIdentity OnArchiveLoaded = new("OnArchiveLoaded");
        
        public static EventIdentity OnRebindStarted = new("OnRebindStarted");
        public static EventIdentity OnRebindCompleted = new("OnRebindCompleted");
        
        public static EventIdentity OnCharacterAudioPlay =  new("OnCharacterAudioPlay");
        public static EventIdentity OnGameSceneLoaded = new("OnSceneLoaded");
        
        public static EventIdentity OnWitchTimeRequested = new("OnWitchTimeRequested");
        public static EventIdentity OnWitchTimeStarted = new("OnWitchTimeStarted");
        public static EventIdentity OnWitchTimeUpdated = new("OnWitchTimeUpdated");
        public static EventIdentity OnWitchTimeStopped = new("OnWitchTimeStopped");
    }
}
