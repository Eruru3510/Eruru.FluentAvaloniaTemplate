using Avalonia.Media;
using Presentation.Configs;
using Presentation.Navigations;
using Riok.Mapperly.Abstractions;

namespace Presentation.Mapper;

[Mapper]
public partial class PresentationMapper {

	public Color ToColor (ColorConfig colorConfig) {
		ArgumentNullException.ThrowIfNull (colorConfig);
		return Color.FromArgb (colorConfig.A, colorConfig.R, colorConfig.G, colorConfig.B);
	}

	public partial ColorConfig ToColorConfig (Color color);

	[MapperIgnoreSource (nameof (NavigationEntry.ParentId))]
	[MapperIgnoreTarget (nameof (NavigationItem.Parent))]
	public partial NavigationItem ToNavigationItem (NavigationEntry navigationEntry);

}