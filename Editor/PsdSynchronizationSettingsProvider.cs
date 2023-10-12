using UnityEditor;
using UnityEngine.UIElements;

namespace com.utkaka.PsdSynchronization.Editor { 
	public class PsdSynchronizationSettingsProvider : SettingsProvider {
		public enum LoggerType {
			None = 0,
			Console = 1
		}

		private const string PrefsSettingsPrefix = "PsdSynchronization";
		private const string PrefsLoggerTypeKey = "LoggerType"; 
		
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider() {
			var provider = new PsdSynchronizationSettingsProvider("Project/Psd Synchronization Settings", SettingsScope.Project);
			return provider;
		}

		public static LoggerType GetLoggerType() {
			return EditorPrefs.HasKey(GetPrefsSettingFullKey(PrefsLoggerTypeKey))
				? (LoggerType) EditorPrefs.GetInt(GetPrefsSettingFullKey(PrefsLoggerTypeKey))
				: LoggerType.None;
		}
		
		private static string GetPrefsSettingFullKey(string settingKey) {
			return $"{PrefsSettingsPrefix}.{settingKey}";
		}

		private LoggerType _loggerType;

		private PsdSynchronizationSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
			: base(path, scope) {
		}

		public override void OnActivate(string searchContext, VisualElement rootElement) {
			_loggerType = GetLoggerType();
		}

		public override void OnGUI(string searchContext) {
			EditorGUI.BeginChangeCheck();
			_loggerType = (LoggerType)EditorGUILayout.EnumPopup("Logs", _loggerType);
			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetInt(GetPrefsSettingFullKey(PrefsLoggerTypeKey), (int)_loggerType);
			}
		}
	}
}