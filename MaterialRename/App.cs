using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace MaterialRename;
public class App : IExternalApplication
{
    // get the absolute path of this assembly
    public static readonly string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

    // class instance
    public static App ThisApp;

    public static UIControlledApplication CachedUiCtrApp;
    public static UIApplication CachedUiApp;
    public static ControlledApplication CtrApp;
    public static Autodesk.Revit.DB.Document RevitDocument;

    private AppDocEvents _appEvents;
    
    public Result OnStartup(UIControlledApplication application)
    {
        ThisApp = this;
        CachedUiCtrApp = application;
        CtrApp = application.ControlledApplication;

        var panel = RibbonPanel(application);

        AddAppDocEvents();

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        RemoveAppDocEvents();

        return Result.Succeeded;
    }

    #region Event Handling
    private void AddAppDocEvents()
    {
        _appEvents = new AppDocEvents();
        _appEvents.EnableEvents();
    }
    private void RemoveAppDocEvents()
    {
        _appEvents.DisableEvents();
    }


    #endregion

    #region Ribbon Panel

    private RibbonPanel RibbonPanel(UIControlledApplication application)
    {

        RibbonPanel panel = CachedUiCtrApp.CreateRibbonPanel("MaterialRename_Panel");
        panel.Title = "MaterialRename";

        PushButton button = (PushButton)panel.AddItem(
            new PushButtonData(
                "Command",
                "Command",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(MaterialRename)}.{nameof(MaterialRename.Command)}"));
        button.ToolTip = "Execute the MaterialRename command";
        button.LargeImage = PngImageSource("MaterialRename.Resources.MaterialRename_Button.png");

        return panel;
    }

    private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
    {
        var stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
        System.Windows.Media.ImageSource imageSource;
        try
        {
            imageSource = BitmapFrame.Create(stream);
        }
        catch
        {
            imageSource = null;
        }

        return imageSource;
    }
    #endregion
}
