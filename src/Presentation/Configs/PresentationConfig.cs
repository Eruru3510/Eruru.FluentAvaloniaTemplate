namespace Presentation.Configs;

public class PresentationConfig {

	public string? AppLanguage { get; set; }
	public string? AppFont { get; set; }
	public string? AppTheme { get; set; }
	public string? AppFlowDirection { get; set; }
	public bool? IsAppAccentColorExpanded { get; set; }
	public bool? IsUseCustomAccentColor { get; set; }
	public ColorConfig? CustomAccentColor { get; set; }
	public bool? IsAboutExpanded { get; set; }
	public bool? IsNavigationExpanded { get; set; }
	public string? StartPage { get; set; }
	public string? CloseWindowBehaviour { get; set; }
	public string? WindowState { get; set; }
	public WindowSize? WindowSize { get; set; }
	public string? AppStyle { get; set; }

}