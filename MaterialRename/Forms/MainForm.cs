using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace MaterialRename.Forms;
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        this.Text = $"MaterialRename {informationVersion}";

        Application.DoEvents();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {

    }
}
